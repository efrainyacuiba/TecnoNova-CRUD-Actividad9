using Npgsql;
using TiendaProyecto.Models;

namespace TiendaProyecto.Data;
public class ProductoDAO
{
    private readonly string _cadena;
    public ProductoDAO(IConfiguration config) => _cadena = config.GetConnectionString("Tienda")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Tienda.");

    public List<Producto> Listar()
    {
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand("SELECT id, codigo, nombre, precio, stock FROM productos ORDER BY id", conexion);
            using var reader = cmd.ExecuteReader();
            var items = new List<Producto>();
            while (reader.Read()) items.Add(Leer(reader));
            return items;
        }
        catch (PostgresException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException("No pudimos consultar el catálogo. Revise la base de datos e intente nuevamente.", ex);
        }
        catch (NpgsqlException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException("No fue posible conectar con la base de datos. Compruebe que PostgreSQL esté funcionando e intente otra vez.", ex);
        }
        catch (Exception ex) when (ex is not FriendlyException)
        {
            ErrorLog.Registrar(ex);
            throw;
        }
        finally
        {
            if (conexion.State == System.Data.ConnectionState.Open) conexion.Close();
        }
    }

    public Producto? Buscar(int id)
    {
        if (id <= 0) throw new ArgumentException("Seleccione un producto válido para editar.");
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand("SELECT id, codigo, nombre, precio, stock FROM productos WHERE id = @id", conexion);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Leer(reader) : null;
        }
        catch (PostgresException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("No se pudo consultar el producto. Intente nuevamente.", ex); }
        catch (NpgsqlException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("La base de datos no está disponible. Compruebe PostgreSQL e intente nuevamente.", ex); }
        catch (Exception ex) when (ex is not FriendlyException && ex is not ArgumentException) { ErrorLog.Registrar(ex); throw; }
    }

    public int Guardar(Producto producto)
    {
        // Guarda: falla en validación antes de abrir cualquier conexión.
        ArgumentNullException.ThrowIfNull(producto);
        producto.Validar();
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO productos (codigo,nombre,precio,stock) VALUES (@codigo,@nombre,@precio,@stock) RETURNING id", conexion);
            cmd.Parameters.AddWithValue("codigo", producto.Codigo.Trim());
            cmd.Parameters.AddWithValue("nombre", producto.Nombre.Trim());
            cmd.Parameters.AddWithValue("precio", producto.Precio);
            cmd.Parameters.AddWithValue("stock", producto.Stock);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (PostgresException ex)
        {
            ErrorLog.Registrar(ex);
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                throw new FriendlyException($"El código '{producto.Codigo}' ya pertenece a otro producto. Use otro código o edite el producto existente.", ex);
            throw new FriendlyException("No pudimos guardar el producto por una restricción de datos. Revise la información e intente nuevamente.", ex);
        }
        catch (NpgsqlException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException("No fue posible conectar con la base de datos. Verifique que PostgreSQL esté activo y vuelva a intentar.", ex);
        }
        catch (Exception ex) when (ex is not FriendlyException)
        {
            ErrorLog.Registrar(ex);
            throw; // conserva el Stack Trace, no usar throw ex;
        }
        finally
        {
            if (conexion.State == System.Data.ConnectionState.Open) conexion.Close();
        }
    }

    public void Editar(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);
        if (producto.Id <= 0) throw new ArgumentException("Seleccione una fila antes de editar.");
        producto.Validar();
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand("UPDATE productos SET codigo=@codigo,nombre=@nombre,precio=@precio,stock=@stock WHERE id=@id", conexion);
            cmd.Parameters.AddWithValue("id", producto.Id);
            cmd.Parameters.AddWithValue("codigo", producto.Codigo.Trim());
            cmd.Parameters.AddWithValue("nombre", producto.Nombre.Trim());
            cmd.Parameters.AddWithValue("precio", producto.Precio);
            cmd.Parameters.AddWithValue("stock", producto.Stock);
            if (cmd.ExecuteNonQuery() == 0) throw new FriendlyException("No encontramos ese producto. Actualice el listado y selecciónelo nuevamente.");
        }
        catch (PostgresException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException(ex.SqlState == PostgresErrorCodes.UniqueViolation
                ? "Ya existe otro producto con ese código. Use un código diferente."
                : "No se pudo actualizar el producto. Revise sus datos e inténtelo de nuevo.", ex);
        }
        catch (NpgsqlException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("La base de datos no responde. Revise PostgreSQL y pruebe de nuevo.", ex); }
        catch (Exception ex) when (ex is not FriendlyException) { ErrorLog.Registrar(ex); throw; }
    }

    public void Eliminar(int id)
    {
        if (id <= 0) throw new ArgumentException("Seleccione primero un producto de la tabla.");
        using var conexion = new NpgsqlConnection(_cadena);
        try
        {
            conexion.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM productos WHERE id=@id", conexion);
            cmd.Parameters.AddWithValue("id", id);
            if (cmd.ExecuteNonQuery() == 0) throw new FriendlyException("No encontramos ese producto. Actualice el listado e intente otra vez.");
        }
        catch (PostgresException ex)
        {
            ErrorLog.Registrar(ex);
            throw new FriendlyException(ex.SqlState == PostgresErrorCodes.ForeignKeyViolation
                ? "Este producto ya aparece en ventas registradas. Para conservar el historial, no se puede eliminar; puede editar su stock."
                : "No se pudo eliminar el producto. Revise su estado e intente otra vez.", ex);
        }
        catch (NpgsqlException ex) { ErrorLog.Registrar(ex); throw new FriendlyException("No fue posible conectar con la base de datos. Revise PostgreSQL.", ex); }
        catch (Exception ex) when (ex is not FriendlyException) { ErrorLog.Registrar(ex); throw; }
    }
    private static Producto Leer(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0), Codigo = r.GetString(1), Nombre = r.GetString(2),
        Precio = r.GetDecimal(3), Stock = r.GetInt32(4)
    };
}
