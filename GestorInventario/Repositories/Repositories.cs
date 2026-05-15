using System.Collections.Generic;
using GestorInventario.Config;
using GestorInventario.Models;

namespace GestorInventario.Repositories
{
    /// <summary>
    /// Base repository with connection helper.
    /// All repositories will use Npgsql to connect to PostgreSQL.
    /// </summary>
    public abstract class BaseRepository
    {
        protected string ConnectionString => DatabaseConfig.ConnectionString;

        // Example usage (uncomment when connecting real DB):
        // protected NpgsqlConnection GetConnection() => new NpgsqlConnection(ConnectionString);
    }

    /// <summary>
    /// User repository — implement with Npgsql queries against tb_usuarios.
    /// </summary>
    public class UsuarioRepository : BaseRepository
    {
        // TODO: implement with real SQL queries
        // public Usuario? GetByCredentials(string email, string passwordHash) { ... }
        // public List<Usuario> GetAll() { ... }
        // public bool Create(Usuario u) { ... }
        // public bool Update(Usuario u) { ... }
        // public bool Delete(int id) { ... }
    }

    /// <summary>
    /// Product repository — implement with Npgsql queries against tb_productos.
    /// </summary>
    public class ProductoRepository : BaseRepository
    {
        // TODO: implement with real SQL queries
        // public List<Producto> GetAll() { ... }
        // public Producto? GetById(int id) { ... }
        // public bool Create(Producto p) { ... }
        // public bool Update(Producto p) { ... }
        // public bool Delete(int id) { ... }
    }

    /// <summary>
    /// Supplier repository — implement with Npgsql queries against tb_proveedores.
    /// </summary>
    public class ProveedorRepository : BaseRepository
    {
        // TODO: implement with real SQL queries
    }

    /// <summary>
    /// Inventory movement repository — implement with Npgsql queries against tb_movimientos_inventario.
    /// </summary>
    public class MovimientoRepository : BaseRepository
    {
        // TODO: implement with real SQL queries
    }

    /// <summary>
    /// Alert repository — implement with Npgsql queries against tb_alertas.
    /// </summary>
    public class AlertaRepository : BaseRepository
    {
        // TODO: implement with real SQL queries
    }
}
