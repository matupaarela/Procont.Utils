using FontAwesome.Sharp;
using System.ComponentModel;

namespace Procont.Utils.Sidebar.Models
{
    /// <summary>
    /// Ítem raíz del sidebar (nivel 0, sin hijos).
    /// Visualmente idéntico a un grupo padre pero no es colapsable.
    ///
    /// Hereda SidebarNodeModel para que el CodeDom serializer lo escriba
    /// como tipo concreto en InitializeComponent() — sin errores RESX.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SidebarRootItemModel : SidebarNodeModel
    {
        [Category("Sidebar")]
        [Description("Texto visible del ítem en el menú (se muestra en mayúsculas).")]
        [DefaultValue("Ítem")]
        public string ItemText { get; set; } = "Ítem";

        public override string ToString() =>
            string.IsNullOrEmpty(ItemText) ? "(ítem sin texto)" : $"▶  {ItemText}";
    }
}