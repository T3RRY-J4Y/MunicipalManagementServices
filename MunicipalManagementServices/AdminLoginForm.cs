using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalManagementServices
{
    /// <summary>
    /// Modal username + password login for the Admin area.
    /// Returns DialogResult.OK only when credentials validate.
    /// </summary>
    public class AdminLoginForm : Form
    {
        private TextBox _txtUser;
        private TextBox _txtPass;
        private Label   _lblError;

        public AdminLoginForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Admin Login";
            this.Size = new Size(440, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Top colour strip
            var strip = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = UITheme.Primary };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 20, 30, 20) };

            // Build bottom-up so Top-docked items stack in the right visual order.
            // (Add the item that should appear LOWEST first.)

            var hint = new Label
            {
                Text = "Demo credentials ---> username: admin    password: admin123",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = UITheme.TextMuted,
                Dock = DockStyle.Top,
                Height = 24,
                Padding = new Padding(0, 8, 0, 0)
            };

            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0) };
            var btnLogin = new StyledButton { Text = "Sign In", BaseColor = UITheme.Primary, Width = 110, Height = 38, Margin = new Padding(6, 0, 0, 0) };
            var btnCancel = new StyledButton { Text = "Cancel", BaseColor = Color.FromArgb(140, 150, 165), Width = 100, Height = 38 };
            btnLogin.Click += (s, e) => TryLogin();
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnRow.Controls.Add(btnLogin);
            btnRow.Controls.Add(btnCancel);

            _lblError = new Label { Text = "", Font = UITheme.FontSmall, ForeColor = UITheme.Danger, Dock = DockStyle.Top, Height = 22 };

            _txtPass = new TextBox { Font = UITheme.FontBody, Dock = DockStyle.Top, Height = 30, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            _txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) TryLogin(); };
            var lblPass = new Label { Text = "Password", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Top, Height = 24, Padding = new Padding(0, 8, 0, 0) };

            _txtUser = new TextBox { Font = UITheme.FontBody, Dock = DockStyle.Top, Height = 30, BorderStyle = BorderStyle.FixedSingle };
            var lblUser = new Label { Text = "Username", Font = UITheme.FontSubheading, ForeColor = UITheme.TextSecondary, Dock = DockStyle.Top, Height = 24, Padding = new Padding(0, 8, 0, 0) };

            var lblSub = new Label
            {
                Text = "Enter your administrator credentials to continue.",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 26
            };
            var lblTitle = new Label
            {
                Text = "Staff Sign In",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 34
            };

            body.Controls.Add(hint);
            body.Controls.Add(btnRow);
            body.Controls.Add(_lblError);
            body.Controls.Add(_txtPass);
            body.Controls.Add(lblPass);
            body.Controls.Add(_txtUser);
            body.Controls.Add(lblUser);
            body.Controls.Add(lblSub);
            body.Controls.Add(lblTitle);

            this.Controls.Add(body);
            this.Controls.Add(strip);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void TryLogin()
        {
            if (AdminAuth.Validate(_txtUser.Text.Trim(), _txtPass.Text))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                _lblError.Text = "Invalid username or password. Please try again.";
                _txtPass.Clear();
                _txtPass.Focus();
            }
        }
    }
}
