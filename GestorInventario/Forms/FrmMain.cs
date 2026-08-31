using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Config;
using GestorInventario.Helpers;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    /// <summary>
    /// Formulario Principal Contenedor (frmPrincipal / FrmMain)
    /// Aplica navegación moderna embebiendo formularios hijos en panel central sin MDI.
    /// </summary>
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
            Text = "Sistema de Facturación y Control de Inventario — Pascual Bravo";
            FormBorderStyle = FormBorderStyle.Sizable;
            BuildLayout();
            LoadModule(new FrmDashboard(), "Dashboard", "Dashboard");
        }

        private void BuildLayout()
        {
            _sidebar = new SidebarControl();
            _topBar = new TopBarControl { UserName = string.IsNullOrEmpty(Session.UserName) ? "Lorena Correa (Cajera)" : Session.UserName };

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

        private void Navigate(string module)
        {
            switch (module)
            {
                // ── TABLAS ──────────────────────────────────────────
                case "Clientes":
                    LoadModule(new frmLista_Clientes(), "Administración de Clientes", "Clientes");
                    break;
                case "Productos":
                    LoadModule(new frmlista_productos(), "Administración de Productos", "Productos");
                    break;
                case "Categorías":
                    LoadModule(new frmlista_CategoriaProductos(), "Categorías de Productos", "Categorías");
                    break;
                case "Proveedores":
                    LoadModule(new FrmProveedores(), "Administración de Proveedores", "Proveedores");
                    break;

                // ── FACTURACIÓN ─────────────────────────────────────
                case "Facturación":
                    LoadModule(new frmlistaFacturas(), "Administración de Facturas", "Facturación");
                    break;
                case "Informes":
                    LoadModule(new frmInformes(), "Generador de Informes", "Informes");
                    break;

                // ── INVENTARIO ──────────────────────────────────────
                case "Dashboard":
                    LoadModule(new FrmDashboard(), "Panel de Control", "Dashboard");
                    break;
                case "Entradas":
                    LoadModule(new FrmEntradaInventario(), "Entrada de Inventario", "Entradas");
                    break;
                case "Salidas":
                    LoadModule(new FrmSalidaInventario(), "Salida de Inventario", "Salidas");
                    break;
                case "Stock Actual":
                    LoadModule(new FrmInventario(), "Existencias de Inventario", "Stock Actual");
                    break;
                case "Alertas":
                    LoadModule(new FrmAlertas(), "Alertas de Stock Crítico", "Alertas");
                    break;

                // ── SEGURIDAD ───────────────────────────────────────
                case "Empleados":
                    LoadModule(new frmlista_Empleados(), "Administración de Empleados", "Empleados");
                    break;
                case "Roles":
                    LoadModule(new frmlista_RolEmpleados(), "Roles de Empleados", "Roles");
                    break;
                case "Seguridad":
                    LoadModule(new frmAdminSeguridad(), "Seguridad y Usuarios", "Seguridad");
                    break;

                // ── AYUDA ───────────────────────────────────────────
                case "Ayuda Web":
                    LoadModule(new frmAyuda(), "Centro de Ayuda y Documentación", "Ayuda Web");
                    break;
                case "Acerca de":
                    LoadModule(new frmAcercaDe(), "Acerca del Sistema", "Acerca de");
                    break;

                // ── CERRAR SESIÓN ───────────────────────────────────
                case "Logout":
                    if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar sesión",
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

    /// <summary>
    /// Alias para cumplir con el nombre exacto de la guía universitaria (frmPrincipal)
    /// </summary>
    public class frmPrincipal : FrmMain
    {
    }
}
