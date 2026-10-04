using GestorInventario.Models;
using GestorInventario.Repositories;

namespace GestorInventario.Services
{
    
    /// Authentication service — connect to PostgreSQL via AuthRepository.
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


    /// Category service — connect to PostgreSQL via CategoriaRepository.
    public class CategoriaService
    {
        private readonly CategoriaRepository _repo = new();

        public List<Categoria> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener categorías: {ex.Message}"); }
        }

        public List<Categoria> Buscar(string termino)
        {
            try { return _repo.Search(termino); }
            catch (Exception ex) { throw new Exception($"Error al buscar categorías: {ex.Message}"); }
        }

        public bool Guardar(Categoria c)
        {
            try
            {
                if (c.Id == 0) return _repo.Create(c);
                else return _repo.Update(c);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar categoría: {ex.Message}"); }
        }
    }

    /// Client service — connect to PostgreSQL via ClienteRepository.
    public class ClienteService
    {
        private readonly ClienteRepository _repo = new();

        public List<Cliente> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener clientes: {ex.Message}"); }
        }

        public List<Cliente> Buscar(string termino)
        {
            try { return _repo.Search(termino); }
            catch (Exception ex) { throw new Exception($"Error al buscar clientes: {ex.Message}"); }
        }

        public bool Guardar(Cliente c)
        {
            try
            {
                if (c.Id == 0) return _repo.Create(c);
                else return _repo.Update(c);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar cliente: {ex.Message}"); }
        }
    }

    /// Employee service — connect to PostgreSQL via EmpleadoRepository.
    public class EmpleadoService
    {
        private readonly EmpleadoRepository _repo = new();

        public List<Empleado> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener empleados: {ex.Message}"); }
        }

        public List<Empleado> Buscar(string termino)
        {
            try { return _repo.Search(termino); }
            catch (Exception ex) { throw new Exception($"Error al buscar empleados: {ex.Message}"); }
        }

        public bool Guardar(Empleado e)
        {
            try
            {
                if (e.Id == 0) return _repo.Create(e);
                else return _repo.Update(e);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar empleado: {ex.Message}"); }
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
    public class UsuarioService
    {
        private readonly UsuarioRepository _repo = new();

        public List<Usuario> ObtenerTodos()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener usuarios: {ex.Message}"); }
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            try { return _repo.GetByEmail(email); }
            catch (Exception ex) { throw new Exception($"Error al buscar usuario: {ex.Message}"); }
        }

        public bool Guardar(Usuario u)
        {
            try
            {
                if (u.Id == 0) return _repo.Create(u);
                else return _repo.Update(u);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Eliminar(int id)
        {
            try { return _repo.Delete(id); }
            catch (Exception ex) { throw new Exception($"Error al eliminar usuario: {ex.Message}"); }
        }
    }

    // ══════════════════════════════════════════════════════════
    //  FACTURA SERVICE
    // ══════════════════════════════════════════════════════════
    public class FacturaService
    {
        public const decimal TasaIva = 0.19m;
        private readonly FacturaRepository _repo = new();

        public List<Factura> ObtenerTodas()
        {
            try { return _repo.GetAll(); }
            catch (Exception ex) { throw new Exception($"Error al obtener facturas: {ex.Message}"); }
        }

        public List<Factura> Buscar(string termino, string estado)
        {
            try { return _repo.Search(termino, estado); }
            catch (Exception ex) { throw new Exception($"Error al buscar facturas: {ex.Message}"); }
        }

        public Factura? ObtenerPorId(int id)
        {
            try { return _repo.GetById(id); }
            catch (Exception ex) { throw new Exception($"Error al obtener la factura: {ex.Message}"); }
        }

        public string SiguienteNumero()
        {
            try { return _repo.GetSiguienteNumero(); }
            catch (Exception ex) { throw new Exception($"Error al generar el consecutivo: {ex.Message}"); }
        }

        /// <summary>Subtotal, IVA y total de la factura a partir de sus líneas.</summary>
        public static void CalcularTotales(Factura f)
        {
            f.Subtotal = f.Detalles.Sum(d => d.Subtotal);
            f.TotalIva = f.Subtotal * TasaIva;
            f.TotalFactura = Math.Max(0, f.Subtotal + f.TotalIva - f.Descuento);
        }

        public bool Guardar(Factura f)
        {
            if (f.Detalles.Count == 0)
                throw new InvalidOperationException("La factura debe tener al menos un producto.");
            if (f.Descuento < 0)
                throw new InvalidOperationException("El descuento no puede ser negativo.");

            CalcularTotales(f);
            try
            {
                if (f.Id == 0) return _repo.Create(f) > 0;
                else return _repo.Update(f);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public bool Anular(int id)
        {
            try { return _repo.Anular(id); }
            catch (Exception ex) { throw new Exception($"Error al anular factura: {ex.Message}"); }
        }

        public List<Factura> ObtenerPorRango(DateTime desde, DateTime hasta)
        {
            try { return _repo.GetPorRangoFechas(desde, hasta); }
            catch (Exception ex) { throw new Exception($"Error al generar el informe: {ex.Message}"); }
        }

        public List<ProductoVendido> ObtenerMasVendidos(DateTime desde, DateTime hasta)
        {
            try { return _repo.GetProductosMasVendidos(desde, hasta); }
            catch (Exception ex) { throw new Exception($"Error al generar el informe: {ex.Message}"); }
        }
    }
}
