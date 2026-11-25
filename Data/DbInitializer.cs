using Npgsql;

namespace TiendaProyecto.Data;
public static class DbInitializer
{
    public static void Initialize(IConfiguration config, bool cargarDemo = false)
    {
        using var conexion = new NpgsqlConnection(config.GetConnectionString("Tienda"));
        conexion.Open();
        // SQL base de A5/A7 y migración incremental A8, sin eliminar ventas antiguas.
        foreach (var nombre in new[] { "00_base.sql", "01_cr01_descuentos.sql", "02_datos_demo.sql" })
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "Sql", nombre);
            var sql = File.ReadAllText(ruta);
            using var cmd = new NpgsqlCommand(sql, conexion);
            cmd.ExecuteNonQuery();
        }
        if (cargarDemo)
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Data", "Sql", "03_a10_demo.sql");
            using var cmd = new NpgsqlCommand(File.ReadAllText(ruta), conexion);
            cmd.ExecuteNonQuery();
        }
    }
}
