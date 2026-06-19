using TaskMaster.Controllers;
using TaskMaster.Models;
using TaskMaster.Utils;
using TaskMaster.Views.Controls;

namespace TaskMaster.Views.Forms;

public class PaginaDashboard : UserControl
{
    private readonly TarefaController _tarefaCtrl;
    private readonly GamificacaoController _gamCtrl;

    public PaginaDashboard(TarefaController tarefaCtrl, GamificacaoController gamCtrl)
    {
        _tarefaCtrl = tarefaCtrl;
        _gamCtrl    = gamCtrl;
        BackColor   = AppColors.Background;
        DoubleBuffered = true;
        ConstruirUI();
    }

    private void ConstruirUI()
    {
        var u      = _gamCtrl.ObterUsuario();
        var tarefas = _tarefaCtrl.ObterTarefas(ordenarPorScore: false);
        int hoje   = _tarefaCtrl.ContarConcluidasHoje();

        // Título
        var lblTitulo = new Label
        {
            Text      = "Dashboard",
            Font      = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 46
        };

        // ── Cards de métricas ─────────────────────────────────────────────────
        var painelCards = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 110,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            BackColor     = AppColors.Background,
            Padding       = new Padding(0, 8, 0, 8)
        };

        painelCards.Controls.Add(CriarCardMetrica("⚡ Nível",          $"{u.Nivel}", u.TituloNivel,       AppColors.Primary));
        painelCards.Controls.Add(CriarCardMetrica("🔥 Streak",         $"{u.StreakAtual} dias", $"Recorde: {u.MaiorStreak}", AppColors.Warning));
        painelCards.Controls.Add(CriarCardMetrica("✅ Hoje",           $"{hoje}/{u.MetaDiaria}", "meta diária",             AppColors.Success));
        painelCards.Controls.Add(CriarCardMetrica("📋 Total concluídas",$"{u.TarefasConcluidasTotal}", "tarefas concluídas", AppColors.Accent));
        painelCards.Controls.Add(CriarCardMetrica("💰 XP Total",       $"{u.XpAtual} XP",     $"Próx. nível: {u.XpParaProximoNivel}", AppColors.Gold));

        // ── Gráfico de prioridades ────────────────────────────────────────────
        var lblGrafTitulo = new Label
        {
            Text      = "Distribuição por Prioridade",
            Font      = AppFonts.BodyBold,
            ForeColor = AppColors.TextSecondary,
            Dock      = DockStyle.Top,
            Height    = 28
        };

        var graficoPrioridade = new GraficoPrioridade(tarefas)
        {
            Dock      = DockStyle.Top,
            Height    = 180,
            BackColor = AppColors.Background
        };

        // ── Progresso XP ──────────────────────────────────────────────────────
        var lblXpTitulo = new Label
        {
            Text      = "Progresso de Experiência",
            Font      = AppFonts.BodyBold,
            ForeColor = AppColors.TextSecondary,
            Dock      = DockStyle.Top,
            Height    = 28
        };

        var barraXp = new BarraXP
        {
            Dock      = DockStyle.Top,
            Height    = 30,
            Progresso = u.ProgressoNivel,
            Label     = $"{u.XpAtual} / {u.XpParaProximoNivel} XP — Nível {u.Nivel}"
        };

        // ── Meta diária ───────────────────────────────────────────────────────
        var lblMetaTitulo = new Label
        {
            Text      = $"Meta diária: {hoje} de {u.MetaDiaria} tarefas",
            Font      = AppFonts.BodyBold,
            ForeColor = AppColors.TextSecondary,
            Dock      = DockStyle.Top,
            Height    = 28
        };

        var barraHoje = new BarraXP
        {
            Dock      = DockStyle.Top,
            Height    = 30,
            Progresso = u.MetaDiaria > 0 ? Math.Min(1.0, (double)hoje / u.MetaDiaria) : 0,
            Label     = $"{hoje} / {u.MetaDiaria} tarefas hoje"
        };

        Controls.Add(barraHoje);
        Controls.Add(lblMetaTitulo);
        Controls.Add(barraXp);
        Controls.Add(lblXpTitulo);
        Controls.Add(graficoPrioridade);
        Controls.Add(lblGrafTitulo);
        Controls.Add(painelCards);
        Controls.Add(lblTitulo);
    }

    private static CardPanel CriarCardMetrica(string titulo, string valor, string subtexto, Color cor)
    {
        var card = new CardPanel
        {
            Width       = 180,
            Height      = 90,
            Margin      = new Padding(0, 0, 12, 0),
            BackColor   = AppColors.Surface,
            BorderColor = cor
        };
        card.Controls.Add(new Label
        {
            Text      = titulo,
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextMuted,
            Location  = new Point(12, 10),
            AutoSize  = true
        });
        card.Controls.Add(new Label
        {
            Text      = valor,
            Font      = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = cor,
            Location  = new Point(12, 26),
            AutoSize  = true
        });
        card.Controls.Add(new Label
        {
            Text      = subtexto,
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextSecondary,
            Location  = new Point(12, 65),
            AutoSize  = true
        });
        return card;
    }
}

// ─── Controle de gráfico de barras simples ────────────────────────────────────
public class GraficoPrioridade : Control
{
    private readonly List<Tarefa> _tarefas;

    public GraficoPrioridade(List<Tarefa> tarefas)
    {
        _tarefas = tarefas;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var grupos = new[]
        {
            ("Baixa",   PrioridadeTarefa.Baixa,   AppColors.PrioridadeBaixa),
            ("Média",   PrioridadeTarefa.Media,   AppColors.PrioridadeMedia),
            ("Alta",    PrioridadeTarefa.Alta,    AppColors.PrioridadeAlta),
            ("Crítica", PrioridadeTarefa.Critica, AppColors.PrioridadeCritica),
        };

        int total   = _tarefas.Count;
        int maxCount= _tarefas.Count == 0 ? 1 :
                      grupos.Max(g2 => _tarefas.Count(t => t.Prioridade == g2.Item2 && t.Status != StatusTarefa.Concluida));
        if (maxCount == 0) maxCount = 1;

        int barW    = 80;
        int gap     = 40;
        int startX  = 40;
        int maxH    = Height - 60;

        for (int i = 0; i < grupos.Length; i++)
        {
            var (nome, prio, cor) = grupos[i];
            int count  = _tarefas.Count(t => t.Prioridade == prio && t.Status != StatusTarefa.Concluida);
            int barH   = maxCount > 0 ? (int)((double)count / maxCount * maxH) : 0;
            int x      = startX + i * (barW + gap);
            int y      = Height - 40 - barH;

            // Barra
            using var brush = new SolidBrush(cor);
            g.FillRectangle(brush, x, y, barW, barH);

            // Valor
            using var txtBrush = new SolidBrush(AppColors.TextPrimary);
            g.DrawString(count.ToString(), AppFonts.BodyBold, txtBrush,
                x + barW / 2f - 8, Math.Max(4, y - 22));

            // Rótulo
            using var lblBrush = new SolidBrush(AppColors.TextSecondary);
            g.DrawString(nome, AppFonts.Small, lblBrush,
                x + (barW - 40) / 2f, Height - 36);
        }

        // Linha base
        using var linePen = new Pen(AppColors.Border);
        g.DrawLine(linePen, 20, Height - 40, Width - 20, Height - 40);
    }
}
