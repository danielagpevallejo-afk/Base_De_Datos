using System.Text;
using Microsoft.EntityFrameworkCore;
using OllinLexer.Models;

namespace OllinLexer.Data;

public static class DatabaseSeeder
{
    private static readonly HashSet<string> ReservedWords = new(StringComparer.Ordinal)
    {
        "AND", "BREAK", "CLEAN", "CONTINUE", "DECIMAL", "DO", "END", "ENDFOR", "ENDIF", "ENDWHILE",
        "FALSE", "FOR", "FUNCTION", "IF", "INTEGER", "LOCATE", "NOT", "OR", "ORIF", "OTHERWISE", "PRINT",
        "READ", "READ_FILE", "RETURN", "SHOW_MESSAGE", "START", "STRING", "TRUE", "VAR", "WHILE", "WRITE_FILE"
    };

    private static readonly Dictionary<string, string> CategoryNames = new(StringComparer.Ordinal)
    {
        ["IDENTIFICADOR"] = "IDENTIFICADOR", ["ERROR"] = "ERROR",
        [">"] = "REL_MAYOR", [">="] = "REL_MAYOR_IGUAL", ["<"] = "REL_MENOR", ["<="] = "REL_MENOR_IGUAL",
        ["<>"] = "REL_DIFERENTE", ["="] = "ASIGNACION", ["=="] = "REL_IGUAL",
        ["OPA1"] = "OPA1_SUMA", ["OPA2"] = "OPA2_RESTA", ["OPA3"] = "OPA3_MULTIPLICACION", ["OPA4"] = "OPA4_DIVISION",
        ["CMT"] = "COMENTARIO", ["CE1"] = "CE1_PUNTO_COMA", ["CE2"] = "CE2_ARROBA", ["CE3"] = "CE3_NUMERAL",
        ["CE4"] = "CE4_DOLAR", ["CE5"] = "CE5_PORCENTAJE", ["CE6"] = "CE6_POTENCIA", ["CE7"] = "CE7_AMPERSAND",
        ["CE8"] = "CE8_ASTERISCO", ["CE9"] = "CE9_PARENTESIS_ABRE", ["CE10"] = "CE10_PARENTESIS_CIERRA",
        ["CE11"] = "CE11_LLAVE_ABRE", ["CE_CORCHETE"] = "CE_CORCHETE_CIERRA", ["CE12"] = "CE12_LLAVE_CIERRA",
        ["CE13"] = "CE13_DOS_PUNTOS", ["CE14"] = "CE14_COMILLAS_DOBLES", ["CE15"] = "CE15_COMILLA_SIMPLE",
        ["CE16"] = "CE16_PUNTO", ["CE17"] = "CE17_INTERROGACION"
    };

    public static async Task InitializeAsync(AutomataDbContext db, string contentRoot)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Estados.AnyAsync()) return;

        var csvPath = Path.Combine(contentRoot, "Data", "Seed", "transiciones.csv");
        var rows = File.ReadLines(csvPath, Encoding.UTF8).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseCsvLine)
            .Select(c => new SeedRow(c[0], c[1], c[2], c.Count > 3 ? c[3] : ""))
            .ToList();

        var categoryByState = rows.Where(r => !string.IsNullOrWhiteSpace(r.Token))
            .GroupBy(r => r.To)
            .ToDictionary(g => g.Key, g => CategoryFor(g.Last().Token), StringComparer.Ordinal);

        db.Estados.AddRange(Enumerable.Range(0, 240).Select(number =>
        {
            var id = $"q{number}";
            var type = id == "q0" ? "Inicial" : id == "q179" ? "Error" : categoryByState.ContainsKey(id) ? "Final" : "Intermedio";
            return new Estado { Id = id, Tipo = type, Categoria = categoryByState.GetValueOrDefault(id) };
        }));

        var expanded = rows.SelectMany(row => ExpandSymbols(row.Symbol)
            .Select(symbol => new SeedRow(row.From, symbol, row.To, row.Token))).ToList();
        var symbols = expanded.Select(r => r.Symbol).Distinct(StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal)
            .Select(value => new Simbolo { Valor = value }).ToList();
        db.Simbolos.AddRange(symbols);
        await db.SaveChangesAsync();

        var symbolIds = symbols.ToDictionary(s => s.Valor, s => s.Id, StringComparer.Ordinal);
        db.Transiciones.AddRange(expanded.Select(r => new Transicion
        {
            EstadoOrigenId = r.From, SimboloId = symbolIds[r.Symbol], EstadoDestinoId = r.To
        }));
        await db.SaveChangesAsync();
    }

    private static IEnumerable<string> ExpandSymbols(string value) => value switch
    {
        "LET,_" => new[] { "LET", "_" },
        "LET,DIG,FDC,_" => new[] { "LET", "DIG", "FDC", "_" },
        _ => new[] { value }
    };

    private static string CategoryFor(string token)
        => ReservedWords.Contains(token) ? $"PR_{token}" : CategoryNames.GetValueOrDefault(token, token);

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (quoted && ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { field.Append('"'); i++; }
            else if (ch == '"') quoted = !quoted;
            else if (ch == ',' && !quoted) { result.Add(field.ToString()); field.Clear(); }
            else field.Append(ch);
        }
        result.Add(field.ToString());
        return result;
    }

    private sealed record SeedRow(string From, string Symbol, string To, string Token);
}
