using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Procont.Utils.Components.DataItem.Models
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataItemModel
    {
        [Category("DataItem")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        [Category("DataItem")]
        [DefaultValue("Item")]
        public string Title { get; set; } = "Item";

        [Category("DataItem")]
        [DefaultValue("")]
        public string Description { get; set; } = "";

        [Category("DataItem — Media")]
        [DefaultValue(DataItemMediaVariant.None)]
        public DataItemMediaVariant MediaVariant { get; set; } = DataItemMediaVariant.None;

        [Category("DataItem — Media")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        [Category("DataItem — Media")]
        [DefaultValue("")]
        public string AvatarInitials { get; set; } = "";

        [Category("DataItem — Media")]
        [DefaultValue(null)]
        public Image MediaImage { get; set; }

        [Category("DataItem — Style")]
        [DefaultValue(DataItemVariant.Default)]
        public DataItemVariant Variant { get; set; } = DataItemVariant.Default;

        [Category("DataItem — Style")]
        [DefaultValue(DataItemSize.Default)]
        public DataItemSize Size { get; set; } = DataItemSize.Default;

        [Category("DataItem — Style")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge { get; set; } = SidebarBadge.None;

        [Category("DataItem — Action")]
        [DefaultValue("")]
        [Description("Texto inline (legacy). Para botones reales usar Actions.")]
        public string ActionLabel { get; set; } = "";

        [Category("DataItem — Actions")]
        [Description("Botones de acción. Handlers se wirean por Key en Form.Load.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(DataItemActionCollectionEditor), typeof(UITypeEditor))]
        public List<DataItemActionModel> Actions { get; } = new List<DataItemActionModel>();

        public override string ToString() =>
            string.IsNullOrEmpty(Title) ? "(sin título)" : $"• {Title}";
    }
}