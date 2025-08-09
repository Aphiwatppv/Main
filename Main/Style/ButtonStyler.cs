using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Main.Style
{
    public static class ButtonStyler
    {
        // Track per-button handlers so we can detach when reapplying styles
        private class HandlerSet
        {
            public EventHandler EnterHandler;
            public EventHandler LeaveHandler;
            public MouseEventHandler DownHandler;
            public MouseEventHandler UpHandler;
        }

        private static readonly ConditionalWeakTable<Button, HandlerSet> _handlerMap
            = new ConditionalWeakTable<Button, HandlerSet>();

        /// <summary>
        /// Apply a flat style to a Button with hover/press effects.
        /// styleType: "normal" (teal), "warning", or "danger".
        /// </summary>
        public static void ApplyButtonStyle(Button btn, string styleType = "normal")
        {
            // Resolve palette
            GetPalette(styleType, out var baseColor, out var hoverColor, out var pressedColor, out var foreColor);

            // Base look (flat, no border, solid fill)
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = baseColor;
            btn.ForeColor = foreColor;
            btn.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            // If we previously attached handlers, detach them first
            if (_handlerMap.TryGetValue(btn, out var oldSet))
            {
                btn.MouseEnter -= oldSet.EnterHandler;
                btn.MouseLeave -= oldSet.LeaveHandler;
                btn.MouseDown -= oldSet.DownHandler;
                btn.MouseUp -= oldSet.UpHandler;
                _handlerMap.Remove(btn);
            }

            // Build new handlers capturing this palette
            var newSet = new HandlerSet
            {
                EnterHandler = (s, e) => btn.BackColor = hoverColor,
                LeaveHandler = (s, e) => btn.BackColor = baseColor,
                DownHandler = (s, e) => btn.BackColor = pressedColor,
                UpHandler = (s, e) => btn.BackColor = hoverColor
            };

            // Attach
            btn.MouseEnter += newSet.EnterHandler;
            btn.MouseLeave += newSet.LeaveHandler;
            btn.MouseDown += newSet.DownHandler;
            btn.MouseUp += newSet.UpHandler;

            // Remember for the next apply
            _handlerMap.Add(btn, newSet);
        }

        private static void GetPalette(string styleType,
            out Color baseColor, out Color hoverColor, out Color pressedColor, out Color foreColor)
        {
            switch ((styleType ?? "normal").Trim().ToLowerInvariant())
            {
                case "warning":
                    baseColor = Color.FromArgb(255, 193, 7);   // amber
                    hoverColor = Color.FromArgb(255, 205, 40);
                    pressedColor = Color.FromArgb(204, 154, 5);
                    foreColor = Color.Black;                   // better contrast on yellow
                    break;

                case "danger":
                    baseColor = Color.FromArgb(220, 53, 69);   // red
                    hoverColor = Color.FromArgb(230, 80, 90);
                    pressedColor = Color.FromArgb(180, 40, 50);
                    foreColor = Color.White;
                    break;

                default: // "normal" teal
                    baseColor = Color.FromArgb(0, 128, 128);
                    hoverColor = Color.FromArgb(0, 150, 150);
                    pressedColor = Color.FromArgb(0, 100, 100);
                    foreColor = Color.White;
                    break;
            }
        }

        /// <summary>
        /// Optional helper: apply to all Buttons in a container (recursively).
        /// </summary>
        public static void ApplyButtonStyleToAll(Control root, string styleType = "normal")
        {
            if (root is Button b) ApplyButtonStyle(b, styleType);
            foreach (Control c in root.Controls)
                ApplyButtonStyleToAll(c, styleType);
        }
    }
}
