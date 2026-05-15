using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Config;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    /// <summary>
    /// Base form providing the sidebar + topbar layout for all main screens.
    /// All inner forms inherit from this.
    /// </summary>
    public class BaseMainForm : Form
    {
        protected SidebarControl Sidebar { get; private set; }
        protected TopBarControl TopBar { get; private set; }
        protected Panel ContentArea { get; private set; }

        public BaseMainForm()
        {
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            BackColor = AppColors.BackgroundGeneral;
            Font = AppFonts.Body;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gestor de Inventario";

            Sidebar = new SidebarControl();
            TopBar = new TopBarControl { UserName = Session.UserName };
            ContentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.BackgroundGeneral,
                Padding = new Padding(24)
            };

            Sidebar.NavigateTo += OnNavigateTo;
            Controls.Add(ContentArea);
            Controls.Add(TopBar);
            Controls.Add(Sidebar);
        }

        protected virtual void OnNavigateTo(string module)
        {
            switch (module)
            {
                case "Inicio": OpenModule(new FrmDashboard()); break;
                case "Productos": OpenModule(new FrmProductos()); break;
                case "Proveedores": OpenModule(new FrmProveedores()); break;
                case "Entradas": OpenModule(new FrmEntradaInventario()); break;
                case "Salidas": OpenModule(new FrmSalidaInventario()); break;
                case "Inventario": OpenModule(new FrmInventario()); break;
                case "Reportes": OpenModule(new FrmHistorialMovimientos()); break;
                case "Alertas": OpenModule(new FrmAlertas()); break;
                case "Logout":
                    if (MessageBox.Show("¿Desea cerrar sesión?", "Cerrar sesión",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Session.Clear();
                        Application.Restart();
                    }
                    break;
            }
        }

        private void OpenModule(Form form)
        {
            // Embed the form inside the content area
            foreach (Control c in ContentArea.Controls)
                c.Dispose();
            ContentArea.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.BackColor = AppColors.BackgroundGeneral;
            ContentArea.Controls.Add(form);
            form.Show();

            // Update topbar module name
            if (form is IModuleForm mf)
            {
                TopBar.ModuleName = mf.ModuleName;
                Sidebar.ActiveItem = mf.SidebarKey;
            }
        }
    }

    public interface IModuleForm
    {
        string ModuleName { get; }
        string SidebarKey { get; }
    }
}
