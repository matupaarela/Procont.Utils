using System;
using System.ComponentModel.Design;

namespace Procont.Utils.Sidebar.Models
{
    /// <summary>
    /// CollectionEditor personalizado para List&lt;SidebarNodeModel&gt;.
    /// Muestra SidebarItemModel y SidebarGroupModel como opciones en el
    /// botón "Add ▼" del diseñador, evitando el error de clase abstracta.
    /// </summary>
    public class SidebarNodeCollectionEditor : CollectionEditor
    {
        public SidebarNodeCollectionEditor(Type type) : base(type) { }

        /// <summary>
        /// Devuelve los tipos concretos que el editor puede instanciar.
        /// VS los muestra en el desplegable "Add ▼".
        /// </summary>
        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(SidebarItemModel),   // "SidebarItemModel"
                typeof(SidebarGroupModel)   // "SidebarGroupModel"
            };
        }

        /// <summary>
        /// Título de la ventana del editor según el tipo que se está editando.
        /// </summary>
        protected override string GetDisplayText(object value)
        {
            if (value is SidebarItemModel item)
                return $"📄  {item.ItemText}";
            if (value is SidebarGroupModel grp)
                return $"📁  {grp.GroupTitle}";
            return base.GetDisplayText(value);
        }
    }
}
