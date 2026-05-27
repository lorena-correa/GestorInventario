using System;
using System.Collections.Generic;
using GestorInventario.Models;

namespace GestorInventario.Services
{
    /// <summary>
    /// Authentication service — connect to PostgreSQL via AuthRepository.
    /// </summary>
    public class AuthService
    {
        // TODO: inject AuthRepository
        public bool Login(string usuario, string password)
        {
            // DEMO: accept admin/admin for testing UI
            if (usuario == "admin" && password == "admin")
            {
                Config.Session.IsAuthenticated = true;
                Config.Session.UserName = "Administrador";
                Config.Session.Role = "Administrador";
                Config.Session.UserId = 1;
                return true;
            }
            return false;
        }

        public void Logout() => Config.Session.Clear();
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
