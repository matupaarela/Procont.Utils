using FontAwesome.Sharp;
using System;
using System.ComponentModel;

namespace Procont.Utils.Sidebar.Models
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
    }
}
