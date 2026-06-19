using TaskMaster.Controllers;
using TaskMaster.Models;
using TaskMaster.Utils;
using TaskMaster.Views.Controls;
using TaskMaster.Data;

namespace TaskMaster.Views.Forms;

// ─── Página de Conquistas ─────────────────────────────────────────────────────
public class PaginaConquistas : UserControl
{
    private readonly GamificacaoController _gamCtrl;

    public PaginaConquistas(GamificacaoController gamCtrl)
    {
        _gamCtrl       = gamCtrl;
        BackColor      = AppColors.Background;
        DoubleBuffered = true;
        ConstruirUI();
    }

    private void ConstruirUI()
    {
        var lblTitulo = new Label
        {
            Text      = "Conquistas",
            Font      = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 46
        };

        var conquistas  = _gamCtrl.ObterConquistas();
        int desbloqueadas = conquistas.Count(c => c.Desbloqueada);

        var lblProgresso = new Label
        {
            Text      = $"{desbloqueadas} de {conquistas.Count} conquistas desbloqueadas",
            Font      = AppFonts.Subtitle,
            ForeColor = AppColors.TextSecondary,
            Dock      = DockStyle.Top,
            Height    = 28
        };

        var painel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = true,
            AutoScroll    = true,
            BackColor     = AppColors.Background,
            Padding       = new Padding(0, 8, 0, 0)
        };

        foreach (var c in conquistas)
            painel.Controls.Add(CriarCardConquista(c));

        Controls.Add(painel);
        Controls.Add(lblProgresso);
        Controls.Add(lblTitulo);
    }

    private static CardPanel CriarCardConquista(Conquista c)
    {
        var desbloqueada = c.Desbloqueada;
        var card = new CardPanel
        {
            Width       = 210,
            Height      = 130,
            Margin      = new Padding(0, 0, 12, 12),
            BackColor   = desbloqueada ? AppColors.Surface : AppColors.Background,
            BorderColor = desbloqueada ? AppColors.Gold    : AppColors.Border,
        };

        var lblIcone = new Label
        {
            Text      = desbloqueada ? c.Icone : "🔒",
            Font      = new Font("Segoe UI Emoji", 24),
            ForeColor = desbloqueada ? AppColors.Gold : AppColors.TextMuted,
            Location  = new Point(12, 14),
            AutoSize  = true
        };

        var lblNome = new Label
        {
            Text      = c.Nome,
            Font      = AppFonts.SmallBold,
            ForeColor = desbloqueada ? AppColors.TextPrimary : AppColors.TextMuted,
            Location  = new Point(54, 16),
            Width     = 148,
            Height    = 36,
            AutoSize  = false
        };

        var lblDesc = new Label
        {
            Text      = c.Descricao,
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextSecondary,
            Location  = new Point(12, 64),
            Width     = 190,
            Height    = 36,
            AutoSize  = false
        };

        var lblXp = new Label
        {
            Text      = $"+{c.XpBonus} XP",
            Font      = AppFonts.SmallBold,
            ForeColor = desbloqueada ? AppColors.Gold : AppColors.TextMuted,
            Location  = new Point(12, 104),
            AutoSize  = true
        };

        if (desbloqueada && c.DataDesbloqueio.HasValue)
        {
            var lblData = new Label
            {
                Text      = c.DataDesbloqueio.Value.ToString("dd/MM/yyyy"),
                Font      = AppFonts.Small,
                ForeColor = AppColors.TextMuted,
                Location  = new Point(90, 108),
                AutoSize  = true
            };
            card.Controls.Add(lblData);
        }

        card.Controls.AddRange(new Control[] { lblIcone, lblNome, lblDesc, lblXp });
        return card;
    }
}

// ─── Página de Histórico ──────────────────────────────────────────────────────
public class PaginaHistorico : UserControl
{
    public PaginaHistorico()
    {
        BackColor      = AppColors.Background;
        DoubleBuffered = true;
        ConstruirUI();
    }

    private void ConstruirUI()
    {
        var lblTitulo = new Label
        {
            Text      = "Histórico de Atividades",
            Font      = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 46
        };

        var repo  = new LogRepository();
        var logs  = repo.ObterRecentes(100);

        var listView = new ListView
        {
            Dock          = DockStyle.Fill,
            View          = View.Details,
            FullRowSelect = true,
            GridLines     = false,
            BackColor     = AppColors.Surface,
            ForeColor     = AppColors.TextPrimary,
            BorderStyle   = BorderStyle.None,
            Font          = AppFonts.Body,
            OwnerDraw     = false
        };

        listView.Columns.Add("Data/Hora",    140);
        listView.Columns.Add("Tipo",          110);
        listView.Columns.Add("Descrição",     380);
        listView.Columns.Add("XP",             60);

        foreach (var log in logs)
        {
            string tipoStr = log.Tipo switch
            {
                TipoLog.TarefaCriada           => "📋 Criada",
                TipoLog.TarefaConcluida        => "✅ Concluída",
                TipoLog.TarefaEditada          => "✏️ Editada",
                TipoLog.TarefaCancelada        => "🗑 Excluída",
                TipoLog.ConquistaDesbloqueada  => "🏆 Conquista",
                TipoLog.NivelSubiu             => "⬆️ Nível",
                TipoLog.StreakAtualizado        => "🔥 Streak",
                _                              => log.Tipo.ToString()
            };

            var item = new ListViewItem(log.DataHora.ToString("dd/MM/yy HH:mm"));
            item.SubItems.Add(tipoStr);
            item.SubItems.Add(log.Descricao);
            item.SubItems.Add(log.XpGanho > 0 ? $"+{log.XpGanho}" : "-");
            item.BackColor = AppColors.Surface;
            item.ForeColor = log.Tipo == TipoLog.TarefaConcluida ? AppColors.Success :
                             log.Tipo == TipoLog.ConquistaDesbloqueada ? AppColors.Gold :
                             log.Tipo == TipoLog.NivelSubiu ? AppColors.PrimaryLight :
                             AppColors.TextPrimary;
            listView.Items.Add(item);
        }

        if (logs.Count == 0)
        {
            var lblVazio = new Label
            {
                Text      = "Nenhuma atividade registrada ainda.",
                Font      = AppFonts.Subtitle,
                ForeColor = AppColors.TextMuted,
                Dock      = DockStyle.Top,
                Height    = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblVazio);
        }

        Controls.Add(listView);
        Controls.Add(lblTitulo);
    }
}
