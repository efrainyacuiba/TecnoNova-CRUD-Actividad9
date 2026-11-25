using TiendaProyecto.Models;
using Xunit;
namespace TiendaProyecto.Tests;
public class ReporteVentasTests
{
    private static ReporteFila Fila(string periodo, int n, decimal bruto, decimal descuento) =>
        new(periodo, new DateTime(2026,11,24), null, "", "", n, bruto, descuento, bruto-descuento);
    [Fact] public void SumaMensualRespetaCantidadYDescuentos()
    {
        var vm = new ReporteVentas { Tipo = "Mensual", Filas = new() { Fila("2026-10",2,300,10), Fila("2026-11",3,1200,70) } };
        Assert.Equal(new ReporteTotales(5,1500,80,1420),vm.Total);
        Assert.Equal(2,vm.Paginas().SelectMany(p=>p).Count());
    }
    [Fact] public void PaginacionNoDuplicaFilasNiSubtotales()
    {
        var vm = new ReporteVentas { Filas = Enumerable.Range(1,17).Select(_=>Fila("2026-11-24",1,100,10)).ToList() };
        var grupos = vm.Paginas().SelectMany(x=>x).ToList();
        Assert.Equal(17,grupos.Sum(x=>x.Filas.Count));
        Assert.Single(grupos.Where(x=>x.UltimoTramo));
        Assert.Equal(1530m,vm.Total.Total);
    }
    [Fact] public void VacioNoCreaPaginas()
    {
        var vm = new ReporteVentas(); Assert.Empty(vm.Paginas()); Assert.Equal(0m,vm.Total.Total);
    }
}
