using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Config;
using GestorInventario.Helpers;
using GestorInventario.Services;

namespace GestorInventario.Forms
{

    public class FrmMain : Form
    {
        private SidebarControl _sidebar = null!;
        private TopBarControl _topBar = null!;
        private Panel _contentPanel = null!;

        public FrmMain()
        {
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            BackColor = AppColors.BackgroundGeneral;
            Font = AppFonts.Body;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gestor de Inventario";
            FormBorderStyle = FormBorderStyle.Sizable;
            BuildLayout();
            // AplicarPermisosPorRol();  ← comenta esta línea por ahora
            LoadModule(new FrmDashboard(), "Dashboard", "Inicio");

            /*

            // ── DEBUG temporal  para validar los roles ─────────────────────────────────────────
            MessageBox.Show(
                $"Usuario: {Session.UserName}\n" +
                $"Rol raw: '{Session.Role}'\n" +
                $"Rol lower: '{Session.Role?.ToLower()}'",
                "Debug sesión");
            // ──────────────────────────────────────────────────────────*/

            AplicarPermisosPorRol();
            LoadModule(new FrmDashboard(), "Dashboard", "Inicio");

        }

        private void BuildLayout()
        {
            _sidebar = new SidebarControl();
            _topBar = new TopBarControl { UserName = Session.UserName };

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BackgroundGeneral,
                Padding = new Padding(0)
            };

            _sidebar.NavigateTo += Navigate;

            Controls.Add(_contentPanel);
            Controls.Add(_topBar);
            Controls.Add(_sidebar);
        }
        //Agrega  los permisos y reestricciones por rol
        private void AplicarPermisosPorRol()
        {
            string rol = Session.Role ?? "";

            // El Supervisor solo ve: Inicio, Inventario, Reportes, Historial
            // El Almacenista no ve: Usuarios, Configuración, Reportes
            // El Administrador ve todo

            _sidebar.SetModuleVisible("Usuarios", rol == "Administrador");
            _sidebar.SetModuleVisible("Configuración", rol == "Administrador");
            _sidebar.SetModuleVisible("Reportes", rol == "Administrador" || rol == "Supervisor");
            _sidebar.SetModuleVisible("Entradas", rol == "Administrador" || rol == "Almacenista");
            _sidebar.SetModuleVisible("Salidas", rol == "Administrador" || rol == "Almacenista");
            _sidebar.SetModuleVisible("Proveedores", rol == "Administrador" || rol == "Almacenista");
            _sidebar.SetModuleVisible("Alertas", rol == "Administrador" || rol == "Almacenista");
        }

        //Nvegación de la página
        private void Navigate(string module)
        {
            string rol = Session.Role ?? "";

            switch (module)
            {
                case "Inicio":
                    LoadModule(new FrmDashboard(), "Dashboard", "Inicio");
                    break;

                // ── Módulos accesibles por Administrador y Almacenista ──────────
                case "Productos":
                    if (rol == "Administrador" || rol == "Almacenista")
                        LoadModule(new FrmProductos(), "Productos", "Productos");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Proveedores":
                    if (rol == "Administrador" || rol == "Almacenista")
                        LoadModule(new FrmProveedores(), "Proveedores", "Proveedores");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Entradas":
                    if (rol == "Administrador" || rol == "Almacenista")
                        LoadModule(new FrmEntradaInventario(), "Entrada de Inventario", "Entradas");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Salidas":
                    if (rol == "Administrador" || rol == "Almacenista")
                        LoadModule(new FrmSalidaInventario(), "Salida de Inventario", "Salidas");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Alertas":
                    if (rol == "Administrador" || rol == "Almacenista")
                        LoadModule(new FrmAlertas(), "Alertas", "Alertas");
                    else
                        MostrarAccesoDenegado();
                    break;

                // ── Módulos de solo lectura: Administrador y Supervisor ─────────
                case "Inventario":
                    if (rol == "Administrador" || rol == "Supervisor")
                        LoadModule(new FrmInventario(), "Inventario", "Inventario");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Reportes":
                    if (rol == "Administrador" || rol == "Supervisor")
                        LoadModule(new FrmReportes(), "Reportes", "Reportes");
                    else
                        MostrarAccesoDenegado();
                    break;

                // ── Solo Administrador ──────────────────────────────────────────
                case "Usuarios":
                    if (rol == "Administrador")
                        LoadModule(new FrmUsuarios(), "Usuarios", "Usuarios");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Configuración":
                    if (rol == "Administrador")
                        LoadModule(new FrmConfiguracion(), "Configuración", "Configuración");
                    else
                        MostrarAccesoDenegado();
                    break;

                case "Logout":
                    if (MessageBox.Show("¿Desea cerrar sesión?", "Cerrar sesión",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Session.Clear();
                        var login = new FrmLogin();
                        login.Show();
                        Close();
                    }
                    break;
            }
        }

        private void MostrarAccesoDenegado()
        {
            MessageBox.Show(
                "No tiene permisos para acceder a este módulo.",
                "Acceso denegado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private void LoadModule(Form form, string moduleName, string sidebarKey)
        {
            _topBar.ModuleName = moduleName;
            _sidebar.ActiveItem = sidebarKey;

            foreach (Control c in _contentPanel.Controls) c.Dispose();
            _contentPanel.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.BackColor = AppColors.BackgroundGeneral;
            _contentPanel.Controls.Add(form);
            form.Show();
        }
    }
}
