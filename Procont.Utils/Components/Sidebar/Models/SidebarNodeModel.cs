using FontAwesome.Sharp;
using System.ComponentModel;

namespace Procont.Utils.Components.Sidebar.Models
{
    /// <summary>
    /// Clase base para SidebarItemModel y SidebarGroupModel.
    ///
    /// NO usar [Serializable] — el CodeDom serializer escribe cada subclase
    /// concreta como código en InitializeComponent(), evitando el error RESX.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class SidebarNodeModel
    {
        [Category("Sidebar")]
        [Description("Clave única para identificar este nodo por código (Key).")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        [Category("Sidebar")]
        [Description("Ícono FontAwesome.Sharp (IconChar.*).")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        [Category("Sidebar")]
        [Description("Insignia informativa: None, New (verde) o Beta (ámbar).")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge { get; set; } = SidebarBadge.None;
    }
}
