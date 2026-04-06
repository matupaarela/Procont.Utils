using System.ComponentModel;

namespace Procont.Utils.Components.Sidebar.Models
{
    /// <summary>
    /// Modelo de un ítem hoja del sidebar.
    /// Serializado como código en InitializeComponent() — sin .resx.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SidebarItemModel : SidebarNodeModel
    {
        [Category("Sidebar")]
        [Description("Texto visible del ítem en el menú.")]
        [DefaultValue("Ítem")]
        public string ItemText { get; set; } = "Ítem";

        public override string ToString() =>
            string.IsNullOrEmpty(ItemText) ? "(ítem sin texto)" : $"📄 {ItemText}";
    }
}
