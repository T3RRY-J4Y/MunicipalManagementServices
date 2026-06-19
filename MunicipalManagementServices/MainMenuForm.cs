using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Municipal Services · Ekurhuleni";
            this.Size = new Size(680, 800);
            this.MinimumSize = new Size(600, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.Surface;
            this.DoubleBuffered = true;

            // ── HERO HEADER ──────────────────────────────────
            var hero = new HeroPanel
            {
                Dock = DockStyle.Top,
                Height = 160
            };

            // ── SUBTITLE STRIP ───────────────────────────────
            var strip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = UITheme.PrimaryDark
            };
            var lblSub = new Label
            {
                Text = "City of Ekurhuleni  ·  Benoni District  ·  Citizen Services Portal",
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(160, 200, 230),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            strip.Controls.Add(lblSub);

            // ── BODY PANEL ───────────────────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 20, 32, 20),
                BackColor = UITheme.Surface
            };

            var lblQuestion = new Label
            {
                Text = "What would you like to do today?",
                Font = UITheme.FontSubheading,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ── CARD STACK ───────────────────────────────────
            var cardStack = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0)
            };

            var card1 = MakeMenuCard(
                "🛠", "Report Issues",
                "Submit a report about potholes, water outages, illegal dumping, and more.",
                UITheme.Primary, true, btnReportIssue_Click);

            var card2 = MakeMenuCard(
                "📅", "Local Events & Announcements",
                "Browse upcoming events, municipal notices, and community announcements.",
                UITheme.Accent, true, btnEvents_Click);

            var card3 = MakeMenuCard(
                "📋", "Service Request Status",
                "Track the progress of your submitted service requests using unique identifiers.",
                UITheme.Info, true, btnServiceRequest_Click);

            var card4 = MakeMenuCard(
                "🔑", "Admin Login",
                "Staff only. Manage requests, events, view analytics and reported issues.",
                UITheme.PrimaryDark, true, btnAdmin_Click);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0)
            };

            card1.Height = card2.Height = card3.Height = card4.Height = 88;
            card1.Margin = card2.Margin = card3.Margin = card4.Margin = new Padding(0, 0, 0, 10);

            flow.Controls.Add(card1);
            flow.Controls.Add(card2);
            flow.Controls.Add(card3);
            flow.Controls.Add(card4);

            flow.Resize += (s, e) =>
            {
                int w = flow.ClientSize.Width - 4;
                card1.Width = card2.Width = card3.Width = card4.Width = w;
            };

            // ── ADMIN CREDENTIALS BANNER (for lecturer / marker) ──
            // A read-only text field showing the demo staff login, as required.
            var credPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(255, 251, 235),
                Padding = new Padding(12, 6, 12, 6)
            };
            var credLabel = new Label
            {
                Text = "Admin login (for marking):",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Left,
                Width = 160,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var txtCreds = new Label
            {
                Text = "Username: admin      Password: admin123",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.PrimaryDark,
                BackColor = Color.FromArgb(255, 251, 235),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            credPanel.Controls.Add(txtCreds);
            credPanel.Controls.Add(credLabel);

            cardStack.Controls.Add(flow);
            // Bottom credentials banner added before the Fill cardStack on the body
            body.Controls.Add(cardStack);
            body.Controls.Add(lblQuestion);
            body.Controls.Add(credPanel);

            this.Controls.Add(body);
            this.Controls.Add(strip);
            this.Controls.Add(hero);
        }

        private Panel MakeMenuCard(string icon, string title, string desc,
                                   Color accent, bool enabled, EventHandler onClick)
        {
            var wrapper = new Panel
            {
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            var card = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Cursor = enabled ? Cursors.Hand : Cursors.Default
            };

            // Accent sidebar
            var sidebar = new Panel
            {
                BackColor = enabled ? accent : UITheme.Border,
                Size = new Size(6, 90),
                Location = new Point(0, 0)
            };
            card.Controls.Add(sidebar);

            // Icon
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 20),
                Size = new Size(62, 90),
                Location = new Point(6, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = enabled ? accent : UITheme.TextMuted,
                BackColor = Color.White
            };
            card.Controls.Add(lblIcon);

            // Arrow
            var arrow = new Label
            {
                Text = enabled ? "›" : "–",
                Font = new Font("Segoe UI", 20),
                Size = new Size(40, 90),
                Location = new Point(0, 0),   // will be set by layout
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = enabled ? accent : UITheme.TextMuted,
                BackColor = Color.White
            };
            card.Controls.Add(arrow);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = UITheme.FontHeading,
                ForeColor = enabled ? UITheme.TextPrimary : UITheme.TextMuted,
                Size = new Size(400, 26),
                Location = new Point(80, 18),
                BackColor = Color.White
            };
            card.Controls.Add(lblTitle);

            // Description
            var lblDesc = new Label
            {
                Text = desc,
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                Size = new Size(400, 36),
                Location = new Point(80, 46),
                AutoEllipsis = true,
                BackColor = Color.White
            };
            card.Controls.Add(lblDesc);

            // ── Define layout function BEFORE it is used in any event ──
            Action<Panel> layoutChildren = (c) =>
            {
                int textWidth = c.Width - 80 - 44;
                if (textWidth < 10) textWidth = 10;
                lblTitle.Size = new Size(textWidth, 26);
                lblDesc.Size = new Size(textWidth, 36);
                arrow.Location = new Point(c.Width - 40, 0);
                sidebar.Size = new Size(6, c.Height);
                lblIcon.Size = new Size(62, c.Height);
            };

            // Paint rounded border and sidebar
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                int r = 10;
                using (var path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
                    path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
                    path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                    path.CloseFigure();
                    using (var br = new SolidBrush(card.BackColor))
                        e.Graphics.FillPath(br, path);
                    using (var pen = new Pen(enabled ? UITheme.Border : Color.FromArgb(225, 225, 225), 1))
                        e.Graphics.DrawPath(pen, path);
                }
                // Sidebar drawn on top of border
                using (var br = new SolidBrush(enabled ? accent : UITheme.Border))
                    e.Graphics.FillRectangle(br, 0, 0, 6, card.Height);
            };

            // Wire resize events — layout function is defined above so no compiler error
            wrapper.Resize += (s, e) =>
            {
                card.Size = new Size(wrapper.Width, wrapper.Height);
                card.Location = new Point(0, 0);
                layoutChildren(card);
            };

            card.Resize += (s, e) => layoutChildren(card);

            wrapper.Controls.Add(card);

            // Hover and click
            if (enabled && onClick != null)
            {
                Color hoverColor = Color.FromArgb(245, 250, 255);

                EventHandler enter = (s, e) =>
                {
                    card.BackColor = hoverColor;
                    lblIcon.BackColor = hoverColor;
                    lblTitle.BackColor = hoverColor;
                    lblDesc.BackColor = hoverColor;
                    arrow.BackColor = hoverColor;
                    card.Invalidate();
                };
                EventHandler leave = (s, e) =>
                {
                    card.BackColor = Color.White;
                    lblIcon.BackColor = Color.White;
                    lblTitle.BackColor = Color.White;
                    lblDesc.BackColor = Color.White;
                    arrow.BackColor = Color.White;
                    card.Invalidate();
                };

                foreach (Control c in new Control[]
                    { card, sidebar, lblIcon, arrow, lblTitle, lblDesc })
                {
                    c.MouseEnter += enter;
                    c.MouseLeave += leave;
                    c.Click += onClick;
                    c.Cursor = Cursors.Hand;
                }
            }

            return wrapper;
        }

        private void btnReportIssue_Click(object sender, EventArgs e)
        {
            var f = new ReportIssueForm { MainMenu = this };
            f.Show();
            this.Hide();
        }

        private void btnEvents_Click(object sender, EventArgs e)
        {
            var f = new LocalEventsForm { MainMenu = this };
            f.Show();
            this.Hide();
        }

        private void btnServiceRequest_Click(object sender, EventArgs e)
        {
            var f = new ServiceRequestForm { MainMenu = this };
            f.Show();
            this.Hide();
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            // Require login each time the admin area is opened
            using (var login = new AdminLoginForm())
            {
                if (login.ShowDialog(this) == DialogResult.OK)
                {
                    var f = new AdminForm { MainMenu = this };
                    f.Show();
                    this.Hide();
                }
            }
        }
    }

    // ── Custom hero panel ─────────────────────────────────────────────────────
    internal class HeroPanel : Panel
    {
        public HeroPanel() { DoubleBuffered = true; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;

            using (var br = new LinearGradientBrush(
                rect, UITheme.PrimaryDark, UITheme.PrimaryLight, 135f))
                e.Graphics.FillRectangle(br, rect);

            DrawCircle(e.Graphics, Width - 60, -40, 130, 18);
            DrawCircle(e.Graphics, Width - 180, 50, 80, 12);
            DrawCircle(e.Graphics, 30, Height + 10, 100, 10);

            using (var f = new Font("Segoe UI", 24, FontStyle.Bold))
                e.Graphics.DrawString("Municipal Services", f,
                    Brushes.White, new PointF(36, 30));

            using (var f = new Font("Segoe UI", 10))
                e.Graphics.DrawString("Empowering citizens  ·  Improving communities", f,
                    new SolidBrush(Color.FromArgb(180, 220, 255)), new PointF(38, 78));

            using (var f = new Font("Segoe UI Emoji", 28))
                e.Graphics.DrawString("🏛", f, Brushes.White, new PointF(36, 100));
        }

        private void DrawCircle(Graphics g, int cx, int cy, int r, int alpha)
        {
            using (var br = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                g.FillEllipse(br, cx - r, cy - r, r * 2, r * 2);
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); Invalidate(); }
    }
}