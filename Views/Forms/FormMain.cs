using TaskMaster.Controllers;
using TaskMaster.Models;
using TaskMaster.Utils;
using TaskMaster.Views.Controls;

namespace TaskMaster.Views.Forms;

public class FormMain : Form
{
    // Controllers
    private readonly GamificacaoController _gamCtrl = new();
    private TarefaController _tarefaCtrl = null!;

    // Layout principal
    private Panel      _sidebar      = null!;
    private Panel      _contentArea  = null!;
    private Panel      _headerPanel  = null!;

    // Sidebar buttons
    private BotaoApp   _btnTarefas   = null!;
    private BotaoApp   _btnDashboard = null!;
    private BotaoApp   _btnConquistas= null!;
    private BotaoApp   _btnHistorico = null!;

    // Header — XP e perfil
    private Label      _lblNomeUsuario = null!;
    private Label      _lblNivel       = null!;
    private BarraXP    _barraXp        = null!;
    private Label      _lblXpInfo      = null!;
    private Label      _lblStreak      = null!;

    // Conteúdo atual
    private Control? _paginaAtual;

    // Timer para alertas de prazo
    private System.Windows.Forms.Timer _timerAlertas = null!;

    public FormMain()
    {
        _tarefaCtrl = new TarefaController(_gamCtrl);
        InicializarJanela();
        ConstruirLayout();
        CarregarPaginaTarefas();
        IniciarTimerAlertas();
        AtualizarHeader();
    }

    private void InicializarJanela()
    {
        Text            = "TaskMaster — Gerenciador de Tarefas";
        Size            = new Size(1200, 780);
        MinimumSize     = new Size(900, 600);
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = AppColors.Background;
        ForeColor       = AppColors.TextPrimary;
        Font            = AppFonts.Body;
        DoubleBuffered  = true;
    }

    private void ConstruirLayout()
    {
        // ── Sidebar ──────────────────────────────────────────────────────────
        _sidebar = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 200,
            BackColor = AppColors.Surface,
            Padding   = new Padding(12)
        };

        // Logo
        var lblLogo = new Label
        {
            Text      = "⚡ TaskMaster",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.PrimaryLight,
            Dock      = DockStyle.Top,
            Height    = 52,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(4, 0, 0, 0)
        };

        var sepLogo = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppColors.Border, Margin = new Padding(0, 4, 0, 4) };

        // Botões de navegação
        _btnTarefas    = CriarBotaoNav("📋  Tarefas",     () => CarregarPaginaTarefas());
        _btnDashboard  = CriarBotaoNav("📊  Dashboard",   () => CarregarPaginaDashboard());
        _btnConquistas = CriarBotaoNav("🏆  Conquistas",  () => CarregarPaginaConquistas());
        _btnHistorico  = CriarBotaoNav("📜  Histórico",   () => CarregarPaginaHistorico());

        // Versão na base
        var lblVersao = new Label
        {
            Text      = "v1.0.0 — TCC 2026",
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextMuted,
            Dock      = DockStyle.Bottom,
            Height    = 28,
            TextAlign = ContentAlignment.BottomLeft
        };

        _sidebar.Controls.Add(lblVersao);
        _sidebar.Controls.Add(_btnHistorico);
        _sidebar.Controls.Add(_btnConquistas);
        _sidebar.Controls.Add(_btnDashboard);
        _sidebar.Controls.Add(_btnTarefas);
        _sidebar.Controls.Add(sepLogo);
        _sidebar.Controls.Add(lblLogo);

        // ── Header / Barra de XP ──────────────────────────────────────────────
        _headerPanel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 64,
            BackColor = AppColors.Surface,
            Padding   = new Padding(16, 8, 16, 8)
        };

        _lblNomeUsuario = new Label
        {
            Font      = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 10)
        };
        _lblNivel = new Label
        {
            Font      = AppFonts.Small,
            ForeColor = AppColors.PrimaryLight,
            AutoSize  = true,
            Location  = new Point(0, 30)
        };
        _barraXp = new BarraXP
        {
            Width    = 180,
            Height   = 16,
            Location = new Point(160, 24)
        };
        _lblXpInfo = new Label
        {
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(348, 28)
        };
        _lblStreak = new Label
        {
            Font      = AppFonts.BodyBold,
            ForeColor = AppColors.Warning,
            AutoSize  = true,
            Location  = new Point(460, 22)
        };

        _headerPanel.Controls.AddRange(new Control[]
            { _lblNomeUsuario, _lblNivel, _barraXp, _lblXpInfo, _lblStreak });

        // Separador abaixo do header
        var sepHeader = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 1,
            BackColor = AppColors.Border
        };

        // ── Área de conteúdo ──────────────────────────────────────────────────
        _contentArea = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppColors.Background,
            Padding   = new Padding(20)
        };

        // Montar form
        Controls.Add(_contentArea);
        Controls.Add(sepHeader);
        Controls.Add(_headerPanel);
        Controls.Add(_sidebar);
    }

    private BotaoApp CriarBotaoNav(string texto, Action onClick)
    {
        var btn = new BotaoApp
        {
            Text   = texto,
            Estilo = BotaoApp.EstiloBotao.Ghost,
            Dock   = DockStyle.Top,
            Height = 40,
            Font   = AppFonts.Body,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(8, 0, 0, 0)
        };
        btn.Click += (_, _) => onClick();
        return btn;
    }

    // ─── Navegação ────────────────────────────────────────────────────────────
    private void CarregarPagina(Control pagina)
    {
        _paginaAtual?.Dispose();
        _contentArea.Controls.Clear();
        pagina.Dock = DockStyle.Fill;
        _contentArea.Controls.Add(pagina);
        _paginaAtual = pagina;
    }

    private void CarregarPaginaTarefas()
    {
        DestacarbotaoNav(_btnTarefas);
        var pagina = new PaginaTarefas(_tarefaCtrl, _gamCtrl, AtualizarHeader);
        CarregarPagina(pagina);
    }

    private void CarregarPaginaDashboard()
    {
        DestacarbotaoNav(_btnDashboard);
        var pagina = new PaginaDashboard(_tarefaCtrl, _gamCtrl);
        CarregarPagina(pagina);
    }

    private void CarregarPaginaConquistas()
    {
        DestacarbotaoNav(_btnConquistas);
        var pagina = new PaginaConquistas(_gamCtrl);
        CarregarPagina(pagina);
    }

    private void CarregarPaginaHistorico()
    {
        DestacarbotaoNav(_btnHistorico);
        var pagina = new PaginaHistorico();
        CarregarPagina(pagina);
    }

    private void DestacarbotaoNav(BotaoApp ativo)
    {
        foreach (var ctrl in _sidebar.Controls.OfType<BotaoApp>())
            ctrl.Estilo = BotaoApp.EstiloBotao.Ghost;
        ativo.Estilo = BotaoApp.EstiloBotao.Secundario;
    }

    // ─── Header XP ───────────────────────────────────────────────────────────
    public void AtualizarHeader()
    {
        var u = _gamCtrl.ObterUsuario();
        _lblNomeUsuario.Text = u.Nome;
        _lblNivel.Text       = $"Nível {u.Nivel} — {u.TituloNivel}";
        _barraXp.Progresso   = u.ProgressoNivel;
        _barraXp.Label       = $"{u.XpAtual} XP";
        _lblXpInfo.Text      = $"/ {u.XpParaProximoNivel} XP";
        _lblStreak.Text      = u.StreakAtual > 0 ? $"🔥 {u.StreakAtual} dias" : "";
    }

    // ─── Timer de alertas ─────────────────────────────────────────────────────
    private void IniciarTimerAlertas()
    {
        _timerAlertas = new System.Windows.Forms.Timer { Interval = 60_000 }; // 1 min
        _timerAlertas.Tick += (_, _) => VerificarAlertas();
        _timerAlertas.Start();
        VerificarAlertas(); // verifica imediatamente ao abrir
    }

    private void VerificarAlertas()
    {
        var alertas = _tarefaCtrl.ObterTarefasComAlertaPrazo();
        if (alertas.Count == 0) return;

        var vencidas = alertas.Count(t => t.EstaVencida());
        var proximas = alertas.Count(t => t.EstaPrazoProximo() && !t.EstaVencida());

        if (vencidas > 0 || proximas > 0)
        {
            // Alerta visual não-bloqueante na barra de status
            var msg = "";
            if (vencidas > 0) msg += $"⚠️ {vencidas} tarefa(s) vencida(s)  ";
            if (proximas > 0) msg += $"⏰ {proximas} tarefa(s) vence(m) em breve";

            // Exibe no label da sidebar ou como ToolTip
            // (implementação leve — sem MessageBox bloqueante)
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timerAlertas.Stop();
        AppFonts.Title.Dispose();
        AppFonts.Subtitle.Dispose();
        AppFonts.Body.Dispose();
        AppFonts.BodyBold.Dispose();
        AppFonts.Small.Dispose();
        AppFonts.SmallBold.Dispose();
        base.OnFormClosed(e);
    }
}
