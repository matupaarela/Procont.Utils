using System;
using System.ComponentModel.Design;

namespace Procont.Utils.Sidebar.Models
{
    /// <summary>
    /// CollectionEditor para la colección raíz del SidebarControl
    /// (List&lt;SidebarNodeModel&gt; de nivel 0).
    ///
    /// El botón "Add ▼" ofrece dos opciones:
    ///   • SidebarRootItemModel  — ítem directo (sin hijos, estilo grupo padre)
    ///   • SidebarGroupModel     — grupo colapsable con hijos
    ///
    /// Mismo patrón que SidebarNodeCollectionEditor para evitar el crash
    /// "ArgumentException: component" con la clase base abstracta.
    /// </summary>
    public class SidebarRootCollectionEditor : CollectionEditor
    {
        public SidebarRootCollectionEditor(Type type) : base(type) { }

        protected override Type[] CreateNewItemTypes() => new[]
        {
            typeof(SidebarRootItemModel),
            typeof(SidebarGroupModel),
            typeof(SidebarSeparatorModel)
        };

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(SidebarNodeModel) || itemType.IsAbstract)
                return new SidebarRootItemModel { ItemText = "Nuevo ítem" };

            var instance = base.CreateInstance(itemType);

            if (instance is SidebarRootItemModel root)
                root.ItemText = "Nuevo ítem";
            else if (instance is SidebarGroupModel grp)
                grp.GroupTitle = "Nuevo grupo";

            return instance;
        }

        protected override string GetDisplayText(object value)
        {
            if (value == null) return "(nulo)";
            if (value is SidebarRootItemModel root) return $"▶  {root.ItemText}";
            if (value is SidebarGroupModel grp) return $"📁  {grp.GroupTitle}";
            if (value is SidebarSeparatorModel sep)
                return string.IsNullOrEmpty(sep.Label) ? "─────────────" : $"─── {sep.Label} ───";
            return base.GetDisplayText(value);
        }

        protected override object[] GetItems(object editValue)
        {
            var items = base.GetItems(editValue);
            if (items == null) return Array.Empty<object>();

            var clean = new System.Collections.Generic.List<object>();
            foreach (var item in items)
                if (item != null) clean.Add(item);

            return clean.ToArray();
        }
    }
}