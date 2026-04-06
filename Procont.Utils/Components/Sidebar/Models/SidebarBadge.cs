using System.ComponentModel;

namespace Procont.Utils.Components.Sidebar.Models
{
    /// <summary>
    /// Insignia informativa que se dibuja en el extremo derecho de un
    /// ítem, grupo o raíz del sidebar.
    /// </summary>
    public enum SidebarBadge
    {
        [Description("Sin insignia.")]
        None,

        [Description("Insignia verde «NEW» — funcionalidad recién añadida.")]
        New,

        [Description("Insignia ámbar «BETA» — funcionalidad en fase de pruebas.")]
        Beta
    }
}
