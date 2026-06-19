using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    public class ServiceRequestForm : Form
    {
        public MainMenuForm MainMenu { get; set; }

        private readonly ServiceRequestStore _store = AppData.ServiceRequests;

        // Controls
        private TabControl _tabs;
        private ListView   _lvAll;
        private ListView   _lvPriority;
        private ListView   _lvGraph;
        private RichTextBox _rtbTraversal;

        private Label _lblStatTotal;
        private Label _lblStatPending;
        private Label _lblStatProgress;
        private Label _lblStatResolved;

        private ComboBox _cmbStatusFilter;
        private ComboBox _cmbCategoryFilter;
        private PlaceholderTextBox _txtSearchId;
        private Label _lblSearchResult;

        public ServiceRequestForm()
        {
            InitializeComponent();
            BuildUI();
            RefreshAll();
        }

        // ──────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text           = "Service Request Status";
            this.Size           = new Size(1100, 760);
            this.MinimumSize    = new Size(960, 640);
            this.StartPosition  = FormStartPosition.CenterScreen;
            this.BackColor      = UITheme.Surface;
            this.DoubleBuffered = true;

            // ── TABS (Fill — added FIRST) ──────────────────────
            _tabs = new TabControl
            {
                Dock     = DockStyle.Fill,
                Font     = new Font("Segoe UI", 9f),
                Padding  = new Point(10, 5),
                SizeMode = TabSizeMode.FillToRight
            };

            var tabAll      = new TabPage("All Requests");
            var tabPriority = new TabPage("By Priority");
            var tabTrees    = new TabPage("Tree Explorer");
            var tabWard     = new TabPage("By Ward / Graph");
            var tabSearch   = new TabPage("Search by ID");
            var tabSubmit   = new TabPage("Submit New");

            _lvAll      = MakeListView(true);  tabAll.Controls.Add(_lvAll);
            _lvPriority = MakeListView(false); tabPriority.Controls.Add(_lvPriority);

            // Tree Explorer tab — Basic Tree, Binary Tree, BST, AVL, Red-Black, Heap
            BuildTreeExplorerTab(tabTrees);

            // Graph tab
            BuildGraphTab(tabWard);
            BuildSearchTab(tabSearch);
            BuildSubmitTab(tabSubmit);

            _lvAll.ItemActivate      += (s, e) => ShowDetail(_lvAll);
            _lvPriority.ItemActivate += (s, e) => ShowDetail(_lvPriority);

            _tabs.TabPages.AddRange(new[] { tabAll, tabPriority, tabTrees, tabWard, tabSearch, tabSubmit });
            this.Controls.Add(_tabs);

            // ── FILTER BAR (Top) ───────────────────────────────
            var filterBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.White,
                Padding   = new Padding(16, 8, 16, 8)
            };

            var ff = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };

            var lblFilter = new Label { Text = "Filter:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Width = 46, Margin = new Padding(0, 8, 6, 0) };

            _cmbStatusFilter = new ComboBox
            {
                Width = 140, Font = UITheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 10, 0)
            };
            _cmbStatusFilter.Items.AddRange(new[] { "All Statuses", "Pending", "In Progress", "Resolved", "Closed" });
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilter();

            _cmbCategoryFilter = new ComboBox
            {
                Width = 180, Font = UITheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 10, 0)
            };
            _cmbCategoryFilter.Items.Add("All Categories");
            foreach (var cat in new[] { "Roads", "Water", "Electricity", "Sanitation", "Parks", "Stormwater", "Public Safety" })
                _cmbCategoryFilter.Items.Add(cat);
            _cmbCategoryFilter.SelectedIndex = 0;
            _cmbCategoryFilter.SelectedIndexChanged += (s, e) => ApplyFilter();

            ff.Controls.AddRange(new Control[] { lblFilter, _cmbStatusFilter, _cmbCategoryFilter });
            filterBar.Controls.Add(ff);
            this.Controls.Add(filterBar);

            // ── STAT BAR (Top) ─────────────────────────────────
            var statBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = UITheme.PrimaryDark,
                Padding   = new Padding(20, 10, 20, 10)
            };
            var statFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            _lblStatTotal    = MakeStat("0");
            _lblStatPending  = MakeStat("0");
            _lblStatProgress = MakeStat("0");
            _lblStatResolved = MakeStat("0");

            statFlow.Controls.Add(WrapStat(_lblStatTotal,    "Total"));
            statFlow.Controls.Add(MakeStatDivider());
            statFlow.Controls.Add(WrapStat(_lblStatPending,  "Pending"));
            statFlow.Controls.Add(MakeStatDivider());
            statFlow.Controls.Add(WrapStat(_lblStatProgress, "In Progress"));
            statFlow.Controls.Add(MakeStatDivider());
            statFlow.Controls.Add(WrapStat(_lblStatResolved, "Resolved / Closed"));

            statBar.Controls.Add(statFlow);
            this.Controls.Add(statBar);

            // ── HEADER (Top — last so it renders at top) ───────
            var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = UITheme.Primary };
            var lblH1  = new Label
            {
                Text = "Service Request Status",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };
            var btnBack = new StyledButton
            {
                Text = "Main Menu", BaseColor = UITheme.Accent,
                HoverColor = UITheme.AccentDark, PressColor = UITheme.AccentDark,
                Width = 130, Dock = DockStyle.Right,
                Margin = new Padding(0, 15, 16, 15)
            };
            btnBack.Click += (s, e) => GoBack();
            header.Controls.Add(btnBack);
            header.Controls.Add(lblH1);
            this.Controls.Add(header);
        }

        // ──────────────────────────────────────────────────────
        // TREE EXPLORER TAB — visualises all the tree structures
        // ──────────────────────────────────────────────────────
        private void BuildTreeExplorerTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = UITheme.Surface };

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 48,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
                Padding = new Padding(0, 4, 0, 4)
            };

            var output = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                Font = new Font("Consolas", 9.5f),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(180, 220, 255),
                BorderStyle = BorderStyle.None
            };

            // Helper to write a heading + body lines to the console-style box
            Action<string, IEnumerable<string>> write = (heading, lines) =>
            {
                output.Clear();
                output.SelectionColor = Color.FromArgb(0, 230, 180);
                output.AppendText(heading + "\r\n");
                output.SelectionColor = Color.FromArgb(120, 140, 170);
                output.AppendText(new string('─', 60) + "\r\n");
                output.SelectionColor = Color.FromArgb(180, 220, 255);
                foreach (var l in lines) output.AppendText(l + "\r\n");
            };

            var btnBasic = new StyledButton { Text = "Basic Tree", BaseColor = UITheme.Primary, Width = 120, Height = 34, Margin = new Padding(0, 0, 6, 4) };
            btnBasic.Click += (s, e) =>
            {
                var lines = new List<string>(_store.CategoryTree.Print());
                lines.Add("");
                lines.Add("Purpose: models the municipal department hierarchy.");
                lines.Add("Each request category maps to a parent department, so the");
                lines.Add("app can route a new request to the right division.");
                lines.Add("");
                lines.Add("Example lookup: category 'Water' belongs to department");
                lines.Add("  --> " + (_store.CategoryTree.FindDepartment("Water") ?? "unknown"));
                write("BASIC TREE  (generic N-ary tree)", lines);
            };

            var btnBinary = new StyledButton { Text = "Binary Tree", BaseColor = UITheme.Info, Width = 120, Height = 34, Margin = new Padding(0, 0, 6, 4) };
            btnBinary.Click += (s, e) =>
            {
                var lines = new List<string>(_store.PriorityTree.Print());
                lines.Add("");
                lines.Add("Purpose: a fixed-shape decision tree (NOT a search tree).");
                lines.Add("It classifies a request's priority number into a bucket.");
                lines.Add("");
                lines.Add("Sample classifications:");
                foreach (var req in _store.GetByPriority().Take(5))
                    lines.Add($"  {req.RequestId}  (P{req.Priority})  -->  {_store.PriorityTree.Classify(req.Priority)}");
                write("BINARY TREE  (fixed-shape, non-search)", lines);
            };

            var btnBST = new StyledButton { Text = "BST In-Order", BaseColor = UITheme.Accent, Width = 130, Height = 34, Margin = new Padding(0, 0, 6, 4) };
            btnBST.Click += (s, e) =>
            {
                var lines = _store.GetAllSortedById()
                    .Select(r => $"  {r.RequestId}   {r.Title}").ToList();
                lines.Insert(0, "In-order traversal returns requests sorted by ID:");
                lines.Insert(1, "");
                write("BINARY SEARCH TREE  (BST)", lines);
            };

            var btnAVL = new StyledButton { Text = "AVL (balanced)", BaseColor = Color.FromArgb(142, 68, 173), Width = 140, Height = 34, Margin = new Padding(0, 0, 6, 4) };
            btnAVL.Click += (s, e) =>
            {
                var lines = new List<string>();
                lines.Add("The AVL tree self-balances on every insert, guaranteeing");
                lines.Add("O(log n) search even in the worst case. The 'Search by ID'");
                lines.Add("tab uses this tree for fast, reliable lookups.");
                lines.Add("");
                lines.Add("All requests (AVL in-order, also sorted by ID):");
                lines.Add("");
                lines.AddRange(_store.GetByPriority()
                    .OrderBy(r => r.RequestId)
                    .Select(r => $"  {r.RequestId}   {r.Title}"));
                write("AVL TREE  (self-balancing BST)", lines);
            };

            var btnRB = new StyledButton { Text = "Red-Black", BaseColor = Color.FromArgb(192, 57, 43), Width = 120, Height = 34, Margin = new Padding(0, 0, 6, 4) };
            btnRB.Click += (s, e) =>
            {
                var lines = new List<string>();
                lines.Add("The Red-Black tree holds the same data with a different");
                lines.Add("balancing scheme (colour rules + rotations). It guarantees");
                lines.Add("the longest path is at most twice the shortest path.");
                lines.Add("");
                lines.Add("All requests (Red-Black in-order, sorted by ID):");
                lines.Add("");
                lines.AddRange(_store.GetAllSortedByIdRedBlack()
                    .Select(r => $"  {r.RequestId}   {r.Title}"));
                write("RED-BLACK TREE  (balanced BST)", lines);
            };

            var btnHeap = new StyledButton { Text = "Min-Heap", BaseColor = UITheme.Warning, Width = 110, Height = 34, Margin = new Padding(0, 0, 0, 4) };
            btnHeap.Click += (s, e) =>
            {
                var lines = new List<string>();
                lines.Add("The min-heap always keeps the most urgent request (lowest");
                lines.Add("priority number) at the root for O(1) peek / O(log n) extract.");
                lines.Add("");
                lines.Add("Requests in heap priority order:");
                lines.Add("");
                lines.AddRange(_store.GetByPriority()
                    .Select(r => $"  P{r.Priority}  {r.RequestId}   {r.Title}"));
                write("MIN-HEAP  (priority queue)", lines);
            };

            btnRow.Controls.AddRange(new Control[] { btnBasic, btnBinary, btnBST, btnAVL, btnRB, btnHeap });

            panel.Controls.Add(output);
            panel.Controls.Add(btnRow);
            tab.Controls.Add(panel);

            // Show the Basic Tree by default
            tab.Enter += (s, e) => { if (output.TextLength == 0) btnBasic.PerformClick(); };
        }

        // ──────────────────────────────────────────────────────
        private void BuildGraphTab(TabPage tab)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300,
                Panel1MinSize = 200,
                Panel2MinSize = 150
            };

            // Top: ward graph info
            _lvGraph = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details,
                FullRowSelect = true, GridLines = false,
                BorderStyle = BorderStyle.None, Font = UITheme.FontBody,
                BackColor = Color.White, HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _lvGraph.Columns.Add("Ward",          240);
            _lvGraph.Columns.Add("Requests",       90);
            _lvGraph.Columns.Add("Connected To",  400);
            split.Panel1.Controls.Add(_lvGraph);

            // Bottom: traversal output
            var btmPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 44,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 4, 0, 4)
            };

            var btnBFS = new StyledButton { Text = "BFS Traversal", BaseColor = UITheme.Primary, Width = 130, Height = 34, Margin = new Padding(0, 0, 8, 0) };
            var btnDFS = new StyledButton { Text = "DFS Traversal", BaseColor = UITheme.Info,    Width = 130, Height = 34, Margin = new Padding(0, 0, 8, 0) };
            var btnMST = new StyledButton { Text = "Minimum Spanning Tree", BaseColor = UITheme.Accent, Width = 190, Height = 34, Margin = new Padding(0, 0, 0, 0) };

            btnBFS.Click += (s, e) => RunBFS();
            btnDFS.Click += (s, e) => RunDFS();
            btnMST.Click += (s, e) => RunMST();

            btnRow.Controls.AddRange(new Control[] { btnBFS, btnDFS, btnMST });

            _rtbTraversal = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                Font = new Font("Courier New", 9),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(180, 220, 255),
                BorderStyle = BorderStyle.None
            };

            btmPanel.Controls.Add(_rtbTraversal);
            btmPanel.Controls.Add(btnRow);
            split.Panel2.Controls.Add(btmPanel);

            tab.Controls.Add(split);
        }

        private void BuildSearchTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 24, 30, 24), BackColor = UITheme.Surface };

            var lblTitle = new Label { Text = "Search Service Request by ID", Font = UITheme.FontHeading, ForeColor = UITheme.TextPrimary, Dock = DockStyle.Top, Height = 32 };

            var searchRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 6, 0, 0) };

            _txtSearchId = new PlaceholderTextBox
            {
                Placeholder = "Enter Request ID (e.g. SR-00001)",
                Width = 280, Height = 36, Font = UITheme.FontBody,
                Margin = new Padding(0, 0, 10, 0)
            };

            var btnFind = new StyledButton { Text = "Find", BaseColor = UITheme.Primary, Width = 90, Height = 36 };
            btnFind.Click += DoSearch;
            _txtSearchId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoSearch(s, e); };

            searchRow.Controls.AddRange(new Control[] { _txtSearchId, btnFind });

            _lblSearchResult = new Label
            {
                Dock = DockStyle.Top, Height = 24,
                Font = UITheme.FontSmall, ForeColor = UITheme.TextSecondary,
                Padding = new Padding(2, 4, 0, 0), Text = ""
            };

            // Detail display
            var detailCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                Padding = new Padding(24, 18, 24, 18)
            };
            detailCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(UITheme.Border, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, detailCard.Width - 1, detailCard.Height - 1);
            };

            var _searchDetailTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2,
                Padding = new Padding(0), AutoScroll = true
            };
            _searchDetailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            _searchDetailTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _searchDetailTable.Tag = "search_detail";
            detailCard.Controls.Add(_searchDetailTable);

            panel.Controls.Add(detailCard);
            panel.Controls.Add(_lblSearchResult);
            panel.Controls.Add(searchRow);
            panel.Controls.Add(lblTitle);
            tab.Controls.Add(panel);
        }

        private void BuildSubmitTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 20, 30, 20), BackColor = UITheme.Surface, AutoScroll = true };

            var card = new Panel { Dock = DockStyle.Top, Height = 620, BackColor = Color.White, Padding = new Padding(24, 18, 24, 18) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(UITheme.Border, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, Padding = new Padding(0) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var txtTitle    = MakeField(tbl, "Title",       0);
            var cmbCat      = MakeCombo(tbl, "Category",    1, new[] { "Roads", "Water", "Electricity", "Sanitation", "Parks", "Stormwater", "Public Safety" });
            var txtLocation = MakeField(tbl, "Location",    2);
            var cmbWard     = MakeCombo(tbl, "Ward",        3, new[] { "Ward 1 - Benoni CBD", "Ward 2 - Rynfield", "Ward 3 - Northmead", "Ward 4 - Lakefield", "Ward 5 - Farrarmere", "Ward 6 - Birchleigh", "Ward 7 - Benoni North" });

            AddLabel(tbl, "Description", 4);
            var rtbDesc = new RichTextBox { Dock = DockStyle.Fill, Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 8, 0, 0) };
            tbl.Controls.Add(rtbDesc, 1, 4);

            var cmbPriority = MakeCombo(tbl, "Priority", 5, new[] { "1 - Critical", "2 - High", "3 - Medium", "4 - Low" });

            // Submit button
            tbl.Controls.Add(new Label(), 0, 6);
            var btnSubmit = new StyledButton { Text = "Submit Request", BaseColor = UITheme.Success, HoverColor = Color.FromArgb(34, 153, 84), PressColor = Color.FromArgb(25, 120, 65), Width = 170, Height = 42, Margin = new Padding(0, 8, 0, 0) };
            btnSubmit.Click += (s, e) =>
            {
                string title = txtTitle.RealText;
                string loc   = txtLocation.RealText;
                string desc  = rtbDesc.Text.Trim();
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(loc) || string.IsNullOrWhiteSpace(desc))
                {
                    MessageBox.Show("Please fill in Title, Location, and Description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int priority = cmbPriority.SelectedIndex + 1;
                var req = new ServiceRequest
                {
                    Title       = title,
                    Category    = cmbCat.SelectedItem?.ToString() ?? "Roads",
                    Location    = loc,
                    Description = desc,
                    Ward        = cmbWard.SelectedItem?.ToString() ?? "Ward 1 - Benoni CBD",
                    Priority    = priority,
                    SubmittedAt = DateTime.Now,
                    Status      = "Pending"
                };
                _store.AddRequest(req);
                RefreshAll();

                MessageBox.Show(
                    $"Request submitted successfully!\nYour Request ID is: {req.RequestId}\n\nUse the Search tab to track your request status.",
                    "Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtTitle.Text = ""; txtTitle.ResetPlaceholder();
                txtLocation.Text = ""; txtLocation.ResetPlaceholder();
                rtbDesc.Clear();
                cmbCat.SelectedIndex = 0;
                cmbWard.SelectedIndex = 0;
                cmbPriority.SelectedIndex = 2;
            };
            tbl.Controls.Add(btnSubmit, 1, 6);

            card.Controls.Add(tbl);
            panel.Controls.Add(card);

            var lblH = new Label { Text = "Submit a New Service Request", Font = UITheme.FontHeading, ForeColor = UITheme.TextPrimary, Dock = DockStyle.Top, Height = 34 };
            panel.Controls.Add(lblH);
            tab.Controls.Add(panel);
        }

        private PlaceholderTextBox MakeField(TableLayoutPanel t, string label, int row)
        {
            AddLabel(t, label, row);
            var txt = new PlaceholderTextBox
            {
                Placeholder = $"Enter {label.ToLower()}...",
                Font = UITheme.FontBody, Height = 34,
                Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 8, 0, 0)
            };
            t.Controls.Add(txt, 1, row);
            return txt;
        }

        private ComboBox MakeCombo(TableLayoutPanel t, string label, int row, string[] items)
        {
            AddLabel(t, label, row);
            var cmb = new ComboBox { Font = UITheme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Margin = new Padding(0, 8, 0, 0) };
            cmb.Items.AddRange(items);
            cmb.SelectedIndex = 0;
            t.Controls.Add(cmb, 1, row);
            return cmb;
        }

        private void AddLabel(TableLayoutPanel t, string text, int row)
        {
            t.Controls.Add(new Label { Text = text, Font = UITheme.FontSubheading, ForeColor = UITheme.TextPrimary, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Padding = new Padding(0, 18, 8, 0) }, 0, row);
        }

        // ──────────────────────────────────────────────────────
        private ListView MakeListView(bool withStatus)
        {
            var lv = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details,
                FullRowSelect = true, GridLines = false,
                BorderStyle = BorderStyle.None, Font = UITheme.FontBody,
                BackColor = Color.White, HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            lv.Columns.Add("Request ID",  90);
            lv.Columns.Add("Title",      260);
            lv.Columns.Add("Category",   120);
            lv.Columns.Add("Location",   180);
            lv.Columns.Add("Priority",    72);
            if (withStatus) lv.Columns.Add("Status", 100);
            lv.Columns.Add("Submitted",  105);
            return lv;
        }

        private void PopulateListView(ListView lv, List<ServiceRequest> requests, bool withStatus)
        {
            lv.BeginUpdate();
            lv.Items.Clear();

            foreach (var req in requests)
            {
                var item = new ListViewItem(req.RequestId);
                item.SubItems.Add(req.Title);
                item.SubItems.Add(req.Category);
                item.SubItems.Add(req.Location);
                item.SubItems.Add(PriorityLabel(req.Priority));
                if (withStatus) item.SubItems.Add(req.Status);
                item.SubItems.Add(req.SubmittedAt.ToString("dd MMM yyyy"));
                item.Tag = req;

                // Colour by status
                switch (req.Status)
                {
                    case "Pending":
                        item.BackColor = Color.FromArgb(255, 247, 237);
                        item.ForeColor = Color.FromArgb(160, 80, 0);
                        break;
                    case "In Progress":
                        item.BackColor = Color.FromArgb(239, 246, 255);
                        item.ForeColor = Color.FromArgb(29, 78, 150);
                        break;
                    case "Resolved":
                        item.BackColor = Color.FromArgb(236, 253, 245);
                        item.ForeColor = Color.FromArgb(6, 120, 70);
                        break;
                    case "Closed":
                        item.BackColor = Color.FromArgb(248, 250, 252);
                        item.ForeColor = UITheme.TextMuted;
                        break;
                    default:
                        item.BackColor = Color.White;
                        item.ForeColor = UITheme.TextPrimary;
                        break;
                }

                lv.Items.Add(item);
            }

            if (requests.Count == 0)
            {
                var ph = new ListViewItem("  No requests to display");
                ph.ForeColor = UITheme.TextMuted;
                lv.Items.Add(ph);
            }

            lv.EndUpdate();
        }

        private string PriorityLabel(int p)
        {
            switch (p)
            {
                case 1: return "Critical";
                case 2: return "High";
                case 3: return "Medium";
                default: return "Low";
            }
        }

        // ──────────────────────────────────────────────────────
        private void RefreshAll()
        {
            // All Requests tab — sorted by ID via BST in-order
            PopulateListView(_lvAll, _store.GetAllSortedById(), true);

            // Priority tab — via Min-Heap
            PopulateListView(_lvPriority, _store.GetByPriority(), true);

            // Graph tab
            RefreshGraphTab();

            // Stats
            _lblStatTotal.Text    = _store.TotalCount.ToString();
            _lblStatPending.Text  = _store.PendingCount.ToString();
            _lblStatProgress.Text = _store.InProgressCount.ToString();
            _lblStatResolved.Text = _store.ResolvedCount.ToString();
        }

        private void RefreshGraphTab()
        {
            _lvGraph.Items.Clear();
            foreach (var ward in _store.Graph.GetAllWards())
            {
                int count = _store.Graph.GetRequestCountForWard(ward);
                var neighbours = _store.Graph.GetNeighbours(ward)
                    .Select(n => $"{n.neighbour} ({n.weight}km)")
                    .ToList();
                var item = new ListViewItem(ward);
                item.SubItems.Add(count.ToString());
                item.SubItems.Add(string.Join(", ", neighbours));
                item.BackColor = count > 2 ? Color.FromArgb(255, 242, 242) : Color.White;
                _lvGraph.Items.Add(item);
            }
        }

        private void ApplyFilter()
        {
            string status   = _cmbStatusFilter.SelectedItem?.ToString()   ?? "All Statuses";
            string category = _cmbCategoryFilter.SelectedItem?.ToString() ?? "All Categories";

            var all = _store.GetAllSortedById();

            if (status != "All Statuses")
                all = all.Where(r => r.Status == status).ToList();
            if (category != "All Categories")
                all = all.Where(r => r.Category == category).ToList();

            PopulateListView(_lvAll, all, true);
        }

        // ──────────────────────────────────────────────────────
        private void RunBFS()
        {
            string start = _store.Graph.GetAllWards().FirstOrDefault() ?? "";
            var order = _store.Graph.BFS(start);
            _rtbTraversal.Clear();
            _rtbTraversal.AppendText("BFS TRAVERSAL (Breadth-First Search)\r\n");
            _rtbTraversal.AppendText("Starting from: " + start + "\r\n");
            _rtbTraversal.AppendText(new string('─', 50) + "\r\n");
            for (int i = 0; i < order.Count; i++)
            {
                int reqs = _store.Graph.GetRequestCountForWard(order[i]);
                _rtbTraversal.AppendText($"  Step {i + 1}: {order[i]}  [{reqs} request(s)]\r\n");
            }
            _rtbTraversal.AppendText("\r\nBFS visits all neighbours at current depth before going deeper.");
        }

        private void RunDFS()
        {
            string start = _store.Graph.GetAllWards().FirstOrDefault() ?? "";
            var order = _store.Graph.DFS(start);
            _rtbTraversal.Clear();
            _rtbTraversal.AppendText("DFS TRAVERSAL (Depth-First Search)\r\n");
            _rtbTraversal.AppendText("Starting from: " + start + "\r\n");
            _rtbTraversal.AppendText(new string('─', 50) + "\r\n");
            for (int i = 0; i < order.Count; i++)
            {
                int reqs = _store.Graph.GetRequestCountForWard(order[i]);
                _rtbTraversal.AppendText($"  Step {i + 1}: {order[i]}  [{reqs} request(s)]\r\n");
            }
            _rtbTraversal.AppendText("\r\nDFS explores as far as possible along each branch before backtracking.");
        }

        private void RunMST()
        {
            var mst = _store.Graph.PrimMST();
            _rtbTraversal.Clear();
            _rtbTraversal.AppendText("MINIMUM SPANNING TREE (Prim's Algorithm)\r\n");
            _rtbTraversal.AppendText("Finds the lowest-cost route connecting all wards.\r\n");
            _rtbTraversal.AppendText(new string('─', 50) + "\r\n");
            int total = 0;
            foreach (var edge in mst)
            {
                _rtbTraversal.AppendText($"  {edge.From}  -->  {edge.To}  ({edge.Weight} km)\r\n");
                total += edge.Weight;
            }
            _rtbTraversal.AppendText(new string('─', 50) + "\r\n");
            _rtbTraversal.AppendText($"  Total MST distance: {total} km\r\n");
            _rtbTraversal.AppendText("\r\nThe MST ensures all wards are connected with minimum total road distance,\r\n");
            _rtbTraversal.AppendText("useful for planning efficient municipal service routes.");
        }

        // ──────────────────────────────────────────────────────
        private void DoSearch(object sender, EventArgs e)
        {
            string id = _txtSearchId.RealText.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(id)) return;

            // Use AVL tree for O(log n) guaranteed lookup
            var req = _store.FindById(id);
            var tbl = FindSearchDetailTable();
            if (tbl == null) return;

            tbl.Controls.Clear();
            tbl.RowStyles.Clear();
            tbl.RowCount = 0;

            if (req == null)
            {
                _lblSearchResult.Text = "  No request found with ID: " + id;
                _lblSearchResult.ForeColor = UITheme.Danger;
                return;
            }

            _lblSearchResult.Text = "  Request found (AVL tree lookup — O(log n))";
            _lblSearchResult.ForeColor = UITheme.Success;

            AddDetailRow(tbl, "Request ID",   req.RequestId);
            AddDetailRow(tbl, "Title",        req.Title);
            AddDetailRow(tbl, "Category",     req.Category);
            AddDetailRow(tbl, "Location",     req.Location);
            AddDetailRow(tbl, "Ward",         req.Ward);
            AddDetailRow(tbl, "Priority",     PriorityLabel(req.Priority));
            AddDetailRow(tbl, "Status",       req.Status);
            AddDetailRow(tbl, "Submitted",    req.SubmittedAt.ToString("dd MMMM yyyy HH:mm"));
            AddDetailRow(tbl, "Resolved",     req.ResolvedAt.HasValue ? req.ResolvedAt.Value.ToString("dd MMMM yyyy HH:mm") : "—");
            AddDetailRow(tbl, "Description",  req.Description);

            // Status update buttons
            tbl.RowCount++;
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            tbl.Controls.Add(new Label { Text = "Update Status:", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Fill, Padding = new Padding(0, 14, 8, 0) }, 0, tbl.RowCount - 1);

            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 8, 0, 0) };
            foreach (var status in new[] { "Pending", "In Progress", "Resolved", "Closed" })
            {
                var s = status;
                var btn = new StyledButton
                {
                    Text = s, Width = 110, Height = 32,
                    BaseColor = s == req.Status ? UITheme.Primary : Color.FromArgb(140, 150, 165),
                    Margin = new Padding(0, 0, 6, 0)
                };
                btn.Click += (sv, ev) =>
                {
                    _store.UpdateStatus(req.RequestId, s);
                    RefreshAll();
                    DoSearch(sender, e); // refresh detail view
                };
                btnRow.Controls.Add(btn);
            }
            tbl.Controls.Add(btnRow, 1, tbl.RowCount - 1);
        }

        private TableLayoutPanel FindSearchDetailTable()
        {
            foreach (TabPage tp in _tabs.TabPages)
                foreach (Control c in tp.Controls)
                    if (FindTableWithTag(c, "search_detail") is TableLayoutPanel t)
                        return t;
            return null;
        }

        private Control FindTableWithTag(Control c, string tag)
        {
            if (c.Tag?.ToString() == tag) return c;
            foreach (Control child in c.Controls)
            {
                var result = FindTableWithTag(child, tag);
                if (result != null) return result;
            }
            return null;
        }

        private void AddDetailRow(TableLayoutPanel t, string label, string value)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(new Label { Text = label + ":", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Fill, Padding = new Padding(0, 4, 8, 4) }, 0, r);
            t.Controls.Add(new Label { Text = value, Font = UITheme.FontBody, ForeColor = UITheme.TextPrimary, Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 4) }, 1, r);
        }

        private void ShowDetail(ListView lv)
        {
            if (lv.SelectedItems.Count == 0) return;
            var req = lv.SelectedItems[0].Tag as ServiceRequest;
            if (req == null) return;

            // Navigate to search tab and pre-fill
            _txtSearchId.Text = req.RequestId;
            _txtSearchId.ForeColor = UITheme.TextPrimary;
            _tabs.SelectedIndex = 3;
            DoSearch(this, EventArgs.Empty);
        }

        // ── Stat helpers ──────────────────────────────────────
        private Label MakeStat(string val) => new Label { Text = val, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true };

        private Panel WrapStat(Label valLabel, string caption)
        {
            var p = new Panel { Width = 150, Height = 48, Margin = new Padding(0), Padding = new Padding(0) };
            var cap = new Label { Text = caption, Font = UITheme.FontSmall, ForeColor = Color.FromArgb(160, 200, 230), Dock = DockStyle.Bottom, Height = 18, AutoSize = false };
            valLabel.Dock = DockStyle.Top;
            p.Controls.Add(cap);
            p.Controls.Add(valLabel);
            return p;
        }

        private Panel MakeStatDivider() => new Panel { Width = 1, Height = 48, BackColor = Color.FromArgb(60, 255, 255, 255), Margin = new Padding(16, 4, 16, 4) };

        private void GoBack() { if (MainMenu != null) MainMenu.Show(); this.Close(); }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}
