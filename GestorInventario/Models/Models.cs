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

    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
    }

    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaRetiro { get; set; }
        public string DatosAdicionales { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
    }

    public class Factura
    {
        public int Id { get; set; }
        public string NroFactura { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public int ClienteId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
        public string Empleado { get; set; } = string.Empty;
        public string Estado { get; set; } = "Emitida";
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalIva { get; set; }
        public decimal TotalFactura { get; set; }
        public List<DetalleFactura> Detalles { get; set; } = new();
    }

    public class DetalleFactura
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int ProductoId { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Valores calculados de la línea (IVA 19%)
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public decimal Iva => Subtotal * 0.19m;
        public decimal TotalLinea => Subtotal + Iva;
    }

    // Filas de los informes de facturación
    public class ProductoVendido
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int UnidadesVendidas { get; set; }
        public decimal Ingresos { get; set; }
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