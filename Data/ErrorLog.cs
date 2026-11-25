namespace TiendaProyecto.Data;
public static class ErrorLog
{
    private static readonly object Candado = new();
    public static void Registrar(Exception ex)
    {
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            var ruta = Path.Combine(dir, "errores_sistema.log");
            lock (Candado)
            {
                File.AppendAllText(ruta,
                    $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}---{Environment.NewLine}");
            }
            Console.Error.WriteLine($"[{DateTimeOffset.Now:O}] {ex.GetType().Name}: {ex.Message}");
        }
        catch (Exception falloLog)
        {
            Console.Error.WriteLine($"No se pudo escribir el log: {falloLog.Message}; error original: {ex.Message}");
        }
    }
}
