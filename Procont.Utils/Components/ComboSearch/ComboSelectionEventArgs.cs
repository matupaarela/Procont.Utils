using System;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Args para <c>SelectionCommitted</c>: el usuario confirmó un ítem
    /// (Enter o click). Contiene el objeto completo y su valor extraído.
    /// </summary>
    public class ComboSelectionEventArgs : EventArgs
    {
        /// <summary>Objeto original del datasource.</summary>
        public object Item { get; }

        /// <summary>Valor del ValueMember (o el objeto completo si ValueMember es vacío).</summary>
        public object Value { get; }

        /// <summary>Texto visible del ítem (DisplayMember).</summary>
        public string DisplayText { get; }

        /// <summary>Índice dentro del datasource filtrado actual.</summary>
        public int Index { get; }

        internal ComboSelectionEventArgs(object item, object value, string displayText, int index)
        {
            Item = item;
            Value = value;
            DisplayText = displayText;
            Index = index;
        }
    }

    /// <summary>
    /// Args para <c>ActionButtonClicked</c>: el usuario hizo clic en el
    /// botón de acción sticky. Incluye el texto buscado para pre-llenar
    /// formularios de alta.
    /// </summary>
    public class ComboActionEventArgs : EventArgs
    {
        /// <summary>Texto que el usuario escribió en el input al momento del clic.</summary>
        public string SearchText { get; }

        internal ComboActionEventArgs(string searchText)
        {
            SearchText = searchText ?? string.Empty;
        }
    }
}