namespace Procont.Utils.Core.Theming
{
    /// <summary>
    /// Contrato para controles que soportan recarga de tema en caliente.
    /// Implementar cuando el control necesite responder a cambios de
    /// ProcontTheme en tiempo de ejecución (ej: cambio de tema oscuro/claro).
    /// </summary>
    public interface IThemeable
    {
        /// <summary>
        /// Re-aplica los valores actuales de <see cref="ProcontTheme"/> al control
        /// y llama a <c>Invalidate()</c> para forzar repintado.
        /// </summary>
        void ApplyTheme();
    }
}