using System;
using System.ComponentModel.Design;

namespace Procont.Utils.Sidebar.Models
{
    /// <summary>
    /// CollectionEditor personalizado para List&lt;SidebarNodeModel&gt;.
    ///
    /// Problemas que resuelve:
    /// 1. "Add ▼" muestra los tipos concretos en lugar de intentar
    ///    instanciar la clase abstracta SidebarNodeModel.
    /// 2. Evita el crash "ArgumentException: component" que ocurre cuando
    ///    el editor intenta obtener un TypeDescriptor sobre un item null
    ///    o sobre la clase abstracta.
    /// </summary>
    public class SidebarNodeCollectionEditor : CollectionEditor
    {
        public SidebarNodeCollectionEditor(Type type) : base(type) { }

        // ── Tipos concretos que el botón "Add ▼" puede instanciar ─────
        protected override Type[] CreateNewItemTypes() => new[]
        {
            typeof(SidebarItemModel),
            typeof(SidebarGroupModel),
            typeof(SidebarSeparatorModel)
        };

        // ── Instanciar siempre un tipo concreto, nunca la clase base ──
        protected override object CreateInstance(Type itemType)
        {
            // Si por alguna razón se pide el tipo abstracto, devolver un ítem por defecto
            if (itemType == typeof(SidebarNodeModel) || itemType.IsAbstract)
                return new SidebarItemModel { ItemText = "Nuevo ítem" };

            var instance = base.CreateInstance(itemType);

            // Valores por defecto más descriptivos según el tipo
            if (instance is SidebarItemModel item)
                item.ItemText = "Nuevo ítem";
            else if (instance is SidebarGroupModel grp)
                grp.GroupTitle = "Nuevo grupo";

            return instance;
        }

        // ── Texto en la lista del editor — protegido contra null ──────
        protected override string GetDisplayText(object value)
        {
            if (value == null) return "(nulo)";
            if (value is SidebarItemModel item) return $"📄  {item.ItemText}";
            if (value is SidebarGroupModel grp) return $"📁  {grp.GroupTitle}";
            if (value is SidebarSeparatorModel sep)
                return string.IsNullOrEmpty(sep.Label) ? "─────────────" : $"─── {sep.Label} ───";
            return base.GetDisplayText(value);
        }

        // ── Filtrar nulls antes de que el editor los renderice ────────
        protected override object[] GetItems(object editValue)
        {
            var items = base.GetItems(editValue);
            if (items == null) return Array.Empty<object>();

            // Eliminar cualquier entrada null que pudiera colarse
            var clean = new System.Collections.Generic.List<object>();
            foreach (var item in items)
                if (item != null) clean.Add(item);

            return clean.ToArray();
        }
    }
}
