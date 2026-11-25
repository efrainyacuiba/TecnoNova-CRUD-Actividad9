using MysticMind.PostgresEmbed;
using Npgsql;
using TiendaProyecto.Data;

var verificar = args.Length > 0 && args[0] == "--verificar-sql";
var builder = WebApplication.CreateBuilder(verificar ? Array.Empty<string>() : args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ProductoDAO>();
builder.Services.AddScoped<VentaDAO>();
builder.Services.AddScoped<ReporteDAO>();
// Por defecto: PostgreSQL real, local y persistente. Para BD externa, cambiar
// BaseDatos:Modo a Externa y configurar ConnectionStrings:Tienda.
var integrada = !string.Equals(builder.Configuration["BaseDatos:Modo"], "Externa", StringComparison.OrdinalIgnoreCase);
await using var postgres = integrada ? new PgServer(new PgServerOptions
{
    PgVersion = "17.6.0", InstanceName = "tecnonova-actividad10",
    DbDir = builder.Environment.ContentRootPath,
    StartupTimeout = TimeSpan.FromSeconds(90),
    ServerParams = new Dictionary<string, string> { ["listen_addresses"] = "127.0.0.1" }
}) : null;
if (integrada)
{
    Console.WriteLine("Iniciando PostgreSQL local (primera vez descarga los binarios)...");
    try
    {
        await postgres!.StartAsync();
        builder.Configuration["ConnectionStrings:Tienda"] = postgres.ConnectionString;
        Console.WriteLine($"PostgreSQL iniciado en puerto {postgres.PgPort}.");
    }
    catch (Exception ex)
    {
        ErrorLog.Registrar(ex);
        Console.Error.WriteLine("No se pudo iniciar PostgreSQL. Comprueba internet y ejecuta sin administrador/root.");
        Environment.ExitCode = 1;
        return;
    }
}
var app = builder.Build();
try
{
    DbInitializer.Initialize(app.Configuration, integrada && !verificar);
    Console.WriteLine("Base de datos preparada.");
}
catch (Exception ex)
{
    ErrorLog.Registrar(ex);
    if (verificar || integrada) { Environment.ExitCode = 1; return; }
    app.Logger.LogWarning("PostgreSQL no disponible; se mostrarán avisos UX.");
}
if (verificar)
{
    if (args.Length != 3 || !DateOnly.TryParseExact(args[1], "yyyy-MM-dd", out var inicio) ||
        !DateOnly.TryParseExact(args[2], "yyyy-MM-dd", out var fin) || inicio > fin)
    {
        Console.WriteLine("Uso: dotnet run -- --verificar-sql 2026-11-24 2026-11-24");
        Environment.ExitCode = 1; return;
    }
    using var scope = app.Services.CreateScope();
    var dao = scope.ServiceProvider.GetRequiredService<ReporteDAO>();
    var sql = dao.VerificarTotal(inicio, fin);
    Console.WriteLine("PostgreSQL real: consulta independiente de validación");
    Console.WriteLine(ReporteDAO.SqlValidacion);
    Console.WriteLine($"@inicio = {inicio:yyyy-MM-dd}; @fin = {fin:yyyy-MM-dd}");
    Console.WriteLine($"Operaciones: {sql.Cantidad} | Descuentos: Bs {sql.Descuento:F2} | Total SQL: Bs {sql.Total:F2}");
    return;
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
await app.RunAsync();
