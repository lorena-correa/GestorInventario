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
    public class MovimientoService
    {
        private readonly MovimientoRepository _repo = new();

        public List<Movimiento> ObtenerTodos()
        {
            try { return _repo.GetHistorial(); }
            catch (Exception ex) { throw new Exception($"Error al obtener movimientos: {ex.Message}"); }
        }

        public List<Movimiento> Filtrar(
            int? productoId = null,
            int? tipoId = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            try { return _repo.GetHistorial(productoId, tipoId, desde, hasta); }
            catch (Exception ex) { throw new Exception($"Error al filtrar movimientos: {ex.Message}"); }
        }

        public bool RegistrarEntrada(Movimiento m)
        {
            try
            {
                // Buscar tipo "Compra a proveedor" (id=1) por defecto para entradas
                if (m.TipoMovimientoId == 0) m.TipoMovimientoId = 1;
                return _repo.RegistrarMovimiento(m);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool RegistrarSalida(Movimiento m)
        {
            try
            {
                // Buscar tipo "Venta" (id=2) por defecto para salidas
                if (m.TipoMovimientoId == 0) m.TipoMovimientoId = 2;
                return _repo.RegistrarMovimiento(m);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Registrar(Movimiento m)
        {
            try { return _repo.RegistrarMovimiento(m); }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public List<TipoMovimiento> ObtenerTipos()
        {
            try { return _repo.GetTipos(); }
            catch (Exception ex) { throw new Exception($"Error al obtener tipos: {ex.Message}"); }
        }
    }

    /// Dashboard statistics service.
    public class DashboardService
    {
        private readonly ProductoRepository _prodRepo = new();
        private readonly MovimientoRepository _movRepo = new();
        private readonly AlertaRepository _alertRepo = new();
        private readonly ProveedorRepository _provRepo = new();

        public DashboardStats ObtenerEstadisticas()
        {
            try
            {
                var productos = _prodRepo.GetAll();
                var criticos = _prodRepo.GetCriticos();
                var movHoy = _movRepo.GetHistorial(desde: DateTime.Today, hasta: DateTime.Today.AddDays(1));
                var alertas = _alertRepo.ContarActivas();
                var proveedores = _provRepo.GetAll();

                decimal valorInventario = 0;
                foreach (var p in productos)
                    valorInventario += p.StockActual * p.PrecioVenta;

                return new DashboardStats
                {
                    TotalProductos = productos.Count,
                    ProductosCriticos = criticos.Count,
                    MovimientosHoy = movHoy.Count,
                    AlertasActivas = alertas,
                    TotalProveedores = proveedores.Count,
                    ValorInventario = valorInventario
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estadísticas: {ex.Message}");
            }
        }
    }

    /// Alert service.
    public class AlertaService
    {
        private readonly AlertaRepository _repo = new();

        public List<Alerta> ObtenerAlertas()
        {
            try { return _repo.GetActivas(); }
            catch (Exception ex) { throw new Exception($"Error al obtener alertas: {ex.Message}"); }
        }

        public bool Resolver(int alertaId)
        {
            try { return _repo.Resolver(alertaId); }
            catch (Exception ex) { throw new Exception($"Error al resolver alerta: {ex.Message}"); }
        }

        public bool Ignorar(int alertaId)
        {
            try { return _repo.Ignorar(alertaId); }
            catch (Exception ex) { throw new Exception($"Error al ignorar alerta: {ex.Message}"); }
        }

        public int ContarActivas()
        {
            try { return _repo.ContarActivas(); }
            catch (Exception ex) { throw new Exception($"Error al contar alertas: {ex.Message}"); }
        }
    }
}
