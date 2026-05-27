using System;
using System.Collections.Generic;
using GestorInventario.Models;

namespace GestorInventario.Services
{
    public static class DemoData
    {
        public static List<Proveedor> Proveedores = new()
        {
            new() { Id=1, Nombre="TechSupply S.A.",      Telefono="555-0101", Correo="ventas@techsupply.com",  Direccion="Calle 10 #25-40",          Activo=true, CreadoEn=DateTime.Now.AddMonths(-6) },
            new() { Id=2, Nombre="Distribuidora Norte",  Telefono="555-0202", Correo="info@distnorte.com",     Direccion="Av. Principal #100",        Activo=true, CreadoEn=DateTime.Now.AddMonths(-4) },
            new() { Id=3, Nombre="GlobalParts Ltda.",    Telefono="555-0303", Correo="pedidos@globalparts.com",Direccion="Zona Industrial Bloque 5",  Activo=true, CreadoEn=DateTime.Now.AddMonths(-2) },
        };

        public static List<Producto> Productos = new()
        {
            new() { Id=1, Codigo="PRD-001", Nombre="Monitor Samsung 24\"", Descripcion="Monitor Full HD 1080p",        PrecioCompra=350000, PrecioVenta=450000, StockActual=12, StockMinimo=5,  Categoria="Electrónica",    ProveedorId=1, Proveedor="TechSupply S.A.",     Activo=true, CreadoEn=DateTime.Now.AddDays(-30) },
            new() { Id=2, Codigo="PRD-002", Nombre="Teclado Mecánico",     Descripcion="Teclado RGB switches blue",    PrecioCompra=85000,  PrecioVenta=130000, StockActual=3,  StockMinimo=5,  Categoria="Periféricos",    ProveedorId=1, Proveedor="TechSupply S.A.",     Activo=true, CreadoEn=DateTime.Now.AddDays(-25) },
            new() { Id=3, Codigo="PRD-003", Nombre="Cable HDMI 2m",        Descripcion="Cable HDMI 4K 2 metros",       PrecioCompra=12000,  PrecioVenta=22000,  StockActual=45, StockMinimo=10, Categoria="Cables",         ProveedorId=2, Proveedor="Distribuidora Norte", Activo=true, CreadoEn=DateTime.Now.AddDays(-20) },
            new() { Id=4, Codigo="PRD-004", Nombre="Mouse Inalámbrico",    Descripcion="Mouse óptico 2.4GHz",          PrecioCompra=35000,  PrecioVenta=58000,  StockActual=2,  StockMinimo=8,  Categoria="Periféricos",    ProveedorId=1, Proveedor="TechSupply S.A.",     Activo=true, CreadoEn=DateTime.Now.AddDays(-15) },
            new() { Id=5, Codigo="PRD-005", Nombre="Hub USB 4 puertos",    Descripcion="Hub USB 3.0 con alimentación", PrecioCompra=28000,  PrecioVenta=48000,  StockActual=18, StockMinimo=6,  Categoria="Accesorios",     ProveedorId=3, Proveedor="GlobalParts Ltda.",   Activo=true, CreadoEn=DateTime.Now.AddDays(-10) },
            new() { Id=6, Codigo="PRD-006", Nombre="Disco SSD 500GB",      Descripcion="SSD SATA 2.5\" 500GB",         PrecioCompra=180000, PrecioVenta=260000, StockActual=7,  StockMinimo=3,  Categoria="Almacenamiento", ProveedorId=2, Proveedor="Distribuidora Norte", Activo=true, CreadoEn=DateTime.Now.AddDays(-5)  },
        };

        public static List<Movimiento> Movimientos = new()
        {
            new() { Id=1, ProductoId=1, NombreProducto="Monitor Samsung 24\"", TipoMovimientoId=1, TipoMovimiento="Entrada", Cantidad=10, Observacion="Compra inicial",           UsuarioId=1, NombreUsuario="Administrador", ProveedorId=1, Proveedor="TechSupply S.A.",     Fecha=DateTime.Today.AddHours(-3) },
            new() { Id=2, ProductoId=2, NombreProducto="Teclado Mecánico",     TipoMovimientoId=2, TipoMovimiento="Salida",  Cantidad=2,  Observacion="Venta departamento TI",    UsuarioId=1, NombreUsuario="Administrador", ProveedorId=0, Proveedor="",                    Fecha=DateTime.Today.AddHours(-2) },
            new() { Id=3, ProductoId=3, NombreProducto="Cable HDMI 2m",        TipoMovimientoId=1, TipoMovimiento="Entrada", Cantidad=30, Observacion="Reabastecimiento mensual", UsuarioId=1, NombreUsuario="Administrador", ProveedorId=2, Proveedor="Distribuidora Norte", Fecha=DateTime.Today.AddDays(-1)  },
            new() { Id=4, ProductoId=4, NombreProducto="Mouse Inalámbrico",    TipoMovimientoId=2, TipoMovimiento="Salida",  Cantidad=5,  Observacion="Préstamo sala reuniones",  UsuarioId=1, NombreUsuario="Administrador", ProveedorId=0, Proveedor="",                    Fecha=DateTime.Today.AddDays(-2)  },
        };

        public static List<Alerta> Alertas = new()
        {
            new() { Id=1, ProductoId=2, NombreProducto="Teclado Mecánico",  CodigoProducto="PRD-002", StockActual=3,  StockMinimo=5, UnidadesFaltantes=2, Estado="Activa", CreadoEn=DateTime.Now.AddHours(-1) },
            new() { Id=2, ProductoId=4, NombreProducto="Mouse Inalámbrico", CodigoProducto="PRD-004", StockActual=2,  StockMinimo=8, UnidadesFaltantes=6, Estado="Activa", CreadoEn=DateTime.Now.AddHours(-2) },
        };
    }
}