using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Procont.Utils.Components.DataGrid
{
    // ══════════════════════════════════════════════════════════════════
    // GROUPED COLUMN HEADERS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Define el label agrupador que se pinta encima de un conjunto de columnas
    /// en un <see cref="ProcontDataGridView"/>.
    ///
    /// ── USO EN CÓDIGO ───────────────────────────────────────────────
    ///   grid.ColumnGroups.Add(new DataGridColumnGroup
    ///   {
    ///       Title           = "Período actual",
    ///       BackColor       = Color.FromArgb(10, 245, 168, 30),
    ///       ColumnNamesText = "colUnits, colPrice, colTotal"
    ///   });
    ///   grid.ApplyColumnGroups();
    ///
    /// ── USO EN DISEÑADOR ────────────────────────────────────────────
    ///   Properties → ColumnGroups → [...]
    ///   Establece ColumnNamesText con los nombres de columna separados por coma.
    ///
    /// NOTA: NO usar List&lt;string&gt; en el diseñador — string no tiene ctor vacío
    ///       y el CollectionEditor lanza excepción. Usar ColumnNamesText (string).
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridColumnGroup
    {
        private readonly List<string> _columnNames = new List<string>();

        [Category("ColumnGroup")]
        [Description("Texto visible en la celda de cabecera agrupada.")]
        [DefaultValue("")]
        public string Title { get; set; } = "";

        [Category("ColumnGroup")]
        [Description("Fondo del header del grupo. Transparent = color por defecto.")]
        public Color BackColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Nombres de columna separados por coma.
        /// Usar en el diseñador en lugar de ColumnNames.
        /// Ejemplo: "Column1, Column2, Column3"
        /// </summary>
        [Category("ColumnGroup")]
        [Description("Nombres de columna (DataGridViewColumn.Name) separados por coma.")]
        [DefaultValue("")]
        public string ColumnNamesText
        {
            get => string.Join(", ", _columnNames);
            set
            {
                _columnNames.Clear();
                if (string.IsNullOrEmpty(value)) return;
                foreach (var s in value.Split(','))
                {
                    var t = s.Trim();
                    if (!string.IsNullOrEmpty(t))
                        _columnNames.Add(t);
                }
            }
        }

        /// <summary>
        /// Lista parseada desde <see cref="ColumnNamesText"/>.
        /// Usar en código. En el diseñador usar ColumnNamesText.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> ColumnNames => _columnNames;

        public override string ToString() =>
            string.IsNullOrEmpty(Title) ? "(grupo sin título)" : Title;
    }

    // ══════════════════════════════════════════════════════════════════
    // FOOTER MODELS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Fórmula de agregación de una celda de footer.</summary>
    public enum FooterFormula
    {
        [Description("Muestra un texto fijo.")]
        Custom,

        [Description("Suma de valores numéricos de la columna.")]
        Sum,

        [Description("Promedio de valores numéricos.")]
        Average,

        [Description("Cantidad de celdas no vacías.")]
        Count,

        [Description("Valor numérico mínimo.")]
        Min,

        [Description("Valor numérico máximo.")]
        Max
    }

    /// <summary>
    /// Celda del footer; se mapea a una columna del DataGridView por Name.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridFooterCell
    {
        [Category("FooterCell")]
        [Description("Nombre de la columna (DataGridViewColumn.Name).")]
        [DefaultValue("")]
        public string ColumnName { get; set; } = "";

        [Category("FooterCell")]
        [Description("Cómo se calcula el valor.")]
        [DefaultValue(FooterFormula.Custom)]
        public FooterFormula Formula { get; set; } = FooterFormula.Custom;

        [Category("FooterCell")]
        [Description("Texto fijo (Formula = Custom) o prefijo de etiqueta para otras fórmulas.")]
        [DefaultValue("")]
        public string Text { get; set; } = "";

        [Category("FooterCell")]
        [Description("Formato numérico (\"N2\", \"C2\", etc.). Vacío = ToString().")]
        [DefaultValue("")]
        public string Format { get; set; } = "";

        [Category("FooterCell")]
        [Description("Alineación del texto dentro de la celda.")]
        [DefaultValue(ContentAlignment.MiddleRight)]
        public ContentAlignment Alignment { get; set; } = ContentAlignment.MiddleRight;

        [Category("FooterCell")]
        [Description("Color de primer plano. Transparent = ProcontTheme.TextPrimary.")]
        public Color ForeColor { get; set; } = Color.Transparent;

        [Category("FooterCell")]
        [Description("Texto en negrita.")]
        [DefaultValue(true)]
        public bool Bold { get; set; } = true;
    }

    /// <summary>
    /// Fila de footer en un <see cref="ProcontDataGridView"/>.
    /// Agrega una o más filas para mostrar totales, promedios, etc.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridFooterRow
    {
        [Category("FooterRow")]
        [Description("Alto de fila en píxeles.")]
        [DefaultValue(32)]
        public int Height { get; set; } = 32;

        [Category("FooterRow")]
        [Description("Fondo. Transparent = SurfaceActive.")]
        public Color BackColor { get; set; } = Color.Transparent;

        [Category("FooterRow")]
        [Description("Celdas de esta fila, una por columna.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridFooterCell> Cells { get; } = new List<DataGridFooterCell>();
    }
}