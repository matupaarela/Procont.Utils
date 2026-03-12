using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using FontAwesome.Sharp;

namespace Procont.Utils.Sidebar.Models
{
    /// <summary>
    /// Modelo de un grupo colapsable del sidebar.
    ///
    /// ── ORDEN ────────────────────────────────────────────────────────
    /// Children es una sola lista ordenada que mezcla libremente
    /// SidebarItemModel y SidebarGroupModel en el orden visual exacto.
    ///
    /// En el CollectionEditor (clic en [...]):
    ///   Add ▼ → SidebarItemModel   (ítem hoja)
    ///   Add ▼ → SidebarGroupModel  (sub-grupo)
    ///
    /// IMPORTANTE: NO marcar con [Serializable]. El CodeDom serializer
    /// escribe cada elemento por su tipo concreto en InitializeComponent(),
    /// lo que evita el error de carga del .resx.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SidebarGroupModel : SidebarNodeModel
    {
        [Category("Sidebar")]
        [Description("Texto visible del encabezado del grupo.")]
        [DefaultValue("Grupo")]
        public string GroupTitle { get; set; } = "Grupo";

        [Category("Sidebar")]
        [Description("Si es true, el grupo aparece expandido al cargar.")]
        [DefaultValue(false)]
        public bool Expanded { get; set; } = false;

        /// <summary>
        /// Hijos en ORDEN DE VISUALIZACIÓN.
        /// Mezcla SidebarItemModel y SidebarGroupModel libremente.
        /// El CollectionEditor mostrará "Add ▼" con ambos tipos gracias
        /// a SidebarNodeCollectionEditor.
        /// </summary>
        [Category("Sidebar")]
        [Description("Hijos en orden visual. Mezcla ítems y sub-grupos libremente con Add ▼.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(SidebarNodeCollectionEditor), typeof(UITypeEditor))]
        public List<SidebarNodeModel> Children { get; } = new List<SidebarNodeModel>();

        public override string ToString() =>
            string.IsNullOrEmpty(GroupTitle) ? "(grupo sin título)" : $"📁 {GroupTitle}";
    }
}
