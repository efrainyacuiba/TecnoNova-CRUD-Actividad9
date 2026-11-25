using Microsoft.AspNetCore.Mvc;
using TiendaProyecto.Data;
using TiendaProyecto.Models;
namespace TiendaProyecto.Controllers;
public class ProductosController : Controller
{
    private readonly ProductoDAO _dao;
    public ProductosController(ProductoDAO dao) => _dao = dao;
    public IActionResult Index()
    {
        try { return View(_dao.Listar()); }
        catch (FriendlyException ex) { ViewBag.Error = ex.Message; return View(new List<Producto>()); }
        catch (Exception ex) { ErrorLog.Registrar(ex); ViewBag.Error = "No pudimos cargar el catálogo. Intente nuevamente o contacte a soporte."; return View(new List<Producto>()); }
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Crear(Producto producto)
    {
        try { _dao.Guardar(producto); TempData["Exito"] = "Producto registrado correctamente en el catálogo."; }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (FriendlyException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex) { ErrorLog.Registrar(ex); TempData["Error"] = "Ocurrió un inconveniente al guardar. Los datos no se modificaron; intente otra vez."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public IActionResult Editar(int id)
    {
        try
        {
            var p = _dao.Buscar(id);
            if (p == null) { TempData["Error"] = "Seleccione un producto existente de la tabla."; return RedirectToAction(nameof(Index)); }
            return View(p);
        }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (FriendlyException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex) { ErrorLog.Registrar(ex); TempData["Error"] = "No pudimos abrir el producto. Intente nuevamente."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Editar(Producto producto)
    {
        try { _dao.Editar(producto); TempData["Exito"] = "Producto actualizado correctamente."; }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (FriendlyException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex) { ErrorLog.Registrar(ex); TempData["Error"] = "No se pudo guardar la actualización. Intente nuevamente."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try { _dao.Eliminar(id); TempData["Exito"] = "Producto eliminado del catálogo."; }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (FriendlyException ex) { TempData["Error"] = ex.Message; }
        catch (Exception ex) { ErrorLog.Registrar(ex); TempData["Error"] = "No se pudo eliminar el producto. Intente nuevamente."; }
        return RedirectToAction(nameof(Index));
    }
}
