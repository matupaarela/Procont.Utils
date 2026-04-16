using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Cache de objetos GDI (Brush, Pen, Font) para el ComboSearch.
    ///
    /// ── POR QUÉ EXISTE ───────────────────────────────────────────────
    /// Crear new SolidBrush / new Pen en cada OnPaint genera presión
    /// en el GC y fragmentación del heap de GDI. Este renderer mantiene
    /// una instancia por color/tamaño y la reutiliza en cada frame.
    ///
    /// ── USO ──────────────────────────────────────────────────────────
    ///   using var g = e.Graphics;
    ///   g.FillRectangle(ComboSearchRenderer.BrushBackground, bounds);
    ///   g.DrawString(text, ComboSearchRenderer.FontItem,
    ///                ComboSearchRenderer.BrushText, rect, fmt);
    ///
    /// ── THREAD SAFETY ────────────────────────────────────────────────
    /// WinForms es single-thread (UI thread). No se necesita locking.
    ///
    /// ── INVALIDACIÓN ─────────────────────────────────────────────────
    /// Llamar Invalidate() si ProcontTheme cambia en caliente.
    /// </summary>
    internal static class ComboSearchRenderer
    {
        // ── Brushes ───────────────────────────────────────────────────
        public static SolidBrush BrushBackground { get; private set; }
        public static SolidBrush BrushHover { get; private set; }
        public static SolidBrush BrushSelected { get; private set; }
        public static SolidBrush BrushText { get; private set; }
        public static SolidBrush BrushSubtitle { get; private set; }
        public static SolidBrush BrushPlaceholder { get; private set; }
        public static SolidBrush BrushAccent { get; private set; }
        public static SolidBrush BrushAction { get; private set; }
        public static SolidBrush BrushActionHover { get; private set; }
        public static SolidBrush BrushEmpty { get; private set; }
        public static SolidBrush BrushSearchBg { get; private set; }
        public static SolidBrush BrushInputBg { get; private set; }

        // ── Pens ──────────────────────────────────────────────────────
        public static Pen PenBorder { get; private set; }
        public static Pen PenBorderFocus { get; private set; }
        public static Pen PenSeparator { get; private set; }
        public static Pen PenActiveBar { get; private set; }
        public static Pen PenChevron { get; private set; }
        public static Pen PenChevronActive { get; private set; }
        public static Pen PenClear { get; private set; }

        // ── StringFormats (también cacheados) ─────────────────────────
        public static readonly StringFormat FmtCenter = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        public static readonly StringFormat FmtLeft = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        public static readonly StringFormat FmtLeftTop = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisCharacter
        };

        // ── Init / Reload ─────────────────────────────────────────────

        static ComboSearchRenderer() => Build();

        /// <summary>
        /// Reconstruye todos los objetos GDI desde los valores actuales
        /// de <see cref="ProcontTheme"/>. Llamar si el tema cambia en caliente.
        /// </summary>
        public static void Invalidate()
        {
            DisposeAll();
            Build();
        }

        private static void Build()
        {
            BrushBackground = new SolidBrush(ComboSearchTheme.DropdownBackground);
            BrushHover = new SolidBrush(ComboSearchTheme.ItemHover);
            BrushSelected = new SolidBrush(ComboSearchTheme.ItemSelected);
            BrushText = new SolidBrush(ComboSearchTheme.ItemText);
            BrushSubtitle = new SolidBrush(ComboSearchTheme.ItemSubtitle);
            BrushPlaceholder = new SolidBrush(ComboSearchTheme.InputPlaceholder);
            BrushAccent = new SolidBrush(ComboSearchTheme.ActionText);      // accent = TextAccent
            BrushAction = new SolidBrush(ComboSearchTheme.ActionBackground);
            BrushActionHover = new SolidBrush(ComboSearchTheme.ActionBackgroundHover);
            BrushEmpty = new SolidBrush(ComboSearchTheme.EmptyText);
            BrushSearchBg = new SolidBrush(ComboSearchTheme.SearchBackground);
            BrushInputBg = new SolidBrush(ComboSearchTheme.InputBackground);

            PenBorder = new Pen(ComboSearchTheme.InputBorder, 1f);
            PenBorderFocus = new Pen(ComboSearchTheme.InputBorderFocus, 1.5f);
            PenSeparator = new Pen(ComboSearchTheme.DropdownBorder, 1f);
            PenActiveBar = new Pen(ComboSearchTheme.ActionText, 3f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            PenChevron = new Pen(ComboSearchTheme.ChevronColor, 1.5f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            PenChevronActive = new Pen(ComboSearchTheme.InputBorderFocus, 1.5f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            PenClear = new Pen(ComboSearchTheme.ChevronColor, 1.5f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
        }

        private static void DisposeAll()
        {
            BrushBackground?.Dispose(); BrushHover?.Dispose();
            BrushSelected?.Dispose(); BrushText?.Dispose();
            BrushSubtitle?.Dispose(); BrushPlaceholder?.Dispose();
            BrushAccent?.Dispose(); BrushAction?.Dispose();
            BrushActionHover?.Dispose(); BrushEmpty?.Dispose();
            BrushSearchBg?.Dispose(); BrushInputBg?.Dispose();

            PenBorder?.Dispose(); PenBorderFocus?.Dispose();
            PenSeparator?.Dispose(); PenActiveBar?.Dispose();
            PenChevron?.Dispose(); PenChevronActive?.Dispose();
            PenClear?.Dispose();
        }
    }
}