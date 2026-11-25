using Npgsql;
using TiendaProyecto.Models;

namespace TiendaProyecto.Data;
public class VentaDAO
{
    private readonly string _cadena;
    public VentaDAO(IConfiguration config) => _cadena = config.GetConnectionString("Tienda")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Tienda.");

    public int RegistrarVenta(Venta venta)
    {
        ArgumentNullException.ThrowIfNull(venta);
        venta.ValidarParaGuardar();
        using var conexion = new NpgsqlConnection(_cadena);
        NpgsqlTransaction? transaccion = null;
        try
        {
            conexion.Open();
            transaccion = conexion.BeginTransaction();

            // Se comprueban los productos bajo bloqueo; el cliente NO decide precios.
            var detallesConPrecioReal = new List<DetalleVenta>();
            foreach (var item in venta.Detalles)
            {
                using var consulta = new NpgsqlCommand(
                    "SELECT nombre, precio, stock FROM productos WHERE id=@id FOR UPDATE", conexion, transaccion);
                consulta.Parameters.AddWithValue("id", item.IdProducto);
                using var lector = consulta.ExecuteReader();
                if (!lector.Read()) throw new FriendlyException("Un producto del carrito ya no existe. Actualice la lista e intente otra vez.");
                var nombre = lector.GetString(0);
                var precio = lector.GetDecimal(1);
                var stock = lector.GetInt32(2);
                if (item.Cantidad > stock)
                    throw new FriendlyException($"No hay suficiente stock de {nombre}. Hay {stock} unidades disponibles; ajuste la cantidad.");
                detallesConPrecioReal.Add(new DetalleVenta
                {
                    IdProducto = item.IdProducto, NombreProducto = nombre,
                    Cantidad = item.Cantidad, PrecioUnitario = precio
                });
            }
            // Varias líneas del mismo producto se unifican antes de descontar stock.
            foreach (var grupo in detallesConPrecioReal.GroupBy(d => d.IdProducto))
            {
                var cantidadTotal = grupo.Sum(d => d.Cantidad);
                using var cmd = new NpgsqlCommand(
                    "UPDATE productos SET stock=stock-@cantidad WHERE id=@id AND stock>=@cantidad", conexion, transaccion);
                cmd.Parameters.AddWithValue("cantidad", cantidadTotal);
                cmd.Parameters.AddWithValue("id", grupo.Key);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new FriendlyException($"El stock de {grupo.First().NombreProducto} cambió. Revise las cantidades y vuelva a confirmar.");
            }

            // Recalcular el total con datos confiables de BD y aplicar CR-01 en dominio.
            var verificada = new Venta
            {
                Fecha = DateTime.Now, Cliente = venta.Cliente.Trim(),
                Vendedor = venta.Vendedor.Trim(), Detalles = detallesConPrecioReal
            };
            verificada.AplicarDescuento(venta.PorcentajeDescuento);
            verificada.ValidarParaGuardar();

            using var cabecera = new NpgsqlCommand(@"
                INSERT INTO ventas (fecha,cliente,total,vendedor,subtotal_bruto,porcentaje_descuento,monto_descuento,total_final)
                VALUES (@fecha,@cliente,@total,@vendedor,@bruto,@porcentaje,@monto,@final)
                RETURNING id", conexion, transaccion);
            cabecera.Parameters.AddWithValue("fecha", verificada.Fecha);
            cabecera.Parameters.AddWithValue("cliente", verificada.Cliente);
            cabecera.Parameters.AddWithValue("total", verificada.TotalFinal);
            cabecera.Parameters.AddWithValue("vendedor", verificada.Vendedor);
            cabecera.Parameters.AddWithValue("bruto", verificada.SubtotalBruto);
            cabecera.Parameters.AddWithValue("porcentaje", verificada.PorcentajeDescuento);
            cabecera.Parameters.AddWithValue("monto", verificada.MontoDescuento);
            cabecera.Parameters.AddWithValue("final", verificada.TotalFinal);
            var idVenta = Convert.ToInt32(cabecera.ExecuteScalar());
            foreach (var d in detallesConPrecioReal)
            {
                using var detalle = new NpgsqlCommand(@"
                    INSERT INTO detalle_ventas (id_venta,id_producto,nombre_producto,cantidad,precio_unitario,subtotal)
                    VALUES (@venta,@producto,@nombre,@cantidad,@precio,@subtotal)", conexion, transaccion);
                detalle.Parameters.AddWithValue("venta", idVenta);
                detalle.Parameters.AddWithValue("producto", d.IdProducto);
                detalle.Parameters.AddWithValue("nombre", d.NombreProducto);
                detalle.Parameters.AddWithValue("cantidad", d.Cantidad);
                detalle.Parameters.AddWithValue("precio", d.PrecioUnitario);
                detalle.Parameters.AddWithValue("subtotal", d.Subtotal);
                detalle.ExecuteNonQuery();
            }
            transaccion.Commit();
            return idVenta;
        }
        catch (FriendlyException)
        {
            // Guarda de negocio; nunca dejar una cabecera huérfana ni stock descontado.
            Revertir(transaccion);
            throw;
        }
        catch (PostgresException ex)
        {
            ErrorLog.Registrar(ex);
            Revertir(transaccion);
            throw new FriendlyException("No fue posible registrar la venta por una restricción de datos. Revise el carrito y pruebe nuevamente.", ex);
        }
        catch (NpgsqlException ex)
        {
            ErrorLog.Registrar(ex);
            Revertir(transaccion);
            throw new FriendlyException("La base de datos no está disponible. Su venta no se confirmó; revise PostgreSQL e inténtelo nuevamente.", ex);
        }
        catch (Exception ex)
        {
            ErrorLog.Registrar(ex);
            Revertir(transaccion);
            throw; // Preserva Stack Trace original para inspección Debug.
        }
        finally
        {
            transaccion?.Dispose();
            if (conexion.State == System.Data.ConnectionState.Open) conexion.Close();
        }
    }

    private static void Revertir(NpgsqlTransaction? tx)
    {
        try { tx?.Rollback(); }
        catch (Exception ex) { ErrorLog.Registrar(ex); }
    }
}
