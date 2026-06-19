using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    /// <summary>
    /// Admin dashboard. Accessible only after a successful login.
    /// Provides: dashboard analytics, service-request management (status + ward),
    /// event/announcement management (add/edit/delete), and a view of all
    /// citizen-reported issues from Part 1.
    /// </summary>
    public class AdminForm : Form
    {
        public MainMenuForm MainMenu { get; set; }

        private readonly EventDataStore      _events   = AppData.Events;
        private readonly ServiceRequestStore _requests = AppData.ServiceRequests;
        private readonly List<ReportedIssue> _issues   = AppData.Issues;

        private TabControl _tabs;

        // Dashboard labels
        private Label _dSrTotal, _dSrPending, _dSrProgress, _dSrResolved, _dEvents, _dAnnounce, _dIssues;
        private Panel _chartPanel;

        // Requests management
        private ListView _lvReq;
        private ComboBox _cmbReqStatus, _cmbReqWard;

        // Events management
        private ListView _lvEvt;

        // Issues view
        private ListView _lvIssues;

        public AdminForm()
        {
            InitializeComponent();
            BuildUI();
            RefreshAll();
        }

        // ──────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text           = "Admin Dashboard";
            this.Size           = new Size(1120, 780);
            this.MinimumSize     = new Size(980, 660);
            this.StartPosition  = FormStartPosition.CenterScreen;
            this.BackColor      = UITheme.Surface;
            this.DoubleBuffered = true;

            // ── TABS (Fill, added first) ───────────────────────
            _tabs = new TabControl
            {
                Dock     = DockStyle.Fill,
                Font     = new Font("Segoe UI", 9f),
                Padding  = new Point(12, 5),
                SizeMode = TabSizeMode.FillToRight
            };

            var tabDash   = new TabPage("Dashboard");
            var tabReq    = new TabPage("Manage Requests");
            var tabEvt    = new TabPage("Manage Events");
            var tabIssues = new TabPage("Reported Issues");

            BuildDashboardTab(tabDash);
            BuildRequestsTab(tabReq);
            BuildEventsTab(tabEvt);
            BuildIssuesTab(tabIssues);

            _tabs.TabPages.AddRange(new[] { tabDash, tabReq, tabEvt, tabIssues });
            this.Controls.Add(_tabs);

            // ── HEADER (Top, added last) ───────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = UITheme.PrimaryDark };
            var lblH1  = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(24, 0, 0, 0)
            };
            var btnLogout = new StyledButton
            {
                Text = "Log Out", BaseColor = UITheme.Danger,
                HoverColor = Color.FromArgb(190, 40, 40), PressColor = Color.FromArgb(160, 30, 30),
                Width = 110, Dock = DockStyle.Right, Margin = new Padding(0, 15, 12, 15)
            };
            btnLogout.Click += (s, e) => { AdminAuth.LogOut(); GoBack(); };

            var btnBack = new StyledButton
            {
                Text = "Main Menu", BaseColor = UITheme.Accent,
                HoverColor = UITheme.AccentDark, PressColor = UITheme.AccentDark,
                Width = 130, Dock = DockStyle.Right, Margin = new Padding(0, 15, 4, 15)
            };
            btnBack.Click += (s, e) => GoBack();

            header.Controls.Add(btnLogout);
            header.Controls.Add(btnBack);
            header.Controls.Add(lblH1);
            this.Controls.Add(header);
        }

        // ══════════════════════════════════════════════════════
        // DASHBOARD TAB
        // ══════════════════════════════════════════════════════
        private void BuildDashboardTab(TabPage tab)
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20), BackColor = UITheme.Surface };

            var lblTitle = new Label { Text = "Overview", Font = UITheme.FontHeading, ForeColor = UITheme.TextPrimary, Dock = DockStyle.Top, Height = 34 };

            // Stat cards row
            var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 110, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Padding = new Padding(0, 6, 0, 6) };

            _dSrTotal    = AddDashCard(cards, "Total Requests",   UITheme.Primary);
            _dSrPending  = AddDashCard(cards, "Pending",          UITheme.Warning);
            _dSrProgress = AddDashCard(cards, "In Progress",      UITheme.Info);
            _dSrResolved = AddDashCard(cards, "Resolved / Closed",UITheme.Success);
            _dEvents     = AddDashCard(cards, "Total Events",     UITheme.Accent);
            _dAnnounce   = AddDashCard(cards, "Announcements",    Color.FromArgb(142, 68, 173));
            _dIssues     = AddDashCard(cards, "Reported Issues",  Color.FromArgb(192, 57, 43));

            // Chart (requests by status, drawn manually)
            var chartTitle = new Label { Text = "Service Requests by Status", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Top, Height = 28, Padding = new Padding(0, 10, 0, 0) };

            _chartPanel = new Panel { Dock = DockStyle.Top, Height = 220, BackColor = Color.White };
            _chartPanel.Paint += DrawStatusChart;

            scroll.Controls.Add(_chartPanel);
            scroll.Controls.Add(chartTitle);
            scroll.Controls.Add(cards);
            scroll.Controls.Add(lblTitle);
            tab.Controls.Add(scroll);
        }

        private Label AddDashCard(FlowLayoutPanel parent, string caption, Color accent)
        {
            var card = new Panel { Width = 150, Height = 90, BackColor = Color.White, Margin = new Padding(0, 0, 12, 0) };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(UITheme.Border, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var br = new SolidBrush(accent))
                    e.Graphics.FillRectangle(br, 0, 0, 4, card.Height);
            };
            var val = new Label { Text = "0", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = UITheme.TextPrimary, Location = new Point(14, 10), AutoSize = true, BackColor = Color.Transparent };
            var cap = new Label { Text = caption, Font = UITheme.FontSmall, ForeColor = UITheme.TextSecondary, Location = new Point(14, 56), Size = new Size(130, 28), BackColor = Color.Transparent };
            card.Controls.Add(val);
            card.Controls.Add(cap);
            parent.Controls.Add(card);
            return val;
        }

        private void DrawStatusChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            var data = new (string label, int value, Color colour)[]
            {
                ("Pending",     _requests.PendingCount,    UITheme.Warning),
                ("In Progress", _requests.InProgressCount, UITheme.Info),
                ("Resolved",    _requests.ResolvedCount,   UITheme.Success),
            };

            int max = Math.Max(1, data.Max(d => d.value));
            int chartH = _chartPanel.Height - 60;
            int baseY  = chartH + 20;
            int barW   = 90;
            int gap    = 60;
            int x      = 50;

            using (var axis = new Pen(UITheme.Border, 1))
                g.DrawLine(axis, 30, baseY, _chartPanel.Width - 20, baseY);

            foreach (var d in data)
            {
                int h = (int)((double)d.value / max * chartH);
                var rect = new Rectangle(x, baseY - h, barW, h);
                using (var br = new SolidBrush(d.colour))
                    g.FillRectangle(br, rect);
                using (var f = new Font("Segoe UI", 12, FontStyle.Bold))
                using (var tb = new SolidBrush(UITheme.TextPrimary))
                {
                    var vs = d.value.ToString();
                    var sz = g.MeasureString(vs, f);
                    g.DrawString(vs, f, tb, x + (barW - sz.Width) / 2, baseY - h - 22);
                }
                using (var f = new Font("Segoe UI", 9))
                using (var tb = new SolidBrush(UITheme.TextSecondary))
                {
                    var sz = g.MeasureString(d.label, f);
                    g.DrawString(d.label, f, tb, x + (barW - sz.Width) / 2, baseY + 6);
                }
                x += barW + gap;
            }
        }

        // ══════════════════════════════════════════════════════
        // MANAGE REQUESTS TAB
        // ══════════════════════════════════════════════════════
        private void BuildRequestsTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Surface };

            _lvReq = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = false, BorderStyle = BorderStyle.None, Font = UITheme.FontBody,
                BackColor = Color.White, HeaderStyle = ColumnHeaderStyle.Nonclickable, MultiSelect = false
            };
            _lvReq.Columns.Add("ID",        80);
            _lvReq.Columns.Add("Title",    220);
            _lvReq.Columns.Add("Category", 100);
            _lvReq.Columns.Add("Ward",     160);
            _lvReq.Columns.Add("Priority",  64);
            _lvReq.Columns.Add("Status",   100);
            _lvReq.Columns.Add("Approval",  90);
            _lvReq.SelectedIndexChanged += (s, e) => SyncReqSelection();

            // Action bar
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 104, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };

            var lblStatus = new Label { Text = "Set status:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Width = 80, Margin = new Padding(0, 10, 4, 0) };
            _cmbReqStatus = new ComboBox { Width = 130, Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 6, 8, 0) };
            _cmbReqStatus.Items.AddRange(new[] { "Pending", "In Progress", "Resolved", "Closed" });
            _cmbReqStatus.SelectedIndex = 0;
            var btnStatus = new StyledButton { Text = "Update Status", BaseColor = UITheme.Primary, Width = 130, Height = 34, Margin = new Padding(0, 4, 18, 4) };
            btnStatus.Click += (s, e) => UpdateSelectedStatus();

            var lblWard = new Label { Text = "Reassign ward:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Width = 100, Margin = new Padding(0, 10, 4, 0) };
            _cmbReqWard = new ComboBox { Width = 180, Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 6, 8, 4) };
            foreach (var w in _requests.Graph.GetAllWards()) _cmbReqWard.Items.Add(w);
            if (_cmbReqWard.Items.Count > 0) _cmbReqWard.SelectedIndex = 0;
            var btnWard = new StyledButton { Text = "Reassign", BaseColor = UITheme.Accent, Width = 110, Height = 34, Margin = new Padding(0, 4, 18, 4) };
            btnWard.Click += (s, e) => ReassignSelectedWard();

            var lblApprove = new Label { Text = "Decision:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Width = 70, Margin = new Padding(0, 10, 4, 0) };
            var btnApprove = new StyledButton { Text = "Approve", BaseColor = UITheme.Success, HoverColor = Color.FromArgb(34, 153, 84), Width = 110, Height = 34, Margin = new Padding(0, 4, 8, 4) };
            var btnReject  = new StyledButton { Text = "Reject", BaseColor = UITheme.Danger, HoverColor = Color.FromArgb(190, 40, 40), Width = 100, Height = 34, Margin = new Padding(0, 4, 0, 4) };
            btnApprove.Click += (s, e) => SetSelectedApproval("Approved");
            btnReject.Click  += (s, e) => SetSelectedApproval("Rejected");

            flow.Controls.AddRange(new Control[] { lblStatus, _cmbReqStatus, btnStatus, lblWard, _cmbReqWard, btnWard, lblApprove, btnApprove, btnReject });
            bar.Controls.Add(flow);

            var hint = new Label
            {
                Text = "Select a request, then update its status, reassign its ward, or approve/reject it.",
                Dock = DockStyle.Top, Height = 30, Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary, BackColor = UITheme.Surface,
                Padding = new Padding(16, 8, 0, 0)
            };

            panel.Controls.Add(_lvReq);
            panel.Controls.Add(bar);
            panel.Controls.Add(hint);
            tab.Controls.Add(panel);
        }

        private void SetSelectedApproval(string decision)
        {
            if (_lvReq.SelectedItems.Count == 0) { MessageBox.Show("Please select a request first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var req = _lvReq.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;
            _requests.SetApproval(req.RequestId, decision);
            RefreshAll();
        }

        private void SyncReqSelection()
        {
            if (_lvReq.SelectedItems.Count == 0) return;
            var req = _lvReq.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;
            _cmbReqStatus.SelectedItem = req.Status;
            if (_cmbReqWard.Items.Contains(req.Ward)) _cmbReqWard.SelectedItem = req.Ward;
        }

        private void UpdateSelectedStatus()
        {
            if (_lvReq.SelectedItems.Count == 0) { MessageBox.Show("Please select a request first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var req = _lvReq.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;
            _requests.UpdateStatus(req.RequestId, _cmbReqStatus.SelectedItem.ToString());
            RefreshAll();
        }

        private void ReassignSelectedWard()
        {
            if (_lvReq.SelectedItems.Count == 0) { MessageBox.Show("Please select a request first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var req = _lvReq.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;
            _requests.ReassignWard(req.RequestId, _cmbReqWard.SelectedItem.ToString());
            RefreshAll();
        }

        // ══════════════════════════════════════════════════════
        // MANAGE EVENTS TAB
        // ══════════════════════════════════════════════════════
        private void BuildEventsTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Surface };

            _lvEvt = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = false, BorderStyle = BorderStyle.None, Font = UITheme.FontBody,
                BackColor = Color.White, HeaderStyle = ColumnHeaderStyle.Nonclickable, MultiSelect = false
            };
            _lvEvt.Columns.Add("ID",        50);
            _lvEvt.Columns.Add("Title",    260);
            _lvEvt.Columns.Add("Category", 120);
            _lvEvt.Columns.Add("Date",     100);
            _lvEvt.Columns.Add("Location", 180);
            _lvEvt.Columns.Add("Type",     110);

            var bar = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            var btnAdd  = new StyledButton { Text = "Add New", BaseColor = UITheme.Success, Width = 110, Height = 34, Margin = new Padding(0, 0, 8, 0) };
            var btnEdit = new StyledButton { Text = "Edit Selected", BaseColor = UITheme.Primary, Width = 130, Height = 34, Margin = new Padding(0, 0, 8, 0) };
            var btnDel  = new StyledButton { Text = "Delete Selected", BaseColor = UITheme.Danger, Width = 140, Height = 34, Margin = new Padding(0, 0, 0, 0) };

            btnAdd.Click  += (s, e) => OpenEventEditor(null);
            btnEdit.Click += (s, e) =>
            {
                if (_lvEvt.SelectedItems.Count == 0) { MessageBox.Show("Please select an event to edit.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                OpenEventEditor(_lvEvt.SelectedItems[0].Tag as LocalEvent);
            };
            btnDel.Click += (s, e) =>
            {
                if (_lvEvt.SelectedItems.Count == 0) { MessageBox.Show("Please select an event to delete.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                var ev = _lvEvt.SelectedItems[0].Tag as LocalEvent;
                if (ev == null) return;
                if (MessageBox.Show($"Delete event \"{ev.Title}\"?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _events.DeleteEvent(ev.Id);
                    RefreshAll();
                }
            };

            flow.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });
            bar.Controls.Add(flow);

            panel.Controls.Add(_lvEvt);
            panel.Controls.Add(bar);
            tab.Controls.Add(panel);
        }

        private void OpenEventEditor(LocalEvent existing)
        {
            bool isNew = existing == null;
            var dlg = new Form
            {
                Text = isNew ? "Add Event" : "Edit Event",
                Size = new Size(480, 460),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false, MinimizeBox = false, BackColor = Color.White
            };

            var strip = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = UITheme.Accent };
            var body  = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));   // description
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // buttons

            Func<string, int, Control, Control> row = (label, r, ctrl) =>
            {
                tbl.Controls.Add(new Label { Text = label, Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Fill, Padding = new Padding(0, 10, 6, 0) }, 0, r);
                ctrl.Dock = DockStyle.Fill;
                tbl.Controls.Add(ctrl, 1, r);
                return ctrl;
            };

            var txtTitle = new TextBox { Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            var cmbCat   = new ComboBox { Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCat.Items.AddRange(new[] { "Utilities", "Roads", "Sanitation", "Community", "Education", "Health", "Governance", "Social Services" });
            var dtp      = new DateTimePicker { Font = UITheme.FontBody, Format = DateTimePickerFormat.Short };
            var txtLoc   = new TextBox { Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            var cmbType  = new ComboBox { Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "Event", "Announcement" });
            var cmbPrio  = new ComboBox { Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPrio.Items.AddRange(new[] { "1 - Critical", "2 - High", "3 - Medium", "4 - Low" });
            var rtbDesc  = new RichTextBox { Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle };

            row("Title", 0, txtTitle);
            row("Category", 1, cmbCat);
            row("Date", 2, dtp);
            row("Location", 3, txtLoc);
            row("Type", 4, cmbType);
            row("Priority", 5, cmbPrio);
            tbl.Controls.Add(new Label { Text = "Description", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Fill, Padding = new Padding(0, 8, 6, 0) }, 0, 6);
            rtbDesc.Dock = DockStyle.Fill;
            tbl.Controls.Add(rtbDesc, 1, 6);

            // Prefill
            if (isNew)
            {
                cmbCat.SelectedIndex = 0; cmbType.SelectedIndex = 0; cmbPrio.SelectedIndex = 2;
                dtp.Value = DateTime.Today.AddDays(1);
            }
            else
            {
                txtTitle.Text = existing.Title;
                if (!cmbCat.Items.Contains(existing.Category)) cmbCat.Items.Add(existing.Category);
                cmbCat.SelectedItem = existing.Category;
                dtp.Value = existing.EventDate;
                txtLoc.Text = existing.Location;
                cmbType.SelectedIndex = existing.IsAnnouncement ? 1 : 0;
                cmbPrio.SelectedIndex = Math.Min(3, Math.Max(0, existing.Priority - 1));
                rtbDesc.Text = existing.Description;
            }

            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
            var btnSave   = new StyledButton { Text = isNew ? "Add" : "Save", BaseColor = UITheme.Success, Width = 100, Height = 36, Margin = new Padding(6, 0, 0, 0) };
            var btnCancel = new StyledButton { Text = "Cancel", BaseColor = Color.FromArgb(140, 150, 165), Width = 90, Height = 36 };
            btnCancel.Click += (s, e) => dlg.Close();
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtLoc.Text))
                {
                    MessageBox.Show("Please enter a title and location.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int priority = cmbPrio.SelectedIndex + 1;
                bool isAnn = cmbType.SelectedIndex == 1;

                if (isNew)
                {
                    _events.AddEvent(new LocalEvent
                    {
                        Title = txtTitle.Text.Trim(),
                        Category = cmbCat.SelectedItem.ToString(),
                        EventDate = dtp.Value.Date,
                        Location = txtLoc.Text.Trim(),
                        Description = rtbDesc.Text.Trim(),
                        IsAnnouncement = isAnn,
                        Priority = priority
                    });
                }
                else
                {
                    _events.UpdateEvent(existing.Id, txtTitle.Text.Trim(), cmbCat.SelectedItem.ToString(),
                        dtp.Value.Date, txtLoc.Text.Trim(), rtbDesc.Text.Trim(), isAnn, priority);
                }
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };
            btnRow.Controls.Add(btnSave);
            btnRow.Controls.Add(btnCancel);
            tbl.Controls.Add(new Label(), 0, 7);
            tbl.Controls.Add(btnRow, 1, 7);

            body.Controls.Add(tbl);
            dlg.Controls.Add(body);
            dlg.Controls.Add(strip);

            if (dlg.ShowDialog(this) == DialogResult.OK)
                RefreshAll();
        }

        // ══════════════════════════════════════════════════════
        // REPORTED ISSUES TAB
        // ══════════════════════════════════════════════════════
        private void BuildIssuesTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Surface };

            _lvIssues = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                GridLines = false, BorderStyle = BorderStyle.None, Font = UITheme.FontBody,
                BackColor = Color.White, HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _lvIssues.Columns.Add("Reported",    130);
            _lvIssues.Columns.Add("Category",    180);
            _lvIssues.Columns.Add("Location",    190);
            _lvIssues.Columns.Add("Files",        50);
            _lvIssues.Columns.Add("Status",       90);
            _lvIssues.Columns.Add("Description",  260);
            _lvIssues.ItemActivate += (s, e) =>
            {
                if (_lvIssues.SelectedItems.Count == 0) return;
                var iss = _lvIssues.SelectedItems[0].Tag as ReportedIssue;
                if (iss == null) return;
                MessageBox.Show(
                    $"Reported: {iss.ReportDate:dd MMM yyyy HH:mm}\nCategory: {iss.Category}\nLocation: {iss.Location}\nStatus: {iss.ApprovalStatus}\n\n{iss.Description}\n\nAttachments: {(iss.Attachments.Count == 0 ? "none" : string.Join(", ", iss.Attachments))}",
                    "Issue Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Approve / reject action bar
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            var lbl = new Label { Text = "Decision:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Width = 70, Margin = new Padding(0, 10, 4, 0) };
            var btnApprove = new StyledButton { Text = "Approve", BaseColor = UITheme.Success, HoverColor = Color.FromArgb(34, 153, 84), Width = 110, Height = 34, Margin = new Padding(0, 4, 8, 0) };
            var btnReject  = new StyledButton { Text = "Reject", BaseColor = UITheme.Danger, HoverColor = Color.FromArgb(190, 40, 40), Width = 100, Height = 34, Margin = new Padding(0, 4, 0, 0) };
            btnApprove.Click += (s, e) => SetSelectedIssueApproval("Approved");
            btnReject.Click  += (s, e) => SetSelectedIssueApproval("Rejected");
            flow.Controls.AddRange(new Control[] { lbl, btnApprove, btnReject });
            bar.Controls.Add(flow);

            var hint = new Label
            {
                Text = "All issues reported by citizens. Select one and approve or reject it, or double-click for full details.",
                Dock = DockStyle.Top, Height = 30, Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary, BackColor = UITheme.Surface, Padding = new Padding(16, 8, 0, 0)
            };

            panel.Controls.Add(_lvIssues);
            panel.Controls.Add(bar);
            panel.Controls.Add(hint);
            tab.Controls.Add(panel);
        }

        private void SetSelectedIssueApproval(string decision)
        {
            if (_lvIssues.SelectedItems.Count == 0) { MessageBox.Show("Please select an issue first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var iss = _lvIssues.SelectedItems[0].Tag as ReportedIssue;
            if (iss == null) return;
            iss.ApprovalStatus = decision;
            RefreshAll();
        }

        // ══════════════════════════════════════════════════════
        // REFRESH
        // ══════════════════════════════════════════════════════
        private void RefreshAll()
        {
            // Dashboard
            _dSrTotal.Text    = _requests.TotalCount.ToString();
            _dSrPending.Text  = _requests.PendingCount.ToString();
            _dSrProgress.Text = _requests.InProgressCount.ToString();
            _dSrResolved.Text = _requests.ResolvedCount.ToString();
            _dEvents.Text     = _events.TotalEvents.ToString();
            _dAnnounce.Text   = _events.GetAnnouncements().Count.ToString();
            _dIssues.Text     = _issues.Count.ToString();
            _chartPanel?.Invalidate();

            // Requests
            _lvReq.BeginUpdate();
            _lvReq.Items.Clear();
            foreach (var r in _requests.GetAllSortedById())
            {
                var it = new ListViewItem(r.RequestId);
                it.SubItems.Add(r.Title);
                it.SubItems.Add(r.Category);
                it.SubItems.Add(r.Ward);
                it.SubItems.Add(PriorityLabel(r.Priority));
                it.SubItems.Add(r.Status);
                it.SubItems.Add(r.Approval);
                it.Tag = r;

                // Colour-code the row by approval decision
                switch (r.Approval)
                {
                    case "Approved":
                        it.BackColor = Color.FromArgb(236, 253, 245);
                        it.ForeColor = Color.FromArgb(6, 120, 70);
                        break;
                    case "Rejected":
                        it.BackColor = Color.FromArgb(255, 242, 242);
                        it.ForeColor = Color.FromArgb(180, 30, 30);
                        break;
                    default: // Pending
                        it.BackColor = Color.FromArgb(255, 247, 237);
                        it.ForeColor = Color.FromArgb(160, 80, 0);
                        break;
                }

                _lvReq.Items.Add(it);
            }
            _lvReq.EndUpdate();

            // Events
            _lvEvt.BeginUpdate();
            _lvEvt.Items.Clear();
            foreach (var ev in _events.GetAllEventsSorted())
            {
                var it = new ListViewItem(ev.Id.ToString());
                it.SubItems.Add(ev.Title);
                it.SubItems.Add(ev.Category);
                it.SubItems.Add(ev.EventDate.ToString("dd MMM yyyy"));
                it.SubItems.Add(ev.Location);
                it.SubItems.Add(ev.IsAnnouncement ? "Announcement" : "Event");
                it.Tag = ev;
                _lvEvt.Items.Add(it);
            }
            _lvEvt.EndUpdate();

            // Issues
            _lvIssues.BeginUpdate();
            _lvIssues.Items.Clear();
            foreach (var iss in _issues)
            {
                var it = new ListViewItem(iss.ReportDate.ToString("dd MMM yyyy HH:mm"));
                it.SubItems.Add(iss.Category);
                it.SubItems.Add(iss.Location);
                it.SubItems.Add(iss.Attachments.Count.ToString());
                it.SubItems.Add(iss.ApprovalStatus);
                it.SubItems.Add(iss.Description);
                it.Tag = iss;

                switch (iss.ApprovalStatus)
                {
                    case "Approved":
                        it.BackColor = Color.FromArgb(236, 253, 245);
                        it.ForeColor = Color.FromArgb(6, 120, 70);
                        break;
                    case "Rejected":
                        it.BackColor = Color.FromArgb(255, 242, 242);
                        it.ForeColor = Color.FromArgb(180, 30, 30);
                        break;
                    default:
                        it.BackColor = Color.FromArgb(255, 247, 237);
                        it.ForeColor = Color.FromArgb(160, 80, 0);
                        break;
                }

                _lvIssues.Items.Add(it);
            }
            if (_issues.Count == 0)
            {
                var ph = new ListViewItem("No issues reported yet");
                ph.ForeColor = UITheme.TextMuted;
                _lvIssues.Items.Add(ph);
            }
            _lvIssues.EndUpdate();
        }

        private string PriorityLabel(int p)
        {
            switch (p) { case 1: return "Critical"; case 2: return "High"; case 3: return "Medium"; default: return "Low"; }
        }

        private void GoBack() { if (MainMenu != null) MainMenu.Show(); this.Close(); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}
