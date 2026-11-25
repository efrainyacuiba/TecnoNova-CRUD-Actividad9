namespace TiendaProyecto.Models;
public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Cliente { get; set; } = "Consumidor Final";
    public string Vendedor { get; set; } = string.Empty;
    public List<DetalleVenta> Detalles { get; set; } = new();
    public decimal PorcentajeDescuento { get; private set; }
    public decimal SubtotalBruto => Detalles.Sum(d => d.Subtotal);
    public decimal MontoDescuento => Math.Round(SubtotalBruto * PorcentajeDescuento / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal TotalFinal => SubtotalBruto - MontoDescuento;
    // Compatibilidad conceptual con la actividad 7.
    public decimal Total => TotalFinal;

    public void AplicarDescuento(decimal porcentaje)
    {
        // Poner breakpoint aquí para la Actividad 9.
        if (porcentaje < 0m || porcentaje > 20m)
            throw new ArgumentOutOfRangeException(nameof(porcentaje),
                "El descuento permitido debe estar entre 0 % y 20 %. Corrija el porcentaje y vuelva a intentarlo.");
        PorcentajeDescuento = porcentaje;
    }

    public void ValidarParaGuardar()
    {
        if (string.IsNullOrWhiteSpace(Vendedor))
            throw new ArgumentException("Indique el nombre del vendedor para identificar la venta.");
        if (Vendedor.Trim().Length > 120)
            throw new ArgumentException("El vendedor debe tener máximo 120 caracteres.");
        if (string.IsNullOrWhiteSpace(Cliente) || Cliente.Trim().Length > 150)
            throw new ArgumentException("Indique un cliente válido o use Consumidor Final.");
        if (Detalles == null || Detalles.Count == 0)
            throw new ArgumentException("Agregue al menos un producto antes de confirmar la venta.");
        if (Detalles.Any(d => d.IdProducto <= 0 || d.Cantidad <= 0 || d.Cantidad > 10000 || d.PrecioUnitario <= 0))
            throw new ArgumentException("Revise los artículos: seleccione un producto y use una cantidad mayor que cero.");
        if (SubtotalBruto <= 0 || SubtotalBruto > 99999999m)
            throw new ArgumentException("Revise el total de venta y las cantidades ingresadas.");
        if (PorcentajeDescuento < 0 || PorcentajeDescuento > 20)
            throw new ArgumentOutOfRangeException(nameof(PorcentajeDescuento));
    }
}
