using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    public class LocalEventsForm : Form
    {
        public MainMenuForm MainMenu { get; set; }

        private readonly EventDataStore _store = AppData.Events;

        private PlaceholderTextBox _txtSearch;
        private ComboBox _cmbCategory;
        private DateTimePicker _dtpDate;
        private CheckBox _chkDate;
        private Label _lblResultInfo;

        private TabControl _tabs;
        private ListView _lvAll;
        private ListView _lvAnnouncements;
        private ListView _lvUrgent;
        private ListView _lvRecommended;
        private ListView _lvRecent;

        private Label _lblStatEvents;
        private Label _lblStatSearches;
        private Label _lblStatAnnouncements;

        public LocalEventsForm()
        {
            InitializeComponent();
            BuildUI();
            Refresh_All();
        }

        private void BuildUI()
        {
            this.Text = "Local Events & Announcements";
            this.Size = new Size(1060, 740);
            this.MinimumSize = new Size(920, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.Surface;
            this.DoubleBuffered = true;

            // ── TABS (Fill — must be added to form FIRST) ──────
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                Padding = new Point(10, 5),
                SizeMode = TabSizeMode.FillToRight
            };

            var tabAll = new TabPage("  All Events  ");
            var tabAnn = new TabPage("  Announcements 📢  ");
            var tabUrg = new TabPage("  Urgent 🔴  ");
            var tabRec = new TabPage("  Recommended ⭐  ");
            var tabViewed = new TabPage("  Recently Viewed 🕐  ");

            _lvAll = MakeListView(false); tabAll.Controls.Add(_lvAll);
            _lvAnnouncements = MakeListView(true); tabAnn.Controls.Add(_lvAnnouncements);
            _lvUrgent = MakeListView(false); tabUrg.Controls.Add(_lvUrgent);
            _lvRecommended = MakeListView(false); tabRec.Controls.Add(_lvRecommended);
            _lvRecent = MakeListView(false); tabViewed.Controls.Add(_lvRecent);

            _lvAll.ItemActivate += (s, e) => ShowDetail(_lvAll);
            _lvAnnouncements.ItemActivate += (s, e) => ShowDetail(_lvAnnouncements);
            _lvUrgent.ItemActivate += (s, e) => ShowDetail(_lvUrgent);
            _lvRecommended.ItemActivate += (s, e) => ShowDetail(_lvRecommended);
            _lvRecent.ItemActivate += (s, e) => ShowDetail(_lvRecent);

            _tabs.TabPages.AddRange(new[] { tabAll, tabAnn, tabUrg, tabRec, tabViewed });

            this.Controls.Add(_tabs); 

            // ── RESULT INFO LABEL (Top) ────────────────────────
            _lblResultInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(22, 0, 0, 0),
                BackColor = Color.FromArgb(248, 250, 252),
                Text = ""
            };
            this.Controls.Add(_lblResultInfo);

            // ── SEARCH PANEL (Top) ─────────────────────────────
            var searchCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            var sf = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var searchIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 13),
                Size = new Size(28, 38),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 2, 4, 0)
            };

            _txtSearch = new PlaceholderTextBox
            {
                Placeholder = "Search by keyword…",
                Width = 230,
                Height = 38,
                Font = UITheme.FontBody,
                Margin = new Padding(0, 2, 10, 0)
            };
            _txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) DoSearch(s, e);
            };

            _cmbCategory = new ComboBox
            {
                Width = 185,
                Font = UITheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 10, 0)
            };

            _chkDate = new CheckBox
            {
                Text = "Date:",
                Font = UITheme.FontBody,
                Width = 58,
                Margin = new Padding(0, 10, 4, 0)
            };
            _chkDate.CheckedChanged += (s, e) => _dtpDate.Enabled = _chkDate.Checked;

            _dtpDate = new DateTimePicker
            {
                Width = 130,
                Font = UITheme.FontBody,
                Format = DateTimePickerFormat.Short,
                Margin = new Padding(0, 4, 10, 0),
                Enabled = false
            };

            var btnSearch = new StyledButton
            {
                Text = "Search",
                BaseColor = UITheme.Primary,
                Width = 90,
                Height = 38,
                Margin = new Padding(0, 3, 6, 0)
            };
            btnSearch.Click += DoSearch;

            var btnClear = new StyledButton
            {
                Text = "Clear",
                BaseColor = Color.FromArgb(140, 150, 165),
                Width = 74,
                Height = 38,
                Margin = new Padding(0, 3, 0, 0)
            };
            btnClear.Click += ClearSearch;

            sf.Controls.AddRange(new Control[]
            {
        searchIcon, _txtSearch, _cmbCategory,
        _chkDate, _dtpDate, btnSearch, btnClear
            });
            searchCard.Controls.Add(sf);
            this.Controls.Add(searchCard);

            // ── STAT BAR (Top) ─────────────────────────────────
            var statBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = UITheme.PrimaryDark,
                Padding = new Padding(20, 10, 20, 10)
            };

            var statFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            _lblStatEvents = MakeStat("0");
            _lblStatAnnouncements = MakeStat("0");
            _lblStatSearches = MakeStat("0");

            statFlow.Controls.Add(WrapStat(_lblStatEvents, "Total Events"));
            statFlow.Controls.Add(MakeStatDivider());
            statFlow.Controls.Add(WrapStat(_lblStatAnnouncements, "Announcements"));
            statFlow.Controls.Add(MakeStatDivider());
            statFlow.Controls.Add(WrapStat(_lblStatSearches, "Searches Made"));

            statBar.Controls.Add(statFlow);
            this.Controls.Add(statBar);

            // ── HEADER (Top — added last so it appears at top) ─
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = UITheme.Primary
            };

            var lblH1 = new Label
            {
                Text = "📅  Local Events & Announcements",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };

            var btnBack = new StyledButton
            {
                Text = "◀  Main Menu",
                BaseColor = UITheme.Accent,
                HoverColor = UITheme.AccentDark,
                PressColor = UITheme.AccentDark,
                Width = 130,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 15, 16, 15)
            };
            btnBack.Click += (s, e) => GoBack();

            header.Controls.Add(btnBack);
            header.Controls.Add(lblH1);
            this.Controls.Add(header);
        }

        // ──────────────────────────────────────────────────────
        private ListView MakeListView(bool withTypeCol)
        {
            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                Font = UITheme.FontBody,
                BackColor = Color.White,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            lv.Columns.Add("Title", 340);
            lv.Columns.Add("Category", 140);
            lv.Columns.Add("Date", 105);
            lv.Columns.Add("Location", 210);
            if (withTypeCol) lv.Columns.Add("Type", 110);
            return lv;
        }

        // ──────────────────────────────────────────────────────
        private void Populate(ListView lv, List<LocalEvent> events)
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            bool hasTypeCol = lv.Columns.Count == 5;

            foreach (var ev in events)
            {
                string prefix = ev.IsAnnouncement ? "📢  " : "📅  ";
                var item = new ListViewItem(prefix + ev.Title);
                item.SubItems.Add(ev.Category);
                item.SubItems.Add(ev.EventDate.ToString("dd MMM yyyy"));
                item.SubItems.Add(ev.Location);
                if (hasTypeCol)
                    item.SubItems.Add(ev.IsAnnouncement ? "Announcement" : "Event");
                item.Tag = ev;

                if (ev.IsAnnouncement)
                {
                    item.BackColor = Color.FromArgb(255, 247, 237);
                    item.ForeColor = Color.FromArgb(160, 60, 0);
                }
                else if (ev.EventDate.Date == DateTime.Today)
                {
                    item.BackColor = Color.FromArgb(236, 253, 245);
                    item.ForeColor = Color.FromArgb(6, 120, 70);
                }
                else if (ev.Priority == 1)
                {
                    item.BackColor = Color.FromArgb(255, 242, 242);
                    item.ForeColor = Color.FromArgb(180, 30, 30);
                }
                else
                {
                    item.BackColor = Color.White;
                    item.ForeColor = UITheme.TextPrimary;
                }

                lv.Items.Add(item);
            }

            if (events.Count == 0)
            {
                var ph = new ListViewItem("  No items to display");
                ph.ForeColor = UITheme.TextMuted;
                lv.Items.Add(ph);
            }

            lv.EndUpdate();
        }

        // ──────────────────────────────────────────────────────
        private void Refresh_All()
        {
            _cmbCategory.Items.Clear();
            foreach (var c in _store.GetCategories())
                _cmbCategory.Items.Add(c);
            if (_cmbCategory.Items.Count > 0)
                _cmbCategory.SelectedIndex = 0;

            Populate(_lvAll, _store.GetAllEventsSorted());
            Populate(_lvAnnouncements, _store.GetAnnouncements());
            Populate(_lvUrgent, _store.GetPriorityEvents(30));
            Populate(_lvRecommended, _store.GetRecommendations());
            Populate(_lvRecent, _store.GetRecentlyViewed());

            _lblStatEvents.Text = _store.TotalEvents.ToString();
            _lblStatAnnouncements.Text = _store.GetAnnouncements().Count.ToString();
            _lblStatSearches.Text = _store.TotalSearches.ToString();
        }

        // ──────────────────────────────────────────────────────
        private void DoSearch(object sender, EventArgs e)
        {
            string term = _txtSearch.RealText;
            string cat = _cmbCategory.SelectedItem?.ToString() ?? "All Categories";
            DateTime? date = _chkDate.Checked ? (DateTime?)_dtpDate.Value.Date : null;

            var results = _store.Search(term, cat, date);

            _lblResultInfo.Text = results.Count > 0
                ? "  ✔  " + results.Count + " result(s) found" +
                  (string.IsNullOrWhiteSpace(term) ? "" : " for \"" + term + "\"")
                : "  ✘  No results found";

            Populate(_lvAll, results);
            Populate(_lvRecommended, _store.GetRecommendations());
            _tabs.SelectedIndex = 0;

            _lblStatSearches.Text = _store.TotalSearches.ToString();
        }

        // ──────────────────────────────────────────────────────
        private void ClearSearch(object sender, EventArgs e)
        {
            _txtSearch.Text = "";          // clear text first
            _txtSearch.ResetPlaceholder(); // then restore placeholder

            if (_cmbCategory.Items.Count > 0)
                _cmbCategory.SelectedIndex = 0;

            _chkDate.Checked = false;
            _lblResultInfo.Text = "";

            Populate(_lvAll, _store.GetAllEventsSorted());
        }

        // ──────────────────────────────────────────────────────
        private void ShowDetail(ListView lv)
        {
            if (lv.SelectedItems.Count == 0) return;
            var ev = lv.SelectedItems[0].Tag as LocalEvent;
            if (ev == null) return;

            _store.RecordView(ev);
            Populate(_lvRecent, _store.GetRecentlyViewed());

            var dlg = new Form
            {
                Text = "Event Details",
                Size = new Size(520, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Color stripColor = ev.IsAnnouncement ? UITheme.Warning
                             : ev.Priority == 1 ? UITheme.Danger
                             : UITheme.Accent;

            var topStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 8,
                BackColor = stripColor
            };

            var btnClose = new StyledButton
            {
                Text = "Close",
                BaseColor = UITheme.Primary,
                Dock = DockStyle.Bottom,
                Height = 42
            };
            btnClose.Click += (s, e2) => dlg.Close();

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 16, 28, 16)
            };

            var lblTitle = new Label
            {
                Text = ev.Title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 36,
                AutoEllipsis = true
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 8)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddDetailRow(grid, "Category", ev.Category);
            AddDetailRow(grid, "Date", ev.EventDate.ToString("dddd, dd MMMM yyyy"));
            AddDetailRow(grid, "Location", ev.Location);
            AddDetailRow(grid, "Type", ev.IsAnnouncement ? "📢 Announcement" : "📅 Event");

            var descTitle = new Label
            {
                Text = "Description",
                Font = UITheme.FontSubheading,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 24
            };

            var desc = new RichTextBox
            {
                Text = ev.Description,
                Font = UITheme.FontBody,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 250, 252),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Fill first, then Top items (reverse order of appearance)
            body.Controls.Add(desc);
            body.Controls.Add(descTitle);
            body.Controls.Add(grid);
            body.Controls.Add(lblTitle);

            // Fill first, then Bottom, then Top
            dlg.Controls.Add(body);
            dlg.Controls.Add(btnClose);
            dlg.Controls.Add(topStrip);

            dlg.ShowDialog(this);
        }

        // ──────────────────────────────────────────────────────
        private void AddDetailRow(TableLayoutPanel t, string label, string value)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(new Label
            {
                Text = label + ":",
                Font = UITheme.FontSubheading,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 3, 8, 3)
            }, 0, r);
            t.Controls.Add(new Label
            {
                Text = value,
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 3, 0, 3)
            }, 1, r);
        }

        // ──────────────────────────────────────────────────────
        private Label MakeStat(string val)
        {
            return new Label
            {
                Text = val,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
        }

        private Panel WrapStat(Label valLabel, string caption)
        {
            var p = new Panel
            {
                Width = 140,
                Height = 48,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            var capLbl = new Label
            {
                Text = caption,
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(160, 200, 230),
                Dock = DockStyle.Bottom,
                Height = 18,
                AutoSize = false,
                Width = 140
            };
            valLabel.Dock = DockStyle.Top;
            p.Controls.Add(capLbl);
            p.Controls.Add(valLabel);
            return p;
        }

        private Panel MakeStatDivider()
        {
            return new Panel
            {
                Width = 1,
                Height = 48,
                BackColor = Color.FromArgb(60, 255, 255, 255),
                Margin = new Padding(16, 4, 16, 4)
            };
        }

        // ──────────────────────────────────────────────────────
        private void GoBack()
        {
            if (MainMenu != null) MainMenu.Show();
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}