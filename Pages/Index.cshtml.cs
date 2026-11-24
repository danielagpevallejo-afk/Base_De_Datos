using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

    public async Task OnGetAsync() => await LoadStatsAsync();

    public async Task OnPostAsync()
    {
        Resultado = await automata.AnalizarAsync(Cadena);
        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        TotalEstados = await db.Estados.CountAsync();
        TotalTransiciones = await db.Transiciones.CountAsync();
        Proveedor = configuration["DatabaseProvider"] ?? "Sqlite";
    }
}
