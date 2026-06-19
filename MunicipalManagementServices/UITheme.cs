using System.Drawing;

namespace MunicipalManagementServices
{
    /// <summary>
    /// Centralised design tokens — one place to change the entire app's look.
    /// </summary>
    public static class UITheme
    {
        // ── Palette ──────────────────────────────────────────
        public static readonly Color Primary       = Color.FromArgb(15,  76, 129);   // deep navy blue
        public static readonly Color PrimaryLight  = Color.FromArgb(26, 115, 190);   // medium blue
        public static readonly Color PrimaryDark   = Color.FromArgb(8,   47,  83);   // darkest blue

        public static readonly Color Accent        = Color.FromArgb(0,  188, 140);   // teal green
        public static readonly Color AccentDark    = Color.FromArgb(0,  150, 110);

        public static readonly Color Danger        = Color.FromArgb(214,  48,  49);
        public static readonly Color Warning       = Color.FromArgb(243, 156,  18);
        public static readonly Color Success       = Color.FromArgb(39,  174,  96);
        public static readonly Color Info          = Color.FromArgb(52,  152, 219);

        public static readonly Color Surface       = Color.FromArgb(248, 250, 252);  // page bg
        public static readonly Color Card          = Color.White;
        public static readonly Color Border        = Color.FromArgb(220, 228, 238);
        public static readonly Color TextPrimary   = Color.FromArgb(15,  23,  42);
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        public static readonly Color TextMuted     = Color.FromArgb(160, 174, 192);

        public static readonly Color TabActive     = Color.FromArgb(15,  76, 129);
        public static readonly Color TabInactive   = Color.FromArgb(241, 245, 249);

        // ── Typography ───────────────────────────────────────
        public static readonly Font FontTitle      = new Font("Segoe UI", 20, FontStyle.Bold);
        public static readonly Font FontHeading    = new Font("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font FontSubheading = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontBody       = new Font("Segoe UI",  9.5f);
        public static readonly Font FontSmall      = new Font("Segoe UI",  8.5f);
        public static readonly Font FontBadge      = new Font("Segoe UI",  8,  FontStyle.Bold);

        // ── Spacing ──────────────────────────────────────────
        public const int RadiusSmall  = 4;
        public const int RadiusMedium = 8;
        public const int PadBase      = 16;
    }
}
