using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Style
{
    public static class UIStyler
    {
        // ===== LABEL =====
        public static void ApplyLabelStyle(Label lbl, string styleType = "normal")
        {
            GetPalette(styleType, out var baseColor, out var _, out var _, out var foreColor);

            lbl.ForeColor = baseColor;
            lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lbl.BackColor = Color.Transparent; // Keep background transparent for most layouts
        }

        // ===== COMBOBOX =====
        public static void ApplyComboBoxStyle(ComboBox cbx, string styleType = "normal")
        {
            Color tealColor = Color.FromArgb(0, 128, 128); // base teal
            Color hoverColor = Color.FromArgb(0, 150, 150); // lighter teal
            Color pressedColor = Color.FromArgb(0, 100, 100); // darker teal

            cbx.FlatStyle = FlatStyle.Flat;
            cbx.BackColor = tealColor;
            cbx.ForeColor = Color.White;
            cbx.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            cbx.DropDownStyle = ComboBoxStyle.DropDownList; // prevents text edit for clean look
            cbx.Cursor = Cursors.Hand;

            // Optional hover and click effect
            cbx.MouseEnter += (s, e) => cbx.BackColor = hoverColor;
            cbx.MouseLeave += (s, e) => cbx.BackColor = tealColor;
            cbx.MouseDown += (s, e) => cbx.BackColor = pressedColor;
            cbx.MouseUp += (s, e) => cbx.BackColor = hoverColor;
        }

        // ===== COLOR PALETTE (Shared) =====
        private static void GetPalette(string styleType,
            out Color baseColor, out Color hoverColor, out Color pressedColor, out Color foreColor)
        {
            switch ((styleType ?? "normal").Trim().ToLowerInvariant())
            {
                case "warning":
                    baseColor = Color.FromArgb(255, 193, 7);
                    hoverColor = Color.FromArgb(255, 205, 40);
                    pressedColor = Color.FromArgb(204, 154, 5);
                    foreColor = Color.Black;
                    break;

                case "danger":
                    baseColor = Color.FromArgb(220, 53, 69);
                    hoverColor = Color.FromArgb(230, 80, 90);
                    pressedColor = Color.FromArgb(180, 40, 50);
                    foreColor = Color.White;
                    break;

                default: // normal teal
                    baseColor = Color.FromArgb(0, 128, 128);
                    hoverColor = Color.FromArgb(0, 150, 150);
                    pressedColor = Color.FromArgb(0, 100, 100);
                    foreColor = Color.White;
                    break;
            }
        }
    }
}
