using Microsoft.AspNetCore.Mvc;
using TiendaProyecto.Models;

namespace TiendaProyecto.Controllers;
// SOLO ambiente Development. Reproduce un caso inválido sin guardar ni modificar datos.
public class DepuracionController : Controller
{
    private readonly IWebHostEnvironment _ambiente;
    public DepuracionController(IWebHostEnvironment ambiente) => _ambiente = ambiente;

    [HttpGet("/Depuracion/Descuento25")]
    public IActionResult Descuento25()
    {
        if (!_ambiente.IsDevelopment()) return NotFound();
        var venta = new Venta { Cliente = "Consumidor Final", Vendedor = "Prueba de depuración" };
        venta.Detalles.Add(new DetalleVenta { IdProducto = 1, NombreProducto = "Prueba", Cantidad = 1, PrecioUnitario = 100m });
        try
        {
            venta.AplicarDescuento(25m); // F5: breakpoint en el if de Venta.AplicarDescuento.
            return Content("No se detectó el descuento inválido (revisar regla).", "text/plain");
        }
        catch (ArgumentOutOfRangeException)
        {
            return Content("Caso detectado: el sistema rechazó 25 % y sigue funcionando. No se guardaron datos.", "text/plain");
        }
    }
}
