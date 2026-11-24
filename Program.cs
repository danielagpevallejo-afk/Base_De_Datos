using Microsoft.EntityFrameworkCore;
using OllinLexer.Data;
using OllinLexer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<AutomataService>();

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
builder.Services.AddDbContext<AutomataDbContext>(options =>
{
    if (databaseProvider.Equals("SqlServerLocalDb", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerLocalDb"));
    else
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AutomataDbContext>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await DatabaseSeeder.InitializeAsync(db, environment.ContentRootPath);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();
app.MapRazorPages();

app.MapGet("/api/validar/{cadena}", async (string cadena, AutomataService automata) =>
    Results.Ok(await automata.ValidarAsync(cadena)));

app.MapGet("/api/analizar", async (string fuente, AutomataService automata) =>
    Results.Ok(await automata.AnalizarAsync(fuente)));

app.Run();
