# GestorInventario
### Sistema de Control de Inventario — MVP Universitario

---

## 📁 Estructura del Proyecto

```
GestorInventario/
├── Forms/                    # Capa de Presentación
│   ├── FrmLogin.cs
│   ├── FrmRecuperarContrasena.cs
│   ├── FrmMain.cs            # Shell principal con Sidebar + TopBar
│   ├── FrmDashboard.cs
│   ├── FrmProductos.cs
│   ├── FrmProducto.cs
│   ├── FrmProveedores.cs     # Incluye FrmProveedor
│   ├── FrmMovimientos.cs     # FrmEntradaInventario + FrmSalidaInventario
│   ├── FrmInventarioModules.cs # FrmInventario + FrmHistorial + FrmAlertas
│   └── FrmUsuariosConfig.cs  # FrmUsuarios + FrmConfiguracion
│
├── Models/                   # Entidades del dominio
│   └── Models.cs
│
├── Services/                 # Capa de Lógica de Negocio
│   ├── Services.cs
│   └── DemoData.cs
│
├── Repositories/             # Capa de Acceso a Datos
│   └── Repositories.cs
│
├── Components/               # Controles personalizados reutilizables
│   ├── RoundedButton.cs
│   ├── CardPanel.cs
│   ├── SidebarControl.cs
│   ├── TopBarControl.cs
│   └── ModernTextBox.cs
│
├── Helpers/                  # Utilidades de diseño
│   ├── AppColors.cs
│   ├── AppFonts.cs
│   └── UIHelper.cs
│
├── Config/                   # Configuración
│   ├── Session.cs
│   ├── DatabaseConfig.cs
│   └── schema.sql            # Esquema PostgreSQL completo
│
└── Program.cs                # Punto de entrada
```

---

### Credenciales de demo
- **Usuario:** `admin`
- **Contraseña:** `admin`

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

## 📋 Formularios incluidos

| Formulario | Descripción |
|-----------|-------------|
| FrmLogin | Autenticación con diseño moderno split-panel |
| FrmRecuperarContrasena | Recuperación por correo |
| FrmMain | Shell: Sidebar + TopBar + área de contenido |
| FrmDashboard | Métricas, movimientos recientes, estado |
| FrmProductos | Listado con búsqueda y CRUD |
| FrmProducto | Formulario de alta/edición de producto |
| FrmProveedores | Listado de proveedores con CRUD |
| FrmProveedor | Formulario de proveedor |
| FrmEntradaInventario | Registro de entradas |
| FrmSalidaInventario | Registro de salidas |
| FrmInventario | Vista del stock con filtros |
| FrmHistorialMovimientos | Movimientos con filtros por tipo y fecha |
| FrmAlertas | Productos con stock crítico |
| FrmUsuarios | Gestión de usuarios |
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

*Proyecto universitario*
