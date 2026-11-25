namespace TiendaProyecto.Models;

public sealed record ReporteFila(string Periodo, DateTime Fecha, int? Id, string Cliente,
    string Vendedor, long Cantidad, decimal Bruto, decimal Descuento, decimal Total);
public sealed record ReporteTotales(long Cantidad, decimal Bruto, decimal Descuento, decimal Total)
{
    public static ReporteTotales Sumar(IEnumerable<ReporteFila> filas) => new(
        filas.Sum(x => x.Cantidad), filas.Sum(x => x.Bruto),
        filas.Sum(x => x.Descuento), filas.Sum(x => x.Total));
}
public sealed record ReporteGrupo(string Periodo, IReadOnlyList<ReporteFila> Filas,
    ReporteTotales Totales, bool Continuacion, bool UltimoTramo);

public sealed class ReporteVentas
{
    public DateOnly Inicio { get; init; }
    public DateOnly Fin { get; init; }
    public string Tipo { get; init; } = "Diario";
    public string GeneradoPor { get; init; } = "Operador local";
    public DateTime Emision { get; init; } = DateTime.Now;
    public bool Consultado { get; set; }
    public string? Error { get; set; }
    public List<ReporteFila> Filas { get; set; } = new();
    public ReporteTotales Total => ReporteTotales.Sumar(Filas);
    public bool Mensual => Tipo == "Mensual";

    // Páginas lógicas para impresión; no se pierden filas y cada subtotal sale una vez.
    public List<List<ReporteGrupo>> Paginas()
    {
        var paginas = new List<List<ReporteGrupo>>();
        var actual = new List<ReporteGrupo>();
        int unidades = 0;
        foreach (var grupo in Filas.GroupBy(x => x.Periodo))
        {
            var filas = grupo.ToList();
            var tramos = filas.Chunk(6).ToArray();
            for (int i = 0; i < tramos.Length; i++)
            {
                int costo = tramos[i].Length + 2;
                if (unidades + costo > 10 && actual.Count > 0)
                { paginas.Add(actual); actual = new(); unidades = 0; }
                actual.Add(new ReporteGrupo(grupo.Key, tramos[i], ReporteTotales.Sumar(filas), i > 0, i == tramos.Length - 1));
                unidades += costo;
            }
        }
        if (actual.Count > 0) paginas.Add(actual);
        return paginas;
    }
}
