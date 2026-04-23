using System;
using System.ComponentModel.Design;

namespace Procont.Utils.Components.DataItem.Models
{
    /// <summary>
    /// CollectionEditor para <c>List&lt;DataItemActionModel&gt;</c>.
    /// Muestra texto descriptivo en la lista y evita instanciar tipos nulos.
    /// </summary>
    public class DataItemActionCollectionEditor : CollectionEditor
    {
        public DataItemActionCollectionEditor(Type type) : base(type) { }

        protected override Type[] CreateNewItemTypes()
            => new[] { typeof(DataItemActionModel) };

        protected override object CreateInstance(Type itemType)
        {
            var instance = base.CreateInstance(itemType) as DataItemActionModel
                           ?? new DataItemActionModel();
            instance.Key = "action";
            instance.Label = "Action";
            return instance;
        }

        protected override string GetDisplayText(object value)
        {
            if (value == null) return "(nulo)";
            if (value is DataItemActionModel m)
            {
                string icon = m.Icon != FontAwesome.Sharp.IconChar.None ? $" [{m.Icon}]" : "";
                string type = m.IsSplit ? "Split" : "Icon";
                string label = m.IsSplit ? $" \"{m.Label}\"" : "";
                return string.IsNullOrEmpty(m.Key)
                    ? $"({type}{icon}{label})"
                    : $"{m.Key} → {type}{icon}{label}";
            }
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