using Npgsql;
using TiendaProyecto.Models;

namespace TiendaProyecto.Data;
public class ReporteDAO
{
    private readonly string _cadena;
    public ReporteDAO(IConfiguration config) => _cadena = config.GetConnectionString("Tienda")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Tienda.");

    public List<AuditoriaDescuento> Auditoria()
    {
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand(@"
                SELECT vendedor, COUNT(*) AS ventas_con_descuento, SUM(monto_descuento) AS total_descontado
                FROM ventas WHERE porcentaje_descuento > 0 GROUP BY vendedor ORDER BY total_descontado DESC", conexion);
            using var r = cmd.ExecuteReader();
            var lista = new List<AuditoriaDescuento>();
            while (r.Read()) lista.Add(new AuditoriaDescuento
            {
                Vendedor = r.GetString(0), VentasConDescuento = r.GetInt64(1),
                SumaDescuentos = r.GetDecimal(2)
            });
            return lista;
        }
        catch (PostgresException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("No pudimos generar el informe. Revise la base de datos e inténtelo nuevamente.", ex); }
        catch (NpgsqlException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("El reporte está temporalmente indisponible porque PostgreSQL no responde. Intente más tarde.", ex); }
        catch (Exception ex) { ErrorLog.Registrar(ex); throw; }
    }

    public List<VentaResumen> UltimasVentas()
    {
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand(@"
                SELECT id,fecha,cliente,vendedor,subtotal_bruto,porcentaje_descuento,monto_descuento,total_final
                FROM ventas ORDER BY id DESC LIMIT 30", conexion);
            using var r = cmd.ExecuteReader();
            var lista = new List<VentaResumen>();
            while (r.Read()) lista.Add(new VentaResumen
            {
                Id = r.GetInt32(0), Fecha = r.GetDateTime(1), Cliente = r.GetString(2),
                Vendedor = r.GetString(3), SubtotalBruto = r.GetDecimal(4),
                PorcentajeDescuento = r.GetDecimal(5), MontoDescuento = r.GetDecimal(6),
                TotalFinal = r.GetDecimal(7)
            });
            return lista;
        }
        catch (PostgresException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("No se pudo consultar el historial. Inténtelo nuevamente.", ex); }
        catch (NpgsqlException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("No pudimos conectar con PostgreSQL para consultar ventas. Revise el servicio.", ex); }
        catch (Exception ex) { ErrorLog.Registrar(ex); throw; }
    }
    // A10: consultas independientes según la granularidad. Sin concatenar entradas.
    public const string SqlDiario = @"
SELECT TO_CHAR(v.fecha, 'YYYY-MM-DD') AS periodo_grupo,
       v.fecha, v.id, v.cliente, v.vendedor,
       v.subtotal_bruto, v.monto_descuento, v.total_final
FROM ventas v
WHERE v.fecha >= @inicio AND v.fecha < @fin + 1
ORDER BY v.fecha, v.id;";

    public const string SqlMensual = @"
SELECT TO_CHAR(v.fecha, 'YYYY-MM') AS periodo_mes,
       v.fecha::date AS fecha_dia, COUNT(v.id) AS cantidad_ventas,
       SUM(v.subtotal_bruto) AS subtotal_bruto,
       SUM(v.monto_descuento) AS total_descuentos,
       SUM(v.total_final) AS total_cobrado
FROM ventas v
WHERE v.fecha >= @inicio AND v.fecha < @fin + 1
GROUP BY TO_CHAR(v.fecha, 'YYYY-MM'), v.fecha::date
ORDER BY periodo_mes, fecha_dia;";

    public const string SqlValidacion = @"
SELECT COUNT(id) AS operaciones,
       COALESCE(SUM(subtotal_bruto), 0) AS bruto,
       COALESCE(SUM(monto_descuento), 0) AS descuentos,
       COALESCE(SUM(total_final), 0) AS total_sql
FROM ventas
WHERE fecha::date BETWEEN @inicio AND @fin;";

    private static void Fechas(NpgsqlCommand cmd, DateOnly inicio, DateOnly fin)
    {
        cmd.Parameters.AddWithValue("inicio", NpgsqlTypes.NpgsqlDbType.Date, inicio);
        cmd.Parameters.AddWithValue("fin", NpgsqlTypes.NpgsqlDbType.Date, fin);
    }

    public List<ReporteFila> ObtenerReporteVentas(DateOnly inicio, DateOnly fin, string tipo)
    {
        try
        {
            using var conn = new NpgsqlConnection(_cadena);
            conn.Open();
            bool mensual = tipo == "Mensual";
            using var cmd = new NpgsqlCommand(mensual ? SqlMensual : SqlDiario, conn);
            Fechas(cmd, inicio, fin);
            using var r = cmd.ExecuteReader();
            var filas = new List<ReporteFila>();
            while (r.Read())
            {
                filas.Add(mensual
                    ? new ReporteFila(r.GetString(0), r.GetFieldValue<DateOnly>(1).ToDateTime(TimeOnly.MinValue), null,
                        "", "", r.GetInt64(2), r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5))
                    : new ReporteFila(r.GetString(0), r.GetDateTime(1), r.GetInt32(2),
                        r.GetString(3), r.GetString(4), 1, r.GetDecimal(5), r.GetDecimal(6), r.GetDecimal(7)));
            }
            return filas;
        }
        catch (NpgsqlException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException("No pudimos consultar las ventas. Verifique la conexión con PostgreSQL e inténtelo nuevamente.", ex);
        }
    }

    public ReporteTotales VerificarTotal(DateOnly inicio, DateOnly fin)
    {
        try
        {
            using var conn = new NpgsqlConnection(_cadena);
            conn.Open();
            using var cmd = new NpgsqlCommand(SqlValidacion, conn);
            Fechas(cmd, inicio, fin);
            using var r = cmd.ExecuteReader();
            r.Read();
            return new ReporteTotales(r.GetInt64(0), r.GetDecimal(1), r.GetDecimal(2), r.GetDecimal(3));
        }
        catch (NpgsqlException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException("No pudimos validar el total. Revise PostgreSQL.", ex);
        }
    }
}
