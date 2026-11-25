using Microsoft.AspNetCore.Mvc;
using TiendaProyecto.Data;
using TiendaProyecto.Models;
namespace TiendaProyecto.Controllers;
public class ReportesController : Controller
{
    private readonly ReporteDAO _dao;
    public ReportesController(ReporteDAO dao) => _dao = dao;

    public IActionResult Index()
    {
        try { return View(_dao.Auditoria()); }
        catch (FriendlyException ex) { ViewBag.Error = ex.Message; return View(new List<AuditoriaDescuento>()); }
        catch (Exception ex) { ErrorLog.Registrar(ex); ViewBag.Error = "No pudimos consultar el reporte. Revise la conexión e intente de nuevo."; return View(new List<AuditoriaDescuento>()); }
    }

    [HttpGet]
    public IActionResult Ventas(DateOnly? inicio, DateOnly? fin, string tipo = "Diario", string? generadoPor = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var vm = new ReporteVentas
        {
            Inicio = inicio ?? hoy, Fin = fin ?? hoy, Tipo = tipo,
            GeneradoPor = string.IsNullOrWhiteSpace(generadoPor) ? "Operador local" : generadoPor.Trim()
        };
        if (!ModelState.IsValid || vm.Inicio > vm.Fin || (tipo != "Diario" && tipo != "Mensual") || vm.GeneradoPor.Length > 100)
        {
            vm.Error = "Revise el rango de fechas, la granularidad y el nombre del operador (máximo 100 caracteres).";
            return View(vm);
        }
        try
        {
            vm.Filas = _dao.ObtenerReporteVentas(vm.Inicio, vm.Fin, tipo);
            vm.Consultado = true;
        }
        catch (FriendlyException ex) { vm.Error = ex.Message; }
        catch (Exception ex) { ErrorLog.Registrar(ex); vm.Error = "No se pudo generar el reporte. Inténtelo nuevamente."; }
        return View(vm);
    }

    [HttpGet]
    public IActionResult ValidarSql(DateOnly? inicio, DateOnly? fin)
    {
        if (!ModelState.IsValid || inicio is null || fin is null || inicio > fin)
            return BadRequest("Indique un rango de fechas válido.");
        try
        {
            var total = _dao.VerificarTotal(inicio.Value, fin.Value);
            ViewBag.Inicio = inicio.Value;
            ViewBag.Fin = fin.Value;
            ViewBag.Sql = ReporteDAO.SqlValidacion;
            return View(total);
        }
        catch (FriendlyException ex) { return StatusCode(503, ex.Message); }
    }
}
