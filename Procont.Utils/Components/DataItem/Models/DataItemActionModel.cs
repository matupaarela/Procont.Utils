using FontAwesome.Sharp;
using System;
using System.ComponentModel;

namespace Procont.Utils.Components.DataItem.Models
{
    /// <summary>
    /// Modelo de una acción embebida en el área derecha de un <see cref="DataItemControl"/>.
    ///
    /// ── DISEÑADOR ────────────────────────────────────────────────────
    ///   Configura Key, Label, Icon e IsSplit desde Properties → Actions → [...].
    ///   El CollectionEditor mostrará "Add" con este tipo.
    ///
    /// ── CÓDIGO (wiring de handlers) ──────────────────────────────────
    ///   En Form.Load, tras RebuildFromModels() o SetDataSource():
    ///
    ///   // Botón simple:
    ///   dataItemControl1.GetAction("edit").PrimaryClicked += (s,e) => Edit();
    ///
    ///   // Split button:
    ///   var btn = dataItemControl1.GetAction("follow");
    ///   btn.PrimaryClicked += (s,e) => Follow();
    ///   btn.AddOption("Unfollow", IconChar.UserMinus, (s,e) => Unfollow());
    ///
    /// NOTA: [Serializable] es obligatorio para que el clipboard/undo del
    ///       diseñador pueda clonar los items de la colección sin excepción.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataItemActionModel
    {
        /// <summary>
        /// Clave para obtener el control construido vía
        /// <see cref="DataItemControl.GetAction(string)"/>.
        /// </summary>
        [Category("DataItem — Action")]
        [Description("Clave única para wiring de handlers en código.")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        /// <summary>Texto visible del botón.</summary>
        [Category("DataItem — Action")]
        [Description("Texto del botón.")]
        [DefaultValue("Action")]
        public string Label { get; set; } = "Action";

        /// <summary>Ícono FontAwesome del botón.</summary>
        [Category("DataItem — Action")]
        [Description("Ícono del botón (IconChar.*).")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        /// <summary>
        /// Si es true, el botón se renderiza como [Label | ▼] con menú desplegable.
        /// Si es false, solo se muestra el ícono (IconButton sin texto).
        /// </summary>
        [Category("DataItem — Action")]
        [Description("true = split button [Label|▼] con menú desplegable. false = icon button.")]
        [DefaultValue(false)]
        public bool IsSplit { get; set; } = false;

        public override string ToString()
        {
            string type = IsSplit ? "Split" : "Icon";
            return string.IsNullOrEmpty(Key) ? $"({type})" : $"{type}: {Key}";
        }
    }
}