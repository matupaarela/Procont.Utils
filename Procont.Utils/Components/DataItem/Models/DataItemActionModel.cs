using FontAwesome.Sharp;
using System;
using System.ComponentModel;

namespace Procont.Utils.Components.DataItem.Models
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataItemActionModel
    {
        [Category("DataItem — Action")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        [Category("DataItem — Action")]
        [DefaultValue("Action")]
        public string Label { get; set; } = "Action";

        [Category("DataItem — Action")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        [Category("DataItem — Action")]
        [Description("false = ícono simple.  true = split [Label|▼] con dropdown.")]
        [DefaultValue(false)]
        public bool IsSplit { get; set; } = false;

        public override string ToString()
        {
            string type = IsSplit ? "Split" : "Icon";
            return string.IsNullOrEmpty(Key) ? $"({type})" : $"{type}: {Key}";
        }
    }
}