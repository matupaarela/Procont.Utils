using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Procont.Utils.Components.DataGrid
{
    // ══════════════════════════════════════════════════════════════════
    // GROUPED COLUMN HEADERS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Define el label agrupador que se pinta encima de un conjunto de columnas.
    ///
    /// ── USO EN CÓDIGO ───────────────────────────────────────────────
    ///   grid.ColumnGroups.Add(new DataGridColumnGroup
    ///   {
    ///       Title           = "Período actual",
    ///       ColumnNamesText = "colUnits, colPrice, colTotal"
    ///   });
    ///   grid.ApplyColumnGroups();
    ///
    /// ── USO EN DISEÑADOR ────────────────────────────────────────────
    ///   Properties → ColumnGroups → [...] → establecer Title + ColumnNamesText.
    ///
    /// NOTA: ColumnNamesText usa string CSV (no List&lt;string&gt;) porque
    ///       el CollectionEditor del diseñador no puede instanciar string.
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
        [Description("Color de fondo del header del grupo. Empty = SystemColors.ControlDark.")]
        public Color BackColor { get; set; } = Color.Empty;

        /// <summary>
        /// Nombres de columna (DataGridViewColumn.Name) separados por coma.
        /// Se usa tanto en código como en el diseñador.
        /// Ejemplo: "Column1, Column2, Column3"
        /// </summary>
        [Category("ColumnGroup")]
        [Description("Nombres de columna separados por coma. Ej: \"Column1, Column2, Column3\"")]
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
        /// Lista parseada de ColumnNamesText. Solo lectura; no se usa en el diseñador.
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
        [Description("Muestra un texto fijo.")] Custom,
        [Description("Suma de valores numéricos.")] Sum,
        [Description("Promedio de valores numéricos.")] Average,
        [Description("Cantidad de celdas no vacías.")] Count,
        [Description("Valor mínimo.")] Min,
        [Description("Valor máximo.")] Max
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
        [Description("Cómo se calcula el valor de la celda.")]
        [DefaultValue(FooterFormula.Custom)]
        public FooterFormula Formula { get; set; } = FooterFormula.Custom;

        [Category("FooterCell")]
        [Description("Texto fijo (Formula=Custom) o prefijo para otras fórmulas.")]
        [DefaultValue("")]
        public string Text { get; set; } = "";

        [Category("FooterCell")]
        [Description("Formato numérico (\"N2\", \"C2\", etc.). Vacío = ToString().")]
        [DefaultValue("")]
        public string Format { get; set; } = "";

        [Category("FooterCell")]
        [Description("Alineación del texto.")]
        [DefaultValue(ContentAlignment.MiddleRight)]
        public ContentAlignment Alignment { get; set; } = ContentAlignment.MiddleRight;

        [Category("FooterCell")]
        [Description("Color de primer plano. Empty = SystemColors.ControlText.")]
        public Color ForeColor { get; set; } = Color.Empty;

        [Category("FooterCell")]
        [Description("Texto en negrita.")]
        [DefaultValue(true)]
        public bool Bold { get; set; } = true;
    }

    /// <summary>
    /// Fila de footer. Agrega una o más a FooterRows y llama RebuildFooter().
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridFooterRow
    {
        [Category("FooterRow")]
        [Description("Alto de fila en píxeles.")]
        [DefaultValue(28)]
        public int Height { get; set; } = 28;

        [Category("FooterRow")]
        [Description("Fondo. Empty = SystemColors.ControlDark.")]
        public Color BackColor { get; set; } = Color.Empty;

        [Category("FooterRow")]
        [Description("Celdas de esta fila, una por columna.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridFooterCell> Cells { get; } = new List<DataGridFooterCell>();
    }
}