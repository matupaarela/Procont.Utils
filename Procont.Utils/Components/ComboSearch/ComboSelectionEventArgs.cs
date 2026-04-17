using System;
using System.Collections.Generic;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Args para <c>SelectionCommitted</c>: el usuario confirmó un ítem
    /// (Enter o click). Contiene el objeto completo y su valor extraído.
    /// </summary>
    public class ComboSelectionEventArgs : EventArgs
    {
        public object Item { get; }
        public object Value { get; }
        public string DisplayText { get; }
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
        public string SearchText { get; }

        internal ComboActionEventArgs(string searchText)
        {
            SearchText = searchText ?? string.Empty;
        }
    }

    /// <summary>
    /// Args para <c>MultiSelectionChanged</c>: se dispara cada vez que el
    /// usuario marca o desmarca un ítem en modo <see cref="ComboSearchBox.MultiSelect"/>.
    /// Contiene los snapshots actualizados de valores y textos seleccionados.
    /// </summary>
    public class MultiSelectionChangedEventArgs : EventArgs
    {
        /// <summary>Valores (ValueMember) de todos los ítems actualmente marcados.</summary>
        public IReadOnlyList<object> SelectedValues { get; }

        /// <summary>Textos visibles (DisplayMember) de los ítems marcados, en orden del datasource.</summary>
        public IReadOnlyList<string> SelectedDisplayTexts { get; }

        /// <summary>Cantidad de ítems seleccionados.</summary>
        public int Count => SelectedValues.Count;

        internal MultiSelectionChangedEventArgs(
            List<object> selectedValues,
            List<string> selectedDisplayTexts)
        {
            SelectedValues = selectedValues.AsReadOnly();
            SelectedDisplayTexts = selectedDisplayTexts.AsReadOnly();
        }
    }
}