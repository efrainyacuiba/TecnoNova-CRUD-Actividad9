using Microsoft.AspNetCore.Mvc;
using TiendaProyecto.Data;
using TiendaProyecto.Models;
using TiendaProyecto.ViewModels;
namespace TiendaProyecto.Controllers;
public class VentasController : Controller
{
    private readonly ProductoDAO _productos;
    private readonly VentaDAO _ventas;
    private readonly ReporteDAO _reportes;
    public VentasController(ProductoDAO productos, VentaDAO ventas, ReporteDAO reportes)
    {
        _productos = productos; _ventas = ventas; _reportes = reportes;
    }
    public IActionResult Nueva()
    {
        try { return View(_productos.Listar()); }
        catch (FriendlyException ex) { ViewBag.Error = ex.Message; return View(new List<Producto>()); }
        catch (Exception ex) { ErrorLog.Registrar(ex); ViewBag.Error = "No podemos cargar productos. Revise la conexión e intente nuevamente."; return View(new List<Producto>()); }
    }
    public IActionResult Historial()
    {
        try { return View(_reportes.UltimasVentas()); }
        catch (FriendlyException ex) { ViewBag.Error = ex.Message; return View(new List<VentaResumen>()); }
        catch (Exception ex) { ErrorLog.Registrar(ex); ViewBag.Error = "No pudimos obtener las ventas. Intente nuevamente."; return View(new List<VentaResumen>()); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Registrar([FromBody] RegistrarVentaRequest? datos)
    {
        try
        {
            if (datos == null) throw new ArgumentException("Complete los datos de la venta antes de confirmar.");
            var venta = new Venta
            {
                Cliente = string.IsNullOrWhiteSpace(datos.Cliente) ? "Consumidor Final" : datos.Cliente.Trim(),
                Vendedor = datos.Vendedor ?? "",
                Detalles = (datos.Items ?? new()).Select(x => new DetalleVenta
                {
                    IdProducto = x.IdProducto, Cantidad = x.Cantidad,
                    NombreProducto = "Pendiente de validar", PrecioUnitario = 1m
                }).ToList()
            };
            venta.AplicarDescuento(datos.PorcentajeDescuento); // breakpoint A9: % 25.
            venta.ValidarParaGuardar();
            int id = _ventas.RegistrarVenta(venta);
            return Json(new { ok = true, ventaId = id,
                mensaje = $"¡Venta #{id} registrada correctamente! El descuento se guardó junto al vendedor para auditoría." });
        }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, mensaje = ex.Message }); }
        catch (FriendlyException ex) { return StatusCode(409, new { ok = false, mensaje = ex.Message }); }
        catch (Exception ex)
        {
            ErrorLog.Registrar(ex);
            return StatusCode(500, new { ok = false,
                mensaje = "Hubo un inconveniente al confirmar. No se guardó una venta nueva. Revise la conexión o contacte a soporte." });
        }
    }
}
