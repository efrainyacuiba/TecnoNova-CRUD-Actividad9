namespace TiendaProyecto.Models;
public class AuditoriaDescuento
{
    public string Vendedor { get; set; } = string.Empty;
    public long VentasConDescuento { get; set; }
    public decimal SumaDescuentos { get; set; }
}
public class VentaResumen
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public decimal SubtotalBruto { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal TotalFinal { get; set; }
}
