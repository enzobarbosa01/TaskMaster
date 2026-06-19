namespace TaskMaster.Utils;

public static class AppColors
{
    // Fundo
    public static readonly Color Background     = Color.FromArgb(18, 18, 24);
    public static readonly Color Surface        = Color.FromArgb(28, 28, 36);
    public static readonly Color SurfaceElevated= Color.FromArgb(38, 38, 50);
    public static readonly Color Border         = Color.FromArgb(55, 55, 75);

    // Primária / Acento
    public static readonly Color Primary        = Color.FromArgb(99, 102, 241);  // indigo
    public static readonly Color PrimaryHover   = Color.FromArgb(79,  82, 220);
    public static readonly Color PrimaryLight   = Color.FromArgb(199, 200, 255);
    public static readonly Color Accent         = Color.FromArgb(139, 92,  246); // violeta

    // Semânticas
    public static readonly Color Success        = Color.FromArgb(34,  197, 94);
    public static readonly Color Warning        = Color.FromArgb(234, 179, 8);
    public static readonly Color Danger         = Color.FromArgb(239, 68,  68);
    public static readonly Color Info           = Color.FromArgb(59,  130, 246);

    // Texto
    public static readonly Color TextPrimary    = Color.FromArgb(240, 240, 250);
    public static readonly Color TextSecondary  = Color.FromArgb(160, 160, 180);
    public static readonly Color TextMuted      = Color.FromArgb(100, 100, 120);

    // Prioridades
    public static readonly Color PrioridadeBaixa   = Color.FromArgb(34,  197, 94);
    public static readonly Color PrioridadeMedia   = Color.FromArgb(234, 179, 8);
    public static readonly Color PrioridadeAlta    = Color.FromArgb(239, 68,  68);
    public static readonly Color PrioridadeCritica = Color.FromArgb(220, 38,  38);

    // XP / Gamificação
    public static readonly Color Gold           = Color.FromArgb(251, 191, 36);
    public static readonly Color Silver         = Color.FromArgb(148, 163, 184);
    public static readonly Color Bronze         = Color.FromArgb(180, 120, 60);

    public static Color ParaPrioridade(Models.PrioridadeTarefa p) => p switch
    {
        Models.PrioridadeTarefa.Baixa   => PrioridadeBaixa,
        Models.PrioridadeTarefa.Media   => PrioridadeMedia,
        Models.PrioridadeTarefa.Alta    => PrioridadeAlta,
        Models.PrioridadeTarefa.Critica => PrioridadeCritica,
        _                               => PrioridadeMedia
    };
}

public static class AppFonts
{
    public static readonly Font Title     = new("Segoe UI", 18, FontStyle.Bold);
    public static readonly Font Subtitle  = new("Segoe UI", 11, FontStyle.Regular);
    public static readonly Font Body      = new("Segoe UI", 10, FontStyle.Regular);
    public static readonly Font BodyBold  = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font Small     = new("Segoe UI",  9, FontStyle.Regular);
    public static readonly Font SmallBold = new("Segoe UI",  9, FontStyle.Bold);
    public static readonly Font Mono      = new("Consolas",  9, FontStyle.Regular);
    public static readonly Font Icon      = new("Segoe UI Emoji", 14);
}
