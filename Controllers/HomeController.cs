using Microsoft.AspNetCore.Mvc;
namespace TiendaProyecto.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
