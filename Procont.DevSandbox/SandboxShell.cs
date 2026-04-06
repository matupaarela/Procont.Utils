using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Procont.DevSandbox
{
    /// <summary>
    /// Ventana principal del DevSandbox.
    /// Panel izquierdo con lista de componentes → área derecha con la página activa.
    /// </summary>
    public partial class SandboxShell : Form
    {
        private readonly Dictionary<string, Func<UserControl>> _pages =
            new Dictionary<string, Func<UserControl>>();

        private UserControl _activePage;
        private Panel _contentArea;
        private ListBox _nav;

        public SandboxShell()
        {
            InitializeComponent();
        }

        private void SandboxShell_Load(object sender, EventArgs e)
        {
            // Registrar páginas — agregar una línea por cada nuevo componente
            Register("Sidebar", () => new Pages.SidebarPage());
            Register("ComboSearch", () => new Pages.ComboSearchPage());

            if (_nav.Items.Count > 0)
                _nav.SelectedIndex = 0;
        }

        private void Register(string name, Func<UserControl> factory)
        {
            _pages[name] = factory;
            _nav.Items.Add(name);
        }

        private void Nav_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = _nav.SelectedItem?.ToString();
            if (name == null || !_pages.ContainsKey(name)) return;

            _activePage?.Dispose();
            _activePage = _pages[name]();
            _activePage.Dock = DockStyle.Fill;

            _contentArea.Controls.Clear();
            _contentArea.Controls.Add(_activePage);
        }
    }
}