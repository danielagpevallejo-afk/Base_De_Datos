using Microsoft.EntityFrameworkCore;
using OllinLexer.Data;
using OllinLexer.Models;

namespace OllinLexer.Services;

public sealed class AutomataService
{
    private readonly AutomataDbContext db;
    private const string InitialState = "q0";
    private const string ErrorState = "q179";

    public AutomataService(AutomataDbContext db) => this.db = db;

    public async Task<ValidationResult> ValidarAsync(string? input)
    {
        input ??= "";
        var automaton = await LoadAutomatonAsync();
        return ValidateLexeme(input, automaton.States, automaton.Graph);
    }

    public async Task<AnalysisResult> AnalizarAsync(string? source)
    {
        source ??= "";
        var automaton = await LoadAutomatonAsync();
        var tokens = new List<TokenAnalysis>();
        var symbols = new List<SymbolEntry>();
        var errors = new List<LexicalError>();
        var symbolIds = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var lexeme in Tokenize(source))
        {
            var result = ValidateLexeme(lexeme.Text, automaton.States, automaton.Graph);
            var code = result.Token;

            if (result.Valida && result.Token == "IDENTIFICADOR")
            {
                if (!symbolIds.TryGetValue(lexeme.Text, out code))
                {
                    code = $"ID{symbolIds.Count + 1:00}";
                    symbolIds.Add(lexeme.Text, code);
                    symbols.Add(new SymbolEntry(code, lexeme.Text, "—", "—", lexeme.Line));
                }
            }

            var token = new TokenAnalysis(
                tokens.Count + 1,
                lexeme.Line,
                lexeme.Column,
                lexeme.Text,
                result.Valida,
                result.Categoria,
                result.Valida ? code : "ERROR",
                result.Recorrido,
                result.Pasos);
            tokens.Add(token);

            if (!result.Valida)
            {
                errors.Add(new LexicalError(
                    lexeme.Line,
                    lexeme.Column,
                    lexeme.Text,
                    ErrorDescription(lexeme.Text, result)));
            }
        }

        var valid = errors.Count == 0 && tokens.Count > 0;
        var message = tokens.Count == 0
            ? "Escribe al menos un token para comenzar el análisis."
            : errors.Count == 0
                ? $"Análisis completado: {tokens.Count} token(es) reconocidos."
                : $"Análisis terminado con {errors.Count} error(es) léxico(s); los demás tokens sí fueron procesados.";

        return new AnalysisResult(source, valid, message, tokens, symbols, errors);
    }

    private async Task<AutomatonData> LoadAutomatonAsync()
    {
        var states = (await db.Estados.AsNoTracking().ToListAsync())
            .ToDictionary(e => e.Id, StringComparer.Ordinal);
        var transitionRows = await db.Transiciones.AsNoTracking().Include(t => t.Simbolo).ToListAsync();
        var graph = transitionRows.GroupBy(t => (t.EstadoOrigenId, t.Simbolo.Valor))
            .ToDictionary(g => g.Key, g => g.Select(t => t.EstadoDestinoId).Distinct(StringComparer.Ordinal).ToArray());
        return new AutomatonData(states, graph);
    }

    private static ValidationResult ValidateLexeme(
        string input,
        IReadOnlyDictionary<string, Estado> states,
        IReadOnlyDictionary<(string State, string Symbol), string[]> graph)
    {

        var active = new HashSet<string>(StringComparer.Ordinal) { InitialState };
        var steps = new List<ValidationStep>();
        int? errorPosition = null;

        for (var index = 0; index < input.Length; index++)
        {
            var before = FormatStates(active);
            var character = input[index];
            active = Advance(active, InputSymbols(character), graph);
            if (active.SetEquals(new[] { ErrorState }) && errorPosition is null) errorPosition = index + 1;
            steps.Add(new ValidationStep(index + 1, DisplayCharacter(character), CharacterType(character), before, FormatStates(active)));
        }

        var beforeFdc = FormatStates(active);
        active = Advance(active, new[] { "FDC" }, graph);
        steps.Add(new ValidationStep(input.Length + 1, "FDC", "Fin de cadena", beforeFdc, FormatStates(active)));

        var accepted = active.Where(id => states.TryGetValue(id, out var state) && state.Tipo == "Final")
            .Select(id => states[id])
            .OrderByDescending(state => Priority(state.Categoria))
            .ThenBy(state => StateNumber(state.Id))
            .FirstOrDefault();

        var valid = accepted is not null;
        var token = accepted?.Categoria ?? "ERROR";
        var category = valid ? GeneralCategory(token) : "Cadena no válida";
        var message = valid
            ? $"{input}: VÁLIDO — {category} — {token}"
            : errorPosition is int position
                ? $"{input}: NO VÁLIDO (falla en la posición {position})"
                : $"{input}: NO VÁLIDO (no termina en un estado de aceptación)";
        var route = InitialState + " → " + string.Join(" → ", steps.Select(s => s.EstadoNuevo));
        return new ValidationResult(input, valid, category, token, message, route, errorPosition, steps);
    }

    private static IEnumerable<SourceLexeme> Tokenize(string source)
    {
        var index = 0;
        var line = 1;
        var column = 1;
        var pairedSymbols = new HashSet<string>(StringComparer.Ordinal) { ">=", "<=", "<>", "==", "//" };
        while (index < source.Length)
        {
            var character = source[index];
            if (character == '\r')
            {
                index++;
                if (index < source.Length && source[index] == '\n') index++;
                line++;
                column = 1;
                continue;
            }
            if (character == '\n')
            {
                index++;
                line++;
                column = 1;
                continue;
            }
            if (char.IsWhiteSpace(character))
            {
                index++;
                column++;
                continue;
            }

            var start = index;
            var startColumn = column;
            if (IsWordCharacter(character))
            {
                while (index < source.Length && IsWordCharacter(source[index]))
                {
                    index++;
                    column++;
                }
            }
            else if (index + 1 < source.Length && pairedSymbols.Contains(source.Substring(index, 2)))
            {
                index += 2;
                column += 2;
            }
            else
            {
                index++;
                column++;
            }

            var text = source.Substring(start, index - start);
            yield return new SourceLexeme(text, line, startColumn);
        }
    }

    private static bool IsWordCharacter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';

    private static string ErrorDescription(string lexeme, ValidationResult result)
    {
        if (lexeme.All(char.IsDigit))
            return "Constante numérica no reconocida por el autómata actual.";
        if (lexeme.Length > 0 && char.IsDigit(lexeme[0]))
            return "Un identificador no puede comenzar con un dígito.";
        return result.PosicionError is null
            ? "El token no termina en un estado de aceptación."
            : $"Token no reconocido; falla en su carácter {result.PosicionError}.";
    }

    private static HashSet<string> Advance(IEnumerable<string> active, IEnumerable<string> symbols,
        IReadOnlyDictionary<(string State, string Symbol), string[]> graph)
    {
        var next = new HashSet<string>(StringComparer.Ordinal);
        foreach (var state in active)
        foreach (var symbol in symbols)
            if (graph.TryGetValue((state, symbol), out var destinations)) next.UnionWith(destinations);
        if (next.Count == 0) next.Add(ErrorState);
        if (next.Count > 1 && next.Contains(ErrorState)) next.Remove(ErrorState);
        return next;
    }

    private static IEnumerable<string> InputSymbols(char character)
    {
        if (character is >= 'A' and <= 'Z') return new[] { character.ToString(), "LET" };
        if (character is >= '0' and <= '9') return new[] { "DIG" };
        return new[] { character.ToString() };
    }

    private static string CharacterType(char character) => character switch
    {
        >= 'A' and <= 'Z' => "Letra", >= '0' and <= '9' => "Dígito", '_' => "Guion bajo",
        _ when "><=+-*/;@#$%^&(){}]:\"'.?".Contains(character) => "Símbolo", _ => "Otro"
    };

    private static string DisplayCharacter(char character) => character == ' ' ? "␠" : character.ToString();
    private static int StateNumber(string state) => int.TryParse(state.AsSpan(1), out var number) ? number : int.MaxValue;
    private static string FormatStates(IEnumerable<string> states) => string.Join(", ", states.OrderBy(StateNumber));
    private static int Priority(string? token) => token switch
    {
        null => 0, _ when token.StartsWith("PR_", StringComparison.Ordinal) => 100, "IDENTIFICADOR" => 10, _ => 50
    };
    private static string GeneralCategory(string token) => token switch
    {
        "PR_AND" or "PR_NOT" or "PR_OR" => "Operador lógico",
        "PR_TRUE" or "PR_FALSE" => "Constante lógica",
        _ when token.StartsWith("PR_", StringComparison.Ordinal) => "Palabra reservada",
        _ when token.StartsWith("REL_", StringComparison.Ordinal) || token == "ASIGNACION" => "Operador relacional",
        _ when token.StartsWith("OPA", StringComparison.Ordinal) => "Operador aritmético",
        _ when token.StartsWith("CE", StringComparison.Ordinal) => "Carácter especial",
        "IDENTIFICADOR" => "Identificador", "COMENTARIO" => "Comentario", _ => token
    };

    private sealed record SourceLexeme(string Text, int Line, int Column);
    private sealed record AutomatonData(
        IReadOnlyDictionary<string, Estado> States,
        IReadOnlyDictionary<(string State, string Symbol), string[]> Graph);
}
