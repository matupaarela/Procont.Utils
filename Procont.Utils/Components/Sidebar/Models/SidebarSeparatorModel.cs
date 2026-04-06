using System.ComponentModel;

namespace Procont.Utils.Components.Sidebar.Models
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SidebarSeparatorModel : SidebarNodeModel
    {
        [Category("Sidebar")]
        [Description("Etiqueta opcional centrada sobre la línea divisoria. Vacío = línea simple.")]
        [DefaultValue("")]
        public string Label { get; set; } = "";

        public override string ToString() =>
            string.IsNullOrEmpty(Label) ? "── separador ──" : $"── {Label} ──";
    }
}
