using TiendaProyecto.Models;
using Xunit;

namespace TiendaProyecto.Tests;
public class VentaDescuentoTests
{
    private static Venta NuevaVenta(decimal precio = 100m, int cantidad = 2) => new()
    {
        Cliente = "Consumidor Final", Vendedor = "Carlos Pérez",
        Detalles = new List<DetalleVenta> { new()
        { IdProducto = 1, NombreProducto = "Producto de prueba", Cantidad = cantidad, PrecioUnitario = precio } }
    };

    [Fact] // CR-01 PR-08-01: válido 15% del ejemplo obligatorio.
    public void AplicarDescuento_PorcentajeValido_CalculaTotalCorrectamente()
    {
        var venta = NuevaVenta();
        venta.AplicarDescuento(15m);
        Assert.Equal(200m, venta.SubtotalBruto);
        Assert.Equal(30m, venta.MontoDescuento);
        Assert.Equal(170m, venta.TotalFinal);
    }

    [Fact] // CR-01 PR-08-02: exactamente el máximo.
    public void AplicarDescuento_LimiteMaximo20_EsAceptado()
    {
        var venta = NuevaVenta(100m, 1);
        venta.AplicarDescuento(20m);
        Assert.Equal(80m, venta.TotalFinal);
    }

    [Fact] // CR-01 PR-08-03: mayor al límite.
    public void AplicarDescuento_MayorAl20PorCiento_LanzaExcepcion()
    {
        var venta = NuevaVenta();
        Assert.Throws<ArgumentOutOfRangeException>(() => venta.AplicarDescuento(25m));
    }

    [Fact] // CR-01 PR-08-04: descuento negativo.
    public void AplicarDescuento_Negativo_LanzaExcepcion()
    {
        var venta = NuevaVenta();
        Assert.Throws<ArgumentOutOfRangeException>(() => venta.AplicarDescuento(-5m));
    }

    [Fact]
    public void SinDescuento_NoAlteraElTotalDeLaActividad7()
    {
        var venta = NuevaVenta(90m, 3);
        venta.AplicarDescuento(0m);
        Assert.Equal(270m, venta.TotalFinal);
    }

    [Fact]
    public void DescuentoConDecimales_RedondeaDosDecimales()
    {
        var venta = NuevaVenta(10.03m, 1);
        venta.AplicarDescuento(15m);
        Assert.Equal(1.50m, venta.MontoDescuento);
        Assert.Equal(8.53m, venta.TotalFinal);
    }

    [Fact]
    public void VentaSinArticulos_NoPermiteGuardar()
    {
        var venta = new Venta { Vendedor = "Ana Gómez" };
        Assert.Throws<ArgumentException>(() => venta.ValidarParaGuardar());
    }

    [Fact]
    public void VentaSinVendedor_NoPermiteGuardar()
    {
        var venta = NuevaVenta();
        venta.Vendedor = " ";
        Assert.Throws<ArgumentException>(() => venta.ValidarParaGuardar());
    }

    [Fact]
    public void CantidadCero_NoPermiteGuardar()
    {
        var venta = NuevaVenta(cantidad: 0);
        Assert.Throws<ArgumentException>(() => venta.ValidarParaGuardar());
    }

    [Fact]
    public void CantidadNegativa_NoPermiteGuardar()
    {
        var venta = NuevaVenta(cantidad: -1);
        Assert.Throws<ArgumentException>(() => venta.ValidarParaGuardar());
    }

    [Fact]
    public void VentaValida_PasaGuardasDeNegocio()
    {
        var venta = NuevaVenta();
        venta.AplicarDescuento(10m);
        venta.ValidarParaGuardar();
        Assert.Equal(180m, venta.TotalFinal);
    }
}
