# GestorInventario 📦
### Sistema de Control de Inventario — MVP Universitario

---

## 📋 Descripción

Aplicación de escritorio en C# Windows Forms para administrar productos, proveedores, entradas, salidas, existencias y niveles mínimos de stock. Genera alertas automáticas de reabastecimiento cuando el stock baja del mínimo configurado.

---

## 🚀 Estado del proyecto

| Sprint | Estado | Fecha |
|--------|--------|-------|
| Sprint 1 — Análisis, BD y Front | ✅ Entregado | Mayo 14 |
| Sprint 2 — Back, integración y pruebas | ✅ Entregado | Mayo 26 |

---

## ⚙️ Requisitos e instalación

### Requisitos
- Visual Studio 2022
- .NET 8
- PostgreSQL 14+
- Npgsql (instalado via NuGet)

### Pasos para ejecutar

1. Clonar el repositorio
```bash
git clone https://github.com/lorena-correa/GestorInventario.git
```

2. Crear la base de datos en PostgreSQL
```sql
CREATE DATABASE gestor_inventario;
```

3. Ejecutar el script completo en pgAdmin
```
GestorInventario/Config/schema.sql
```

4. Configurar contraseña en `Config/DatabaseConfig.cs`
```csharp
public static string Password { get; set; } = "tu_contraseña";
```

5. Ejecutar el proyecto en Visual Studio con F5

---

## 👥 Roles y credenciales

| Rol | Acceso |
|-----|--------|
| Administrador | Acceso total |
| Almacenista   | Entradas y salidas |
| Supervisor    | Solo lectura |

**Credenciales de prueba:**

| Rol           | Usuario                    | Contraseña |
|---------------|----------------------------|------------|
| Administrador | admin@empresa.com          | admin      |
| Almacenista   | almacenista@empresa.com    | almacenista|
| Supervisor    | supervisor@empresa.com     | supervisor |

---

## ✅ Funcionalidades implementadas

- Autenticación por roles con hash SHA256
- CRUD completo de productos, proveedores y usuarios
- Registro de entradas y salidas con actualización automática de stock
- Validaciones: stock negativo, cantidades inválidas, códigos duplicados, fechas futuras
- Alertas automáticas cuando stock baja del mínimo (trigger PostgreSQL)
- Búsquedas y filtros en productos, proveedores e historial
- Reportes: stock actual, productos críticos, movimientos del mes
- Exportación a CSV desde historial y reportes
- Manejo de errores en formularios y conexión a BD
- Procedimientos almacenados para todas las operaciones

---

## 🗄️ Base de datos

**Motor:** PostgreSQL | **BD:** `gestor_inventario`

### Tablas
| Tabla | Descripción |
|-------|-------------|
| tb_roles | Roles del sistema |
| tb_usuarios | Usuarios autenticados |
| tb_productos | Productos del inventario |
| tb_proveedores | Proveedores registrados |
| tb_tipo_movimiento | Tipos de movimiento |
| tb_movimientos_inventario | Historial de entradas y salidas |
| tb_alertas | Alertas de stock crítico |

### Procedimientos almacenados
- `sp_login` — Autenticación por roles
- `sp_crear_producto` / `sp_actualizar_producto` / `sp_eliminar_producto`
- `sp_crear_proveedor` / `sp_actualizar_proveedor` / `sp_eliminar_proveedor`
- `sp_crear_usuario` / `sp_actualizar_usuario`
- `sp_registrar_movimiento` — Con validación de stock
- `sp_reporte_stock_actual` / `sp_reporte_movimientos`
- `sp_dashboard_stats`

### Trigger
- `trg_stock_minimo` — Genera alerta automática cuando stock ≤ mínimo

---

## 📁 Estructura del proyecto

```
GestorInventario/
├── Forms/                      # Capa de Presentación
│   ├── FrmLogin.cs
│   ├── FrmRecuperarContrasena.cs
│   ├── FrmMain.cs              # Shell principal con Sidebar + TopBar
│   ├── FrmDashboard.cs
│   ├── FrmProductos.cs
│   ├── FrmProducto.cs
│   ├── FrmProveedores.cs       # Incluye FrmProveedor
│   ├── FrmMovimientos.cs       # FrmEntradaInventario + FrmSalidaInventario
│   ├── FrmInventarioModules.cs # FrmInventario + FrmHistorial + FrmAlertas
│   └── FrmUsuariosConfig.cs    # FrmUsuarios + FrmReportes + FrmConfiguracion
│
├── Models/                     # Entidades del dominio
│   └── Models.cs
│
├── Services/                   # Capa de Lógica de Negocio
│   ├── Services.cs
│   └── DemoData.cs             # ⚠️ Archivo legacy — ya no lo uso
│
├── Repositories/               # Capa de Acceso a Datos (Npgsql)
│   └── Repositories.cs
│
├── Components/                 # Controles personalizados reutilizables
│   ├── RoundedButton.cs
│   ├── CardPanel.cs
│   ├── SidebarControl.cs
│   ├── TopBarControl.cs
│   └── ModernTextBox.cs
│
├── Helpers/                    # Utilidades de diseño
│   ├── AppColors.cs
│   ├── AppFonts.cs
│   └── UIHelper.cs
│
├── Config/                     # Configuración
│   ├── Session.cs
│   ├── DatabaseConfig.cs
│   └── schema.sql              # Esquema PostgreSQL completo + SPs
│
└── Program.cs                  # Punto de entrada
```

---

## 📋 Formularios incluidos

| Formulario | Descripción |
|------------|-------------|
| FrmLogin | Autenticación con diseño moderno split-panel |
| FrmRecuperarContrasena | Recuperación por correo |
| FrmMain | Shell: Sidebar + TopBar + área de contenido |
| FrmDashboard | Métricas, movimientos recientes, estado |
| FrmProductos | Listado con búsqueda y CRUD |
| FrmProducto | Formulario de alta/edición de producto |
| FrmProveedores | Listado de proveedores con CRUD |
| FrmProveedor | Formulario de proveedor |
| FrmEntradaInventario | Registro de entradas con validación de stock |
| FrmSalidaInventario | Registro de salidas con validación de stock |
| FrmInventario | Vista del stock con filtros |
| FrmHistorialMovimientos | Movimientos con filtros por tipo, fecha y exportación CSV |
| FrmAlertas | Productos con stock crítico, resolver e ignorar |
| FrmReportes | Reportes de stock, críticos y movimientos con exportación |
| FrmUsuarios | Gestión de usuarios con CRUD completo |
| FrmConfiguracion | Config de base de datos e info del sistema |

---

## 🏗️ Arquitectura

```
┌─────────────────────────────┐
│    Capa de Presentación     │  ← Forms + Components
├─────────────────────────────┤
│  Capa de Lógica de Negocio  │  ← Services
├─────────────────────────────┤
│   Capa de Acceso a Datos    │  ← Repositories (Npgsql)
├─────────────────────────────┤
│    Capa de Base de Datos    │  ← PostgreSQL
└─────────────────────────────┘
```

---

## 🌿 Ramas del repositorio

| Rama | Descripción |
|------|-------------|
| `master` | Rama principal estable |
| `feature/database-connection` | Conexión PostgreSQL y repositorios |
| `feature/stored-procedures` | Procedimientos almacenados |
| `feature/authentication` | Login por roles con SHA256 |
| `feature/crud-products` | CRUD productos con validaciones |
| `feature/crud-providers` | CRUD proveedores |
| `feature/inventory-movements` | Entradas y salidas con transacciones |
| `feature/alerts` | Alertas automáticas e historial con filtros |
| `feature/reports` | Reportes y gestión de usuarios |
| `feature/documentation` | README final |

---

## 🎨 Diseño

| Elemento | Valor |
|----------|-------|
| Fondo | `#F8F9FA` |
| Sidebar | `#1E293B` |
| Primario | `#7B61FF` |
| Fuente | Segoe UI |
| Cards | Bordes suaves, sombras sutiles |

---

*Proyecto universitario — MVP Control de Inventario*