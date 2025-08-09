using ConfigServices.Model;
using ConfigServices.Service;
using Main.Style;
using Main.SubForm;
using System.ComponentModel;

namespace Main
{
    public partial class Main : Form
    {


        // Services / data
        private IAppPaths appPaths;
        private IServerConfigService serverConfigService;
        private List<ServerConfig> serversAll;              // master
        private BindingList<ServerConfig> serversView;      // filtered view
        private BindingSource bindingSource;                // for the grid

        public Main()
        {
            InitializeComponent();
            InitialSet();
        }


        private void InitialSet()
        {
            // 1) Services
            appPaths = new AppPaths("Infineon", "EBS Investigation system");
            serverConfigService = new ServerConfigService(appPaths);

            // 2) Load data
            var loaded = serverConfigService.Load();
            serversAll = loaded.Items ?? new List<ServerConfig>();
            serversView = new BindingList<ServerConfig>(serversAll.ToList()); // start unfiltered
            bindingSource = new BindingSource { DataSource = serversView };

            // 3) Bind grid
            dgvServerConfig.AutoGenerateColumns = true;
            dgvServerConfig.DataSource = bindingSource;

            dgvServerConfig.DataBindingComplete += (s, e) =>
            {
                if (dgvServerConfig.Columns.Contains(nameof(ServerConfig.Id)))
                    dgvServerConfig.Columns[nameof(ServerConfig.Id)].Visible = false;

                // no implicit selection after binding
                dgvServerConfig.ClearSelection();
                if (bindingSource != null) bindingSource.Position = -1;
                UpdateActionButtons();
            };

            // 4) Populate filter
            var locations = serverConfigService.GetDistinctLocations(); // sorted by default
            comboBoxFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxFilter.Items.Clear();
            comboBoxFilter.Items.Add("All");
            foreach (var loc in locations) comboBoxFilter.Items.Add(loc);
            comboBoxFilter.SelectedIndex = 0; // "All"

            // 5) Wire events
            comboBoxFilter.SelectedIndexChanged += (s, e) => ApplyFilter();

            // Any selection change or click should refresh status + buttons
            dgvServerConfig.SelectionChanged += (s, e) => { UpdateStatus(); UpdateActionButtons(); };
            dgvServerConfig.CellClick += (s, e) => { UpdateStatus(); UpdateActionButtons(); };
            dgvServerConfig.KeyUp += (s, e) => { UpdateStatus(); UpdateActionButtons(); }; // keyboard navigation

            // 6) Initial state: no selection -> disable buttons, show counts
            dgvServerConfig.ClearSelection();
            if (bindingSource != null) bindingSource.Position = -1;
            UpdateStatus();
            UpdateActionButtons();
        }

        // ----- helpers -----
        private void ApplyFilter()
        {
            var selected = comboBoxFilter.SelectedItem?.ToString() ?? "All";

            IEnumerable<ServerConfig> filtered = selected == "All"
                ? serversAll
                : serversAll.Where(x => string.Equals(x.Location?.Trim() ?? "",
                                                      selected.Trim(),
                                                      StringComparison.OrdinalIgnoreCase));

            serversView.RaiseListChangedEvents = false;
            serversView.Clear();
            foreach (var s in filtered) serversView.Add(s);
            serversView.RaiseListChangedEvents = true;
            serversView.ResetBindings();

            // after filtering, require the user to select a row again
            dgvServerConfig.ClearSelection();
            if (bindingSource != null) bindingSource.Position = -1;

            UpdateStatus();
            UpdateActionButtons();
        }


        private void UpdateStatus()
        {
            var total = serversAll?.Count ?? 0;
            var showing = serversView?.Count ?? 0;
            var filter = comboBoxFilter.SelectedItem?.ToString() ?? "All";

            string selectedDetail = "No row selected";
            if (bindingSource?.Current is ServerConfig c)
            {
                selectedDetail = $"{c.Location} / {c.Instance} ({c.InstanceType}) • User: {c.Username} • TNS: {c.TNS}";
            }

            labelStatus.Text = $"Total: {total} • Showing: {showing} • Filter: {filter} • {selectedDetail}";
        }


        private void Main_Load(object sender, EventArgs e)
        {


            DgvApplyTealStyle.ApplyTealStyle(dgvServerConfig);
            ButtonStyler.ApplyButtonStyle(buttonAdd, styleType: "normal");
            ButtonStyler.ApplyButtonStyle(buttonDelete, styleType: "danger");
            ButtonStyler.ApplyButtonStyle(buttonEdit, styleType: "warning");
        }


        private bool HasSelection()
        {
            // SelectedRows works with multi-select; bindingSource.Current covers keyboard navigation
            return (dgvServerConfig.SelectedRows.Count > 0) || (bindingSource?.Current is ServerConfig);
        }

        private void UpdateActionButtons()
        {
            bool enabled = HasSelection();
            buttonEdit.Enabled = enabled;
            buttonDelete.Enabled = enabled;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (var dlg = new ServerEditorForm())
            {
                dlg.Text = "Add Server";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // Persist via service
                        var id = serverConfigService.Add(dlg.Value);

                        // Update in-memory master
                        serversAll.Add(dlg.Value);

                        // Re-apply current filter and refresh grid
                        ApplyFilter();

                        // Optional: select the newly added row
                        SelectRowById(id);

                        // Optional: status update
                        UpdateStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Add failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void SelectRowById(Guid id)
        {
            foreach (DataGridViewRow row in dgvServerConfig.Rows)
            {
                if (row.DataBoundItem is ServerConfig s && s.Id == id)
                {
                    row.Selected = true;
                    dgvServerConfig.CurrentCell = row.Cells.Cast<DataGridViewCell>().FirstOrDefault();
                    dgvServerConfig.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (bindingSource?.Current is not ServerConfig current)
                return;

            // make a copy so cancel doesn't mutate the bound row
            var draft = new ServerConfig
            {
                Id = current.Id,
                Location = current.Location,
                Instance = current.Instance,
                InstanceType = current.InstanceType,
                Username = current.Username,
                Password = current.Password,
                TNS = current.TNS
            };

            using (var dlg = new ServerEditorForm(draft))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = dlg.Value;
                    try
                    {
                        if (!serverConfigService.Edit(current.Id, updated))
                        {
                            MessageBox.Show(this, "Item not found.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // copy edited values back into the bound instance
                        current.Location = updated.Location;
                        current.Instance = updated.Instance;
                        current.InstanceType = updated.InstanceType;
                        current.Username = updated.Username;
                        current.Password = updated.Password;
                        current.TNS = updated.TNS;

                        bindingSource.ResetCurrentItem(); // refresh grid row
                        UpdateStatus();

                        // If your current filter hides the edited row, re-apply it
                        ApplyFilter();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Edit failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            // Collect selected items (multi-select supported)
            var targets = new List<ServerConfig>();

            if (dgvServerConfig.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvServerConfig.SelectedRows)
                {
                    if (row.DataBoundItem is ServerConfig s) targets.Add(s);
                }
            }
            else if (bindingSource?.Current is ServerConfig current)
            {
                targets.Add(current);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show(this, "Please select a row to delete.", "Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // De-dupe by Id in case of odd selections
            targets = targets.GroupBy(t => t.Id).Select(g => g.First()).ToList();

            // Confirm
            var preview = string.Join(", ", targets.Take(3).Select(t => $"{t.Location}/{t.Instance}"));
            if (targets.Count > 3) preview += $" (+{targets.Count - 3} more)";
            var prompt = targets.Count == 1
                ? $"Delete server \"{preview}\"?"
                : $"Delete {targets.Count} servers? {preview}";

            if (MessageBox.Show(this, prompt, "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            // Delete via service and update in-memory lists
            int ok = 0, fail = 0;
            foreach (var t in targets)
            {
                try
                {
                    if (serverConfigService.Delete(t.Id))
                    {
                        ok++;

                        // Remove from master list
                        serversAll.RemoveAll(x => x.Id == t.Id);

                        // Remove from current view (BindingList)
                        var viewItem = serversView.FirstOrDefault(x => x.Id == t.Id);
                        if (viewItem != null) serversView.Remove(viewItem);
                    }
                    else
                    {
                        fail++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            // Re-apply current filter & refresh UI state
            ApplyFilter();                   // keeps current comboBoxFilter selection
            dgvServerConfig.ClearSelection();
            if (bindingSource != null) bindingSource.Position = -1;

            UpdateStatus();
            UpdateActionButtons();

            if (fail > 0)
            {
                MessageBox.Show(this, $"Deleted {ok} item(s). {fail} failed.", "Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
