namespace TiendaProyecto.Data;
public class FriendlyException : Exception
{
    public FriendlyException(string mensaje) : base(mensaje) { }
    public FriendlyException(string mensaje, Exception causa) : base(mensaje, causa) { }
}
