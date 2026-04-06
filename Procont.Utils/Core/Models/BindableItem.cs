using FontAwesome.Sharp;
using System.ComponentModel;

namespace Procont.Utils.Core.Models
{
    /// <summary>
    /// Par Display / Value genérico reutilizable por cualquier componente
    /// que necesite mostrar texto pero guardar un valor distinto
    /// (ComboSearch, ListBox custom, Chips, etc.).
    ///
    /// ── USO TÍPICO ────────────────────────────────────────────────────
    ///   var items = new List&lt;BindableItem&gt;
    ///   {
    ///       new BindableItem("Cliente A", 1),
    ///       new BindableItem("Cliente B", 2),
    ///   };
    ///   comboSearch.SetDataSource(items);
    ///
    /// ── USO CON TIPO FUERTEMENTE TIPADO ──────────────────────────────
    ///   BindableItem.From(listaClientes, c => c.Nombre, c => c.Id);
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BindableItem
    {
        // ── Propiedades ────────────────────────────────────────────────

        /// <summary>Texto que se muestra al usuario en el control.</summary>
        [Category("Datos")]
        [Description("Texto visible para el usuario.")]
        public string Display { get; set; }

        /// <summary>
        /// Valor lógico que se almacena en SelectedValue.
        /// Puede ser un int (ID), Guid, enum, o cualquier objeto.
        /// </summary>
        [Category("Datos")]
        [Description("Valor lógico asociado (ID, enum, etc.).")]
        public object Value { get; set; }

        /// <summary>
        /// Ícono FontAwesome opcional para este ítem.
        /// El control lo renderizará si está disponible.
        /// </summary>
        [Category("Datos")]
        [Description("Ícono FontAwesome opcional.")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        /// <summary>
        /// Descripción secundaria opcional (segunda línea, texto más pequeño).
        /// No todos los componentes la renderizan.
        /// </summary>
        [Category("Datos")]
        [Description("Descripción secundaria opcional (subtítulo).")]
        [DefaultValue("")]
        public string Subtitle { get; set; } = "";

        // ── Constructores ──────────────────────────────────────────────

        public BindableItem() { }

        public BindableItem(string display, object value)
        {
            Display = display;
            Value = value;
        }

        public BindableItem(string display, object value, IconChar icon)
        {
            Display = display;
            Value = value;
            Icon = icon;
        }

        public BindableItem(string display, object value, string subtitle)
        {
            Display = display;
            Value = value;
            Subtitle = subtitle;
        }

        // ── Factory ────────────────────────────────────────────────────

        /// <summary>
        /// Crea una lista de <see cref="BindableItem"/> a partir de cualquier
        /// colección tipada, usando selectores para Display y Value.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos fuente.</typeparam>
        /// <param name="source">Colección origen.</param>
        /// <param name="displaySelector">Función que extrae el texto visible.</param>
        /// <param name="valueSelector">Función que extrae el valor lógico.</param>
        /// <returns>Lista lista para asignar a cualquier componente Procont.</returns>
        public static System.Collections.Generic.List<BindableItem> From<T>(
            System.Collections.Generic.IEnumerable<T> source,
            System.Func<T, string> displaySelector,
            System.Func<T, object> valueSelector)
        {
            var result = new System.Collections.Generic.List<BindableItem>();
            foreach (var item in source)
                result.Add(new BindableItem(displaySelector(item), valueSelector(item)));
            return result;
        }

        public override string ToString() => Display ?? "(sin texto)";
    }
}