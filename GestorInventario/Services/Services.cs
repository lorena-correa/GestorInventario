using GestorInventario.Models;
using GestorInventario.Repositories;
using System;
using System.Collections.Generic;

namespace GestorInventario.Services
{
    /// <summary>
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

    /// <summary>
    /// Product service — connect to PostgreSQL via ProductoRepository.
    /// </summary>
    public class ProductoService
    {
        // TODO: inject ProductoRepository
        public List<Producto> ObtenerTodos() => DemoData.Productos;
        public List<Producto> Buscar(string termino) =>
            DemoData.Productos.FindAll(p =>
                p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Contains(termino, StringComparison.OrdinalIgnoreCase));
        public bool Guardar(Producto p) { DemoData.Productos.Add(p); return true; }
        public bool Eliminar(int id) { DemoData.Productos.RemoveAll(p => p.Id == id); return true; }
    }

    /// <summary>
    /// Supplier service — connect to PostgreSQL via ProveedorRepository.
    /// </summary>
    public class ProveedorService
    {
        public List<Proveedor> ObtenerTodos() => DemoData.Proveedores;
        public bool Guardar(Proveedor p) { DemoData.Proveedores.Add(p); return true; }
        public bool Eliminar(int id) { DemoData.Proveedores.RemoveAll(p => p.Id == id); return true; }
    }

    /// <summary>
    /// Inventory movement service.
    /// </summary>
    public class MovimientoService
    {
        public List<Movimiento> ObtenerTodos() => DemoData.Movimientos;
        public bool RegistrarEntrada(Movimiento m) { DemoData.Movimientos.Add(m); return true; }
        public bool RegistrarSalida(Movimiento m) { DemoData.Movimientos.Add(m); return true; }
    }

    /// <summary>
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

    /// <summary>
    /// Alert service.
    /// </summary>
    public class AlertaService
    {
        public List<Alerta> ObtenerAlertas() => DemoData.Alertas;
    }
}
