using System;

namespace GestorInventario.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
    }

    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }

        // Propiedad calculada útil para la UI
        public bool EsCritico => StockActual <= StockMinimo;
    }

    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
    }

    public class TipoMovimiento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "entrada" o "salida"
    }

    // Renombrada de Movimiento → Movimiento
    // para coincidir con los repositorios
    public class Movimiento
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public int TipoMovimientoId { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public string TipoEntradaSalida { get; set; } = string.Empty; // "entrada" o "salida"
        public int Cantidad { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class Alerta
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int UnidadesFaltantes { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }

    public class DashboardStats
    {
        public int TotalProductos { get; set; }
        public int ProductosCriticos { get; set; }
        public int MovimientosHoy { get; set; }
        public int AlertasActivas { get; set; }
        public int TotalProveedores { get; set; }
        public decimal ValorInventario { get; set; }
    }
}