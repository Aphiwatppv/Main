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
            GetPalette(styleType, out var baseColor, out _, out _, out var foreColor);

            lbl.ForeColor = baseColor;
            lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lbl.BackColor = Color.Transparent; // usually labels are transparent
        }

        // ===== TEXTBOX =====
        public static void ApplyTextBoxStyle(TextBox txt, string styleType = "normal")
        {
            GetPalette(styleType, out var baseColor, out _, out _, out var foreColor);

            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Color.White;
            txt.ForeColor = foreColor;
            txt.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // Simulate colored border by painting
            txt.Paint += (s, e) =>
            {
                using (Pen p = new Pen(baseColor, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, txt.Width - 1, txt.Height - 1);
                }
            };
        }

        // ===== COMBOBOX =====
        public static void ApplyComboBoxStyle(ComboBox cbx, string styleType = "normal")
        {
            GetPalette(styleType, out var baseColor, out var hoverColor, out var pressedColor, out var foreColor);

            cbx.FlatStyle = FlatStyle.Flat;
            cbx.BackColor = baseColor; // solid background
            cbx.ForeColor = foreColor;
            cbx.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            cbx.DropDownStyle = ComboBoxStyle.DropDownList;
            cbx.Cursor = Cursors.Hand;

            // Hover & click effects
            cbx.MouseEnter += (s, e) => cbx.BackColor = hoverColor;
            cbx.MouseLeave += (s, e) => cbx.BackColor = baseColor;
            cbx.MouseDown += (s, e) => cbx.BackColor = pressedColor;
            cbx.MouseUp += (s, e) => cbx.BackColor = hoverColor;
        }

        // ===== COLOR PALETTE =====
        private static void GetPalette(string styleType,
            out Color baseColor, out Color hoverColor, out Color pressedColor, out Color foreColor)
        {
            switch ((styleType ?? "normal").Trim().ToLowerInvariant())
            {
                case "warning":
                    baseColor = Color.FromArgb(255, 193, 7);   // amber
                    hoverColor = Color.FromArgb(255, 205, 40);
                    pressedColor = Color.FromArgb(204, 154, 5);
                    foreColor = Color.Black;
                    break;

                case "danger":
                    baseColor = Color.FromArgb(220, 53, 69);   // red
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
