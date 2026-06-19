using TaskMaster.Utils;

namespace TaskMaster.Views.Controls;

// ─── Botão com estilo personalizado ──────────────────────────────────────────
public class BotaoApp : Button
{
    public enum EstiloBotao { Primario, Secundario, Perigo, Sucesso, Ghost }
    private EstiloBotao _estilo = EstiloBotao.Primario;
    private bool _hover;

    public EstiloBotao Estilo
    {
        get => _estilo;
        set { _estilo = value; Invalidate(); }
    }

    public BotaoApp()
    {
        FlatStyle     = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor        = Cursors.Hand;
        Font          = AppFonts.BodyBold;
        ForeColor     = AppColors.TextPrimary;
        Height        = 36;
        Padding       = new Padding(12, 0, 12, 0);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        Color bg = _estilo switch
        {
            EstiloBotao.Primario   => _hover ? AppColors.PrimaryHover : AppColors.Primary,
            EstiloBotao.Secundario => _hover ? AppColors.SurfaceElevated : AppColors.Surface,
            EstiloBotao.Perigo     => _hover ? Color.FromArgb(185, 28, 28) : AppColors.Danger,
            EstiloBotao.Sucesso    => _hover ? Color.FromArgb(21, 128, 61) : AppColors.Success,
            EstiloBotao.Ghost      => _hover ? AppColors.SurfaceElevated : Color.Transparent,
            _                      => AppColors.Primary
        };

        Color border = _estilo == EstiloBotao.Secundario ? AppColors.Border : bg;

        using var bgBrush = new SolidBrush(bg);
        using var borderPen = new Pen(border);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        DrawRoundedRect(g, bgBrush, borderPen, rect, 8);

        using var txtBrush = new SolidBrush(ForeColor);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(Text, Font, txtBrush, ClientRectangle, sf);
    }

    private static void DrawRoundedRect(Graphics g, Brush fill, Pen border, Rectangle r, int radius)
    {
        var path = GetRoundedPath(r, radius);
        g.FillPath(fill, path);
        g.DrawPath(border, path);
    }

    public static System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle r, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

// ─── Barra de progresso XP personalizada ─────────────────────────────────────
public class BarraXP : Control
{
    private double _progresso;
    private string _label = "";

    public double Progresso { get => _progresso; set { _progresso = Math.Max(0, Math.Min(1, value)); Invalidate(); } }
    public string Label     { get => _label;     set { _label = value; Invalidate(); } }

    public BarraXP() { Height = 22; DoubleBuffered = true; }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Fundo
        using var bgBrush = new SolidBrush(AppColors.SurfaceElevated);
        var bgRect = new Rectangle(0, 0, Width - 1, Height - 1);
        g.FillPath(bgBrush, BotaoApp.GetRoundedPath(bgRect, 10));

        // Preenchimento
        if (_progresso > 0)
        {
            int fillW = (int)((Width - 2) * _progresso);
            if (fillW > 0)
            {
                using var grad = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(1, 1, fillW, Height - 2),
                    AppColors.Primary, AppColors.Accent,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                var fillRect = new Rectangle(1, 1, fillW, Height - 2);
                g.FillPath(grad, BotaoApp.GetRoundedPath(fillRect, 9));
            }
        }

        // Texto
        if (!string.IsNullOrEmpty(_label))
        {
            using var txtBrush = new SolidBrush(AppColors.TextPrimary);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(_label, AppFonts.Small, txtBrush, ClientRectangle, sf);
        }
    }
}

// ─── Panel com borda arredondada ──────────────────────────────────────────────
public class CardPanel : Panel
{
    private int _radius = 12;
    private Color _borderColor = AppColors.Border;

    public int CornerRadius { get => _radius; set { _radius = value; Invalidate(); } }
    public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }

    public CardPanel()
    {
        DoubleBuffered = true;
        BackColor      = AppColors.Surface;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var bg   = new SolidBrush(BackColor);
        using var pen  = new Pen(_borderColor, 1);
        var path = BotaoApp.GetRoundedPath(rect, _radius);
        g.FillPath(bg, path);
        g.DrawPath(pen, path);
        base.OnPaint(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { /* evita flicker */ }
}

// ─── TextBox estilizado ───────────────────────────────────────────────────────
public class InputApp : TextBox
{
    public InputApp()
    {
        BackColor   = AppColors.SurfaceElevated;
        ForeColor   = AppColors.TextPrimary;
        BorderStyle = BorderStyle.FixedSingle;
        Font        = AppFonts.Body;
        Height      = 32;
    }
}

// ─── Label de badge colorido ──────────────────────────────────────────────────
public class BadgeLabel : Label
{
    private Color _badgeColor = AppColors.Primary;
    public Color BadgeColor { get => _badgeColor; set { _badgeColor = value; Invalidate(); } }

    public BadgeLabel()
    {
        AutoSize    = false;
        Height      = 22;
        Width       = 80;
        Font        = AppFonts.SmallBold;
        ForeColor   = Color.White;
        TextAlign   = ContentAlignment.MiddleCenter;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var bg  = new SolidBrush(_badgeColor);
        using var txt = new SolidBrush(ForeColor);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        g.FillPath(bg, BotaoApp.GetRoundedPath(rect, Height / 2));
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(Text, Font, txt, ClientRectangle, sf);
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
}
