# GestorInventario
### Sistema de Control de Inventario — MVP Universitario

---

## 🛠️ Tecnologías

- **C# / .NET 8** — Windows Forms
- **PostgreSQL** — Base de datos (preparado para conexión)
- **Npgsql** — Driver PostgreSQL para .NET
- **Arquitectura N-Capas**

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

## 🚀 Cómo ejecutar

### Requisitos
- Visual Studio 2022+ o VS Code con extensión C#
- .NET 8 SDK
- (Opcional) PostgreSQL para conexión real

### Pasos
1. Abre `GestorInventario.sln` en Visual Studio
2. Restaura paquetes NuGet (Npgsql)
3. Ejecuta con F5

### Credenciales de demo
- **Usuario:** `admin`
- **Contraseña:** `admin`

---

## 🗄️ Conectar PostgreSQL

1. Crea la base de datos ejecutando `Config/schema.sql`
2. Abre `Config/DatabaseConfig.cs` y actualiza:
   ```csharp
   public static string Host = "localhost";
   public static string Database = "gestor_inventario";
   public static string Username = "postgres";
   public static string Password = "tu_password";
   ```
3. Implementa los métodos en `Repositories/Repositories.cs`
4. Inyecta los repositorios en `Services/Services.cs`

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

*Proyecto universitario MVP · v1.0.0*
