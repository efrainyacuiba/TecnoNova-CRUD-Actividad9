using TiendaProyecto.Models;
using Xunit;

namespace TiendaProyecto.Tests;
public class ProductoValidacionTests
{
    [Fact] public void CodigoVacio_SeRechaza() =>
        Assert.Throws<ArgumentException>(() => new Producto { Nombre = "Ejemplo", Precio = 10, Stock = 1 }.Validar());
    [Fact] public void PrecioNegativo_SeRechaza() =>
        Assert.Throws<ArgumentException>(() => new Producto { Codigo = "A-1", Nombre = "Ejemplo", Precio = -2, Stock = 1 }.Validar());
    [Fact] public void StockNegativo_SeRechaza() =>
        Assert.Throws<ArgumentException>(() => new Producto { Codigo = "A-1", Nombre = "Ejemplo", Precio = 10, Stock = -1 }.Validar());
    [Fact] public void ProductoValido_SeAcepta() =>
        new Producto { Codigo = "A-1", Nombre = "Ejemplo", Precio = 10, Stock = 1 }.Validar();
}
