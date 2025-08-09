using ConfigServices.Model;
using Main.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Main.SubForm
{
    public partial class ServerEditorForm : Form
    {
        private TextBox txtLocation;
        private TextBox txtInstance;
        private ComboBox cmbInstanceType;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtTns;
        private Label lblError;
        private Button btnOk;
        private Button btnCancel;

        private readonly bool _isEdit;
        private readonly Guid _existingId;

        public ServerConfig Value { get; private set; }

        public ServerEditorForm(ServerConfig? existing = null)
        {
            _isEdit = existing != null;
            _existingId = existing?.Id ?? Guid.Empty;

            InitializeUi();

            if (_isEdit)
            {
                Text = "Edit Server";
                txtLocation.Text = existing!.Location;
                txtInstance.Text = existing.Instance;
                txtUsername.Text = existing.Username;
                txtPassword.Text = existing.Password ?? "";
                txtTns.Text = existing.TNS;

                // Ensure the combo contains the current type; if not, add it
                var types = new[] { "Production", "UAT", "DEV", "TEST" };
                if (!types.Contains(existing.InstanceType, StringComparer.OrdinalIgnoreCase))
                    cmbInstanceType.Items.Add(existing.InstanceType);

                cmbInstanceType.SelectedItem = cmbInstanceType.Items
                    .Cast<object>()
                    .FirstOrDefault(x => string.Equals(x.ToString(), existing.InstanceType, StringComparison.OrdinalIgnoreCase))
                    ?? cmbInstanceType.Items[0];
            }
            else
            {
                Text = "Add Server";
            }
        }

        private void InitializeUi()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 320);
            Font = new Font("Segoe UI", 9F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(12),
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            txtLocation = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtInstance = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cmbInstanceType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            cmbInstanceType.Items.AddRange(new object[] { "Production", "UAT", "DEV", "TEST" });
            cmbInstanceType.SelectedIndex = 0;

            txtUsername = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtPassword = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, UseSystemPasswordChar = true };
            txtTns = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

            lblError = new Label
            {
                ForeColor = Color.Firebrick,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Text = ""
            };

            btnOk = new Button { Text = "OK", DialogResult = DialogResult.None, Width = 100 };
            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 100 };

            // Apply your flat styles if available
            try
            {
               ButtonStyler.ApplyButtonStyle(btnOk, "normal");
               ButtonStyler.ApplyButtonStyle(btnCancel, "danger");
            }
            catch { /* ignore if helper not available here */ }

            btnOk.Click += (s, e) =>
            {
                lblError.Text = "";
                var loc = (txtLocation.Text ?? "").Trim();
                var inst = (txtInstance.Text ?? "").Trim();
                if (string.IsNullOrEmpty(loc) || string.IsNullOrEmpty(inst))
                {
                    lblError.Text = "Location and Instance are required.";
                    (string.IsNullOrEmpty(loc) ? txtLocation : txtInstance).Focus();
                    return;
                }

                Value = new ServerConfig
                {
                    Id = _isEdit ? _existingId : Guid.Empty, // service will assign if Empty on Add
                    Location = loc,
                    Instance = inst,
                    InstanceType = cmbInstanceType.SelectedItem?.ToString() ?? "Production",
                    Username = (txtUsername.Text ?? "").Trim(),
                    Password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text,
                    TNS = (txtTns.Text ?? "").Trim()
                };

                DialogResult = DialogResult.OK;
                Close();
            };

            layout.Controls.Add(new Label { Text = "Location", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(txtLocation, 1, 0);

            layout.Controls.Add(new Label { Text = "Instance", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(txtInstance, 1, 1);

            layout.Controls.Add(new Label { Text = "Instance Type", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(cmbInstanceType, 1, 2);

            layout.Controls.Add(new Label { Text = "Username", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(txtUsername, 1, 3);

            layout.Controls.Add(new Label { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            layout.Controls.Add(txtPassword, 1, 4);

            layout.Controls.Add(new Label { Text = "TNS", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 5);
            layout.Controls.Add(txtTns, 1, 5);

            layout.Controls.Add(lblError, 0, 6);
            layout.SetColumnSpan(lblError, 2);

            var pnlButtons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnOk);

            layout.Controls.Add(pnlButtons, 0, 7);
            layout.SetColumnSpan(pnlButtons, 2);

            Controls.Add(layout);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
