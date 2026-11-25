namespace TiendaProyecto.Models;
public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Codigo) || Codigo.Trim().Length > 30)
            throw new ArgumentException("Escriba un código de producto de máximo 30 caracteres.");
        if (string.IsNullOrWhiteSpace(Nombre) || Nombre.Trim().Length > 100)
            throw new ArgumentException("Escriba un nombre de producto de máximo 100 caracteres.");
        if (Precio <= 0 || Precio > 99999999m)
            throw new ArgumentException("El precio debe ser mayor a Bs. 0 y estar dentro del rango permitido.");
        if (Stock < 0)
            throw new ArgumentException("El stock no puede ser negativo. Revise la cantidad.");
    }
}
