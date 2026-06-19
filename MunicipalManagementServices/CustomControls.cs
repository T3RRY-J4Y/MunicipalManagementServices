using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    // ── Rounded Panel ─────────────────────────────────────────────────────────
    public class RoundedPanel : Panel
    {
        public int   CornerRadius { get; set; } = 10;
        public Color BorderColor  { get; set; } = UITheme.Border;
        public int   BorderWidth  { get; set; } = 1;
        public bool  ShowShadow   { get; set; } = true;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(2, 2, Width - 4, Height - 4);

            if (ShowShadow)
            {
                // soft shadow
                for (int i = 3; i >= 1; i--)
                {
                    var shadow = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);
                    using (var gp = RoundedRect(shadow, CornerRadius))
                    using (var br = new SolidBrush(Color.FromArgb(15 * (4 - i), 0, 0, 0)))
                        e.Graphics.FillPath(br, gp);
                }
            }

            using (var path = RoundedRect(rect, CornerRadius))
            {
                using (var br = new SolidBrush(BackColor))
                    e.Graphics.FillPath(br, path);
                if (BorderWidth > 0)
                    using (var pen = new Pen(BorderColor, BorderWidth))
                        e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); Invalidate(); }
    }

    // ── Beautiful Button ──────────────────────────────────────────────────────
    public class StyledButton : Button
    {
        private bool _hovered;
        private bool _pressed;

        public Color BaseColor   { get; set; } = UITheme.Primary;
        public Color HoverColor  { get; set; } = UITheme.PrimaryLight;
        public Color PressColor  { get; set; } = UITheme.PrimaryDark;
        public int   CornerRadius { get; set; } = 8;
        public bool  Outlined    { get; set; } = false;

        public StyledButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = Color.White;
            Font = UITheme.FontSubheading;
            Cursor = Cursors.Hand;
            Height = 42;
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = Enabled
                ? (_pressed ? PressColor : (_hovered ? HoverColor : BaseColor))
                : Color.FromArgb(200, 200, 200);

            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            using (var path = RoundedRect(rect, CornerRadius))
            {
                if (Outlined)
                {
                    e.Graphics.FillPath(new SolidBrush(Color.White), path);
                    e.Graphics.DrawPath(new Pen(bg, 2), path);
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        e.Graphics.DrawString(Text, Font, new SolidBrush(bg), ClientRectangle, sf);
                }
                else
                {
                    // gradient fill
                    using (var br = new LinearGradientBrush(rect,
                        ControlPaint.Light(bg, 0.05f), ControlPaint.Dark(bg, 0.05f),
                        LinearGradientMode.Vertical))
                        e.Graphics.FillPath(br, path);

                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        e.Graphics.DrawString(Text, Font, Brushes.White, ClientRectangle, sf);
                }
            }
        }

        private GraphicsPath RoundedRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ── Stat Card ─────────────────────────────────────────────────────────────
    public class StatCard : Panel
    {
        private string _value;
        private string _label;
        private Color  _accent;

        public StatCard(string value, string label, Color accent)
        {
            _value = value; _label = label; _accent = accent;
            Size = new Size(150, 80);
            BackColor = Color.White;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            // left accent bar
            e.Graphics.FillRectangle(new SolidBrush(_accent), 0, 0, 4, Height);

            // value
            using (var f = new Font("Segoe UI", 22, FontStyle.Bold))
                e.Graphics.DrawString(_value, f, new SolidBrush(UITheme.TextPrimary), new PointF(16, 10));

            // label
            using (var f = new Font("Segoe UI", 8))
                e.Graphics.DrawString(_label, f, new SolidBrush(UITheme.TextSecondary), new PointF(16, 50));
        }
    }

    // ── Badge Label ───────────────────────────────────────────────────────────
    public class BadgeLabel : Label
    {
        public Color BadgeColor { get; set; } = UITheme.Accent;

        public BadgeLabel()
        {
            Font = UITheme.FontBadge;
            ForeColor = Color.White;
            AutoSize = false;
            Height = 22;
            TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Pill(rect))
            {
                e.Graphics.FillPath(new SolidBrush(BadgeColor), path);
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(Text, Font, Brushes.White, rect, sf);
            }
        }

        private GraphicsPath Pill(Rectangle r)
        {
            int rad = r.Height / 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ── Placeholder TextBox ───────────────────────────────────────────────────
    public class PlaceholderTextBox : TextBox
    {
        private string _placeholder;
        private bool _showingPlaceholder;

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; ResetPlaceholder(); }
        }

        public string RealText => _showingPlaceholder ? "" : Text;

        public PlaceholderTextBox()
        {
            GotFocus += (s, e) => ClearPlaceholder();
            LostFocus += (s, e) => ResetPlaceholder();
        }

        private void ClearPlaceholder()
        {
            if (_showingPlaceholder)
            {
                Text = "";
                ForeColor = UITheme.TextPrimary;
                _showingPlaceholder = false;
            }
        }

        public void ResetPlaceholder()
        {
            if (string.IsNullOrEmpty(Text) || Text == _placeholder)
            {
                Text = _placeholder;
                ForeColor = UITheme.TextMuted;
                _showingPlaceholder = true;
            }
        }
    }
}
