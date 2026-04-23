using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Procont.Utils.Components.DataItem.Models
{
    /// <summary>
    /// Model for one row in a <see cref="DataItemView"/>.
    ///
    /// [Serializable] es obligatorio para que el clipboard y el undo/redo
    /// del diseñador de Visual Studio puedan clonar los items de la
    /// colección Items sin lanzar la excepción "no está marcado como serializable".
    ///
    /// ── DESIGNER (colección Actions) ────────────────────────────────
    ///   Properties → Actions → [...] → "Add" crea DataItemActionModel.
    ///   Configura Key, Label, Icon, IsSplit.
    ///   Los handlers se wirean en código (Form.Load) vía
    ///   dataItemControl.GetAction("key").PrimaryClicked += ...
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataItemModel
    {
        // ── Identity ───────────────────────────────────────────────────
        [Category("DataItem")]
        [Description("Unique key used in ItemClicked events.")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        // ── Content ────────────────────────────────────────────────────
        [Category("DataItem")]
        [Description("Primary text (title line).")]
        [DefaultValue("Item")]
        public string Title { get; set; } = "Item";

        [Category("DataItem")]
        [Description("Secondary text (description line). Empty = title centred vertically.")]
        [DefaultValue("")]
        public string Description { get; set; } = "";

        // ── Media ──────────────────────────────────────────────────────
        [Category("DataItem — Media")]
        [Description("Rendering mode of the left media zone.")]
        [DefaultValue(DataItemMediaVariant.None)]
        public DataItemMediaVariant MediaVariant { get; set; } = DataItemMediaVariant.None;

        [Category("DataItem — Media")]
        [Description("FontAwesome icon (used when MediaVariant = Icon).")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        [Category("DataItem — Media")]
        [Description("1–2 upper-case initials (used when MediaVariant = Avatar).")]
        [DefaultValue("")]
        public string AvatarInitials { get; set; } = "";

        [Category("DataItem — Media")]
        [Description("Bitmap image (used when MediaVariant = Image).")]
        [DefaultValue(null)]
        public Image MediaImage { get; set; }

        // ── Style ──────────────────────────────────────────────────────
        [Category("DataItem — Style")]
        [Description("Visual variant of the row.")]
        [DefaultValue(DataItemVariant.Default)]
        public DataItemVariant Variant { get; set; } = DataItemVariant.Default;

        [Category("DataItem — Style")]
        [Description("Height preset.")]
        [DefaultValue(DataItemSize.Default)]
        public DataItemSize Size { get; set; } = DataItemSize.Default;

        [Category("DataItem — Style")]
        [Description("Optional badge pill (New / Beta).")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge { get; set; } = SidebarBadge.None;

        // ── Action (legacy — texto inline, sin botón) ──────────────────
        [Category("DataItem — Action")]
        [Description("Text of the inline action link rendered on the right. Empty = no link.\n" +
                     "Para botones reales usa la colección Actions.")]
        [DefaultValue("")]
        public string ActionLabel { get; set; } = "";

        // ── Actions (botones embebidos) ────────────────────────────────
        /// <summary>
        /// Botones de acción que se muestran en el área derecha del ítem.
        /// Cada acción puede ser un IconButton (IsSplit=false) o un
        /// SplitActionButton [Label|▼] (IsSplit=true).
        ///
        /// Los handlers se wirean en código después de RebuildFromModels():
        ///   control.GetAction("edit").PrimaryClicked += (s,e) => Edit();
        /// </summary>
        [Category("DataItem — Actions")]
        [Description("Botones de acción embebidos (Icon o Split). Los handlers se wirean en código por Key.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(DataItemActionCollectionEditor), typeof(UITypeEditor))]
        public List<DataItemActionModel> Actions { get; } = new List<DataItemActionModel>();

        public override string ToString() =>
            string.IsNullOrEmpty(Title) ? "(sin título)" : $"• {Title}";
    }
}