using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using OllinLexer.Data;
using OllinLexer.Services;

namespace OllinLexer.Pages;

public class IndexModel : PageModel
{
    private readonly AutomataService automata;
    private readonly AutomataDbContext db;
    private readonly IConfiguration configuration;

    public IndexModel(AutomataService automata, AutomataDbContext db, IConfiguration configuration)
    {
        this.automata = automata;
        this.db = db;
        this.configuration = configuration;
    }

    [BindProperty]
    public string Cadena { get; set; } = "";

    public AnalysisResult? Resultado { get; private set; }
    public int TotalEstados { get; private set; }
    public int TotalTransiciones { get; private set; }
    public string Proveedor { get; private set; } = "";
    public string? ArchivoMensaje { get; private set; }

    public async Task OnGetAsync() => await LoadStatsAsync();

    public async Task OnPostAsync()
    {
        Resultado = await automata.AnalizarAsync(Cadena);
        await LoadStatsAsync();
    }

    public async Task<IActionResult> OnPostDescargarTokensAsync()
    {
        Resultado = await automata.AnalizarAsync(Cadena);
        if (!Resultado.Valido)
        {
            ArchivoMensaje = "No se puede guardar el archivo de tokens mientras el programa tenga errores léxicos.";
            await LoadStatsAsync();
            return Page();
        }

        var output = new StringBuilder();
        output.AppendLine("ARCHIVO DE TOKENS - LENGUAJE OLLIN");
        output.AppendLine("===================================");
        foreach (var line in Resultado.Tokens.GroupBy(token => token.Linea).OrderBy(group => group.Key))
            output.AppendLine($"{line.Key}\t{string.Join(" ", line.Select(token => token.Codigo))}");

        output.AppendLine();
        output.AppendLine("TABLA DE SÍMBOLOS");
        output.AppendLine("ID\tNOMBRE\tTIPO DE DATO\tVALOR");
        foreach (var symbol in Resultado.Simbolos)
            output.AppendLine($"{symbol.Identificador}\t{symbol.Nombre}\t{symbol.TipoDato}\t{symbol.Valor}");

        return File(Encoding.UTF8.GetBytes(output.ToString()), "text/plain; charset=utf-8", "archivo_tokens.txt");
    }

    public async Task<IActionResult> OnPostDescargarProgramaAsync()
    {
        Resultado = await automata.AnalizarAsync(Cadena);
        if (!Resultado.Valido)
        {
            ArchivoMensaje = "No se puede guardar el programa fuente mientras existan errores léxicos.";
            await LoadStatsAsync();
            return Page();
        }

        return File(Encoding.UTF8.GetBytes(Cadena), "text/plain; charset=utf-8", "programa_ollin.txt");
    }

    private async Task LoadStatsAsync()
    {
        TotalEstados = await db.Estados.CountAsync();
        TotalTransiciones = await db.Transiciones.CountAsync();
        Proveedor = configuration["DatabaseProvider"] ?? "Sqlite";
    }
}
