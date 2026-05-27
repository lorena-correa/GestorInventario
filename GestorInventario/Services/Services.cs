using GestorInventario.Models;
using GestorInventario.Repositories;

namespace GestorInventario.Services
{
    
    /// Authentication service — connect to PostgreSQL via AuthRepository.
    /// </summary>
    public class AuthService
    {
        private readonly UsuarioRepository _repo = new();

        public bool Login(string email, string password)
        {
            try
            {
                // Hashear contraseña con SHA256
                string hash = HashPassword(password);

                // Primero intentar con hash, si falla intentar texto plano (para usuario admin inicial)
                var usuario = _repo.GetByCredentials(email, hash)
                           ?? _repo.GetByCredentials(email, password);

                if (usuario == null) return false;

                // Guardar sesión
                Config.Session.IsAuthenticated = true;
                Config.Session.UserId = usuario.Id;
                Config.Session.UserName = usuario.Nombre;
                Config.Session.Email = usuario.Email;
                Config.Session.Role = usuario.Rol;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al conectar con la base de datos: {ex.Message}");
            }
        }

        public void Logout() => Config.Session.Clear();

        public static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }

    
    /// Product service — connecta con PostgreSQL via ProductoRepository :3
    public class ProductoService
    {
        private readonly ProductoRepository _repo = new();

        public List<Producto> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener productos: {ex.Message}"); }
        }

        public List<Producto> Buscar(string termino)
        {
            try { return _repo.Search(termino); }
            catch (Exception ex) { throw new Exception($"Error al buscar productos: {ex.Message}"); }
        }

        public bool Guardar(Producto p)
        {
            try
            {
                if (p.Id == 0) return _repo.Create(p);
                else return _repo.Update(p);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar producto: {ex.Message}"); }
        }

        public List<Producto> ObtenerCriticos()
        {
            try { return _repo.GetCriticos(); }
            catch (Exception ex) { throw new Exception($"Error al obtener críticos: {ex.Message}"); }
        }
    }

    /// Supplier service — connect to PostgreSQL via ProveedorRepository.
    public class ProveedorService
    {
        private readonly ProveedorRepository _repo = new();

        public List<Proveedor> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener proveedores: {ex.Message}"); }
        }

        public List<Proveedor> Buscar(string termino)
        {
            try { return _repo.Search(termino); }
            catch (Exception ex) { throw new Exception($"Error al buscar proveedores: {ex.Message}"); }
        }

        public bool Guardar(Proveedor p)
        {
            try
            {
                if (p.Id == 0) return _repo.Create(p);
                else return _repo.Update(p);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar proveedor: {ex.Message}"); }
        }
    }

    
    /// Inventory movement service.
    /// </summary>
    public class MovimientoService
    {
        public List<Movimiento> ObtenerTodos() => DemoData.Movimientos;
        public bool RegistrarEntrada(Movimiento m) { DemoData.Movimientos.Add(m); return true; }
        public bool RegistrarSalida(Movimiento m) { DemoData.Movimientos.Add(m); return true; }
    }

    
    /// Dashboard statistics service.
    /// </summary>
    public class DashboardService
    {
        public DashboardStats ObtenerEstadisticas() => new DashboardStats
        {
            TotalProductos = DemoData.Productos.Count,
            ProductosCriticos = DemoData.Productos.FindAll(p => p.StockActual <= p.StockMinimo).Count,
            MovimientosHoy = DemoData.Movimientos.FindAll(m => m.Fecha.Date == DateTime.Today).Count,
            AlertasActivas = 3,
            TotalProveedores = DemoData.Proveedores.Count,
            ValorInventario = 125430.50m
        };
    }

    
    /// Alert service.
    /// </summary>
    public class AlertaService
    {
        public List<Alerta> ObtenerAlertas() => DemoData.Alertas;
    }
}
