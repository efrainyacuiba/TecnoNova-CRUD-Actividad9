namespace TiendaProyecto.ViewModels;
public class RegistrarVentaRequest
{
    public string Cliente { get; set; } = "Consumidor Final";
    public string Vendedor { get; set; } = string.Empty;
    public decimal PorcentajeDescuento { get; set; }
    public List<ItemVentaRequest> Items { get; set; } = new();
}
public class ItemVentaRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}
