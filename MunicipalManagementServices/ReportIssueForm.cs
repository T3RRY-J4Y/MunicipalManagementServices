using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    public partial class ReportIssueForm : Form
    {
        private List<ReportedIssue> _issues = AppData.Issues;
        private int _attachCount = 0;
        private const int MAX_ATTACH = 5;

        public MainMenuForm MainMenu { get; set; }

        private PlaceholderTextBox _txtLocation;
        private ComboBox _cmbCategory;
        private RichTextBox _rtbDescription;
        private ListBox _lstFiles;
        private ProgressBar _progressBar;
        private Label _lblMessage;
        private Label _lblProgress;

        public ReportIssueForm()
        {
            InitializeComponent();
            BuildUI();
            InitCategories();
            UpdateProgress();
        }

        private void BuildUI()
        {
            this.Text = "Report a Municipal Issue";
            this.Size = new Size(820, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.Surface;
            this.MinimumSize = new Size(720, 620);
            this.DoubleBuffered = true;

            // ── BODY (Fill — added FIRST) ────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                BackColor = UITheme.Surface
            };
            this.Controls.Add(body);

            // ── HEADER (Top — added AFTER Fill) ─────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = UITheme.Primary
            };

            var lblH1 = new Label
            {
                Text = "🛠  Report a Municipal Issue",
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

            // Right before Fill inside header
            header.Controls.Add(btnBack);
            header.Controls.Add(lblH1);
            this.Controls.Add(header);

            // ── FORM CARD (plain Panel — no RoundedPanel for docking) ──
            var cardOuter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.Surface,
                Padding = new Padding(0)
            };
            body.Controls.Add(cardOuter);

            // White background card drawn via Paint
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(1, 1, card.Width - 2, card.Height - 2);
                int r = 12;
                using (var path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
                    path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
                    path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                    path.CloseFigure();
                    using (var br = new SolidBrush(Color.White))
                        e.Graphics.FillPath(br, path);
                    using (var pen = new Pen(UITheme.Border, 1))
                        e.Graphics.DrawPath(pen, path);
                }
            };
            cardOuter.Controls.Add(card);

            // ── TABLE LAYOUT inside card ─────────────────────
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(28, 20, 28, 20)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Location
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Category
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Description
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Attach button
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // File list
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // Progress label
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Progress bar
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // Message
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Submit
            card.Controls.Add(table);

            // ── Row 0: Location ──────────────────────────────
            AddFieldLabel(table, "📍 Location", 0);
            _txtLocation = new PlaceholderTextBox
            {
                Placeholder = "e.g. Corner of Main & Oak Street, Benoni",
                Font = UITheme.FontBody,
                Height = 34,
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 8, 0, 0)
            };
            _txtLocation.TextChanged += (s, e) => UpdateProgress();
            table.Controls.Add(_txtLocation, 1, 0);

            // ── Row 1: Category ──────────────────────────────
            AddFieldLabel(table, "🏷 Category", 1);
            _cmbCategory = new ComboBox
            {
                Font = UITheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Top,
                Height = 34,
                Margin = new Padding(0, 8, 0, 0)
            };
            _cmbCategory.SelectedIndexChanged += (s, e) => UpdateProgress();
            table.Controls.Add(_cmbCategory, 1, 1);

            // ── Row 2: Description ───────────────────────────
            AddFieldLabel(table, "📝 Description", 2);
            _rtbDescription = new RichTextBox
            {
                Font = UITheme.FontBody,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Margin = new Padding(0, 8, 0, 0)
            };
            _rtbDescription.TextChanged += (s, e) => UpdateProgress();
            table.Controls.Add(_rtbDescription, 1, 2);

            // ── Row 3: Attach button ──────────────────────────
            AddFieldLabel(table, "📎 Attachments", 3);
            var attachBtn = new StyledButton
            {
                Text = "📂  Attach File",
                BaseColor = UITheme.Info,
                HoverColor = Color.FromArgb(41, 128, 185),
                Width = 140,
                Height = 36,
                Margin = new Padding(0, 8, 0, 0)
            };
            attachBtn.Click += AttachFile;
            table.Controls.Add(attachBtn, 1, 3);

            // ── Row 4: File list ──────────────────────────────
            AddFieldLabel(table, "", 4);
            _lstFiles = new ListBox
            {
                Font = UITheme.FontSmall,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
            table.Controls.Add(_lstFiles, 1, 4);

            // ── Row 5: Progress label ─────────────────────────
            AddFieldLabel(table, "", 5);
            _lblProgress = new Label
            {
                Text = "Form completion: 0%",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Fill
            };
            table.Controls.Add(_lblProgress, 1, 5);

            // ── Row 6: Progress bar ───────────────────────────
            AddFieldLabel(table, "", 6);
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Margin = new Padding(0, 4, 0, 4)
            };
            table.Controls.Add(_progressBar, 1, 6);

            // ── Row 7: Engagement message ─────────────────────
            AddFieldLabel(table, "", 7);
            _lblMessage = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = UITheme.Accent,
                Dock = DockStyle.Fill
            };
            table.Controls.Add(_lblMessage, 1, 7);

            // ── Row 8: Submit button ──────────────────────────
            AddFieldLabel(table, "", 8);
            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            var btnSubmit = new StyledButton
            {
                Text = "✔  Submit Report",
                BaseColor = UITheme.Success,
                HoverColor = Color.FromArgb(34, 153, 84),
                PressColor = Color.FromArgb(25, 120, 65),
                Width = 160,
                Height = 42
            };
            btnSubmit.Click += SubmitIssue;
            btnRow.Controls.Add(btnSubmit);
            table.Controls.Add(btnRow, 1, 8);
        }

        private void AddFieldLabel(TableLayoutPanel t, string text, int row)
        {
            t.Controls.Add(new Label
            {
                Text = text,
                Font = UITheme.FontSubheading,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 18, 8, 0)
            }, 0, row);
        }

        private void InitCategories()
        {
            _cmbCategory.Items.AddRange(new[]
            {
                "Sanitation & Waste Management",
                "Roads & Potholes",
                "Water & Utilities",
                "Public Safety & Security",
                "Electricity & Power Outages",
                "Parks & Recreational Facilities",
                "Street Lighting",
                "Traffic & Signage",
                "Stormwater & Drainage",
                "Graffiti & Vandalism",
                "Public Transportation"
            });
            _cmbCategory.SelectedIndex = 0;
        }

        private void AttachFile(object sender, EventArgs e)
        {
            if (_attachCount >= MAX_ATTACH)
            {
                MessageBox.Show($"Maximum {MAX_ATTACH} attachments allowed.", "Limit Reached",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ofd = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png|Documents|*.pdf;*.docx|All Files|*.*",
                Multiselect = true,
                Title = "Select Attachment"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var f in ofd.FileNames)
                {
                    if (_attachCount >= MAX_ATTACH) break;
                    _lstFiles.Items.Add(Path.GetFileName(f));
                    _attachCount++;
                }
                UpdateProgress();
            }
        }

        private void SubmitIssue(object sender, EventArgs e)
        {
            string loc = _txtLocation.RealText;
            string desc = _rtbDescription.Text.Trim();

            if (string.IsNullOrWhiteSpace(loc))
            {
                MessageBox.Show("Please enter a location.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(desc))
            {
                MessageBox.Show("Please enter a description.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _issues.Add(new ReportedIssue
            {
                Location = loc,
                Category = _cmbCategory.SelectedItem.ToString(),
                Description = desc,
                Attachments = _lstFiles.Items.Cast<string>().ToList(),
                ReportDate = DateTime.Now
            });

            MessageBox.Show(
                "✔  Issue submitted successfully!\n\n" +
                "Thank you for helping keep our community clean and safe.\n" +
                "Your report has been logged and will be reviewed shortly.",
                "Report Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetForm();
        }

        private void GoBack()
        {
            if (MainMenu != null) MainMenu.Show();
            this.Close();
        }

        private void UpdateProgress()
        {
            int p = 0;
            if (!string.IsNullOrWhiteSpace(_txtLocation?.RealText)) p += 25;
            if (!string.IsNullOrWhiteSpace(_rtbDescription?.Text)) p += 25;
            if ((_cmbCategory?.SelectedIndex ?? -1) >= 0) p += 25;
            if (_attachCount > 0) p += 25;

            if (_progressBar != null) _progressBar.Value = p;

            string[] msgs =
            {
                "Start filling in the details below.",
                "Good start, keep going!",
                "You're halfway there!",
                "Almost done, just one more step!",
                "Ready to submit! 🎉"
            };

            int idx = p / 25;
            if (_lblMessage != null) _lblMessage.Text = msgs[Math.Min(idx, msgs.Length - 1)];
            if (_lblProgress != null) _lblProgress.Text = $"Form completion: {p}%";
        }

        private void ResetForm()
        {
            _txtLocation.Text = "";
            _txtLocation.ResetPlaceholder();
            _rtbDescription.Clear();
            _cmbCategory.SelectedIndex = 0;
            _lstFiles.Items.Clear();
            _attachCount = 0;
            UpdateProgress();
        }
    }

    public class ReportedIssue
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public DateTime ReportDate { get; set; }

        // Admin approval workflow: Pending, Approved, Rejected
        public string ApprovalStatus { get; set; } = "Pending";
    }
}