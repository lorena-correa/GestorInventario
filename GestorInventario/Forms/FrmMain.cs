using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Config;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    /// <summary>
    /// Main shell form: hosts Sidebar + TopBar + dynamic content area.
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
            Text = "Gestor de Inventario";
            FormBorderStyle = FormBorderStyle.Sizable;
            BuildLayout();
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

        private void Navigate(string module)
        {
            switch (module)
            {
                case "Inicio": LoadModule(new FrmDashboard(), "Dashboard", "Inicio"); break;
                case "Productos": LoadModule(new FrmProductos(), "Productos", "Productos"); break;
                case "Proveedores": LoadModule(new FrmProveedores(), "Proveedores", "Proveedores"); break;
                case "Entradas": LoadModule(new FrmEntradaInventario(), "Entrada de Inventario", "Entradas"); break;
                case "Salidas": LoadModule(new FrmSalidaInventario(), "Salida de Inventario", "Salidas"); break;
                case "Inventario": LoadModule(new FrmInventario(), "Inventario", "Inventario"); break;
                case "Reportes": LoadModule(new FrmHistorialMovimientos(), "Historial de Movimientos", "Reportes"); break;
                case "Alertas": LoadModule(new FrmAlertas(), "Alertas", "Alertas"); break;
                case "Usuarios": LoadModule(new FrmUsuarios(), "Usuarios", "Usuarios"); break;
                case "Configuración": LoadModule(new FrmConfiguracion(), "Configuración", "Configuración"); break;
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
