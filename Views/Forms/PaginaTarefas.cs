using TaskMaster.Controllers;
using TaskMaster.Models;
using TaskMaster.Utils;
using TaskMaster.Views.Controls;

namespace TaskMaster.Views.Forms;

public class PaginaTarefas : UserControl
{
    private readonly TarefaController _tarefaCtrl;
    private readonly GamificacaoController _gamCtrl;
    private readonly Action _onXpAtualizado;

    // Filtros
    private InputApp     _txtBusca      = null!;
    private ComboBox     _cmbCategoria  = null!;
    private ComboBox     _cmbStatus     = null!;
    private BotaoApp     _btnNovaTarefa = null!;

    // Lista de tarefas
    private FlowLayoutPanel _painelLista = null!;

    // Estado dos filtros
    private string _filtroTexto   = "";
    private int?   _filtroCategoria = null;
    private StatusTarefa? _filtroStatus = null;

    public PaginaTarefas(TarefaController tarefaCtrl, GamificacaoController gamCtrl, Action onXpAtualizado)
    {
        _tarefaCtrl     = tarefaCtrl;
        _gamCtrl        = gamCtrl;
        _onXpAtualizado = onXpAtualizado;
        BackColor       = AppColors.Background;
        DoubleBuffered  = true;
        ConstruirUI();
        CarregarTarefas();
    }

    private void ConstruirUI()
    {
        // ── Título da página ──────────────────────────────────────────────────
        var lblTitulo = new Label
        {
            Text      = "Minhas Tarefas",
            Font      = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 46,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // ── Barra de filtros ──────────────────────────────────────────────────
        var painelFiltros = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = AppColors.Background
        };

        _txtBusca = new InputApp
        {
            PlaceholderText = "🔍  Buscar tarefa...",
            Width  = 260,
            Height = 36,
            Location = new Point(0, 8)
        };
        _txtBusca.TextChanged += (_, _) => { _filtroTexto = _txtBusca.Text; CarregarTarefas(); };

        _cmbCategoria = CriarComboEstilizado(160, 264 + 8);
        _cmbCategoria.Items.Add("Todas as categorias");
        _cmbCategoria.SelectedIndex = 0;
        CarregarCategorias();
        _cmbCategoria.SelectedIndexChanged += (_, _) =>
        {
            _filtroCategoria = _cmbCategoria.SelectedIndex == 0
                ? null
                : (int?)(_cmbCategoria.SelectedItem as dynamic)?.Id;
            CarregarTarefas();
        };

        _cmbStatus = CriarComboEstilizado(160, 264 + 8 + 168);
        _cmbStatus.Items.AddRange(new object[] { "Todos os status", "Pendente", "Em andamento", "Concluída" });
        _cmbStatus.SelectedIndex = 0;
        _cmbStatus.SelectedIndexChanged += (_, _) =>
        {
            _filtroStatus = _cmbStatus.SelectedIndex switch
            {
                1 => StatusTarefa.Pendente,
                2 => StatusTarefa.EmAndamento,
                3 => StatusTarefa.Concluida,
                _ => null
            };
            CarregarTarefas();
        };

        _btnNovaTarefa = new BotaoApp
        {
            Text     = "+ Nova Tarefa",
            Width    = 140,
            Height   = 36,
            Location = new Point(painelFiltros.Width - 150, 8),
            Anchor   = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnNovaTarefa.Click += (_, _) => AbrirFormTarefa(null);

        painelFiltros.Controls.AddRange(new Control[]
            { _txtBusca, _cmbCategoria, _cmbStatus, _btnNovaTarefa });

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppColors.Border };

        // ── Lista de tarefas ──────────────────────────────────────────────────
        _painelLista = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoScroll    = true,
            BackColor     = AppColors.Background,
            Padding       = new Padding(0, 8, 0, 0)
        };

        Controls.Add(_painelLista);
        Controls.Add(sep);
        Controls.Add(painelFiltros);
        Controls.Add(lblTitulo);
    }

    private void CarregarCategorias()
    {
        var repo = new Data.CategoriaRepository();
        var cats = repo.ObterTodas();
        foreach (var cat in cats)
            _cmbCategoria.Items.Add(cat);
        _cmbCategoria.DisplayMember = "Nome";
    }

    private void CarregarTarefas()
    {
        _painelLista.SuspendLayout();
        _painelLista.Controls.Clear();

        var tarefas = _tarefaCtrl.ObterTarefas(
            categoriaId: _filtroCategoria,
            status:      _filtroStatus,
            filtroTexto: _filtroTexto.Length > 0 ? _filtroTexto : null);

        if (tarefas.Count == 0)
        {
            var lblVazio = new Label
            {
                Text      = "Nenhuma tarefa encontrada.\nClique em '+ Nova Tarefa' para começar!",
                Font      = AppFonts.Subtitle,
                ForeColor = AppColors.TextMuted,
                AutoSize  = false,
                Width     = _painelLista.Width - 40,
                Height    = 80,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _painelLista.Controls.Add(lblVazio);
        }
        else
        {
            foreach (var t in tarefas)
                _painelLista.Controls.Add(CriarCardTarefa(t));
        }

        _painelLista.ResumeLayout();
    }

    private Control CriarCardTarefa(Tarefa t)
    {
        var card = new CardPanel
        {
            Width       = _painelLista.Width - 24,
            Height      = t.Subtarefas.Count > 0 ? 110 : 80,
            Margin      = new Padding(0, 0, 0, 8),
            BackColor   = t.EstaVencida() ? Color.FromArgb(50, 239, 68, 68) : AppColors.Surface
        };

        // Barra lateral colorida por prioridade
        var barraLateral = new Panel
        {
            Width     = 4,
            Height    = card.Height,
            Location  = new Point(0, 0),
            BackColor = AppColors.ParaPrioridade(t.Prioridade)
        };
        // Arredondamento manual não disponível em Panel simples — usamos apenas a cor

        // Ícone de categoria
        var lblIcone = new Label
        {
            Text      = t.Categoria?.Icone ?? "📋",
            Font      = AppFonts.Icon,
            ForeColor = AppColors.TextPrimary,
            AutoSize  = false,
            Width     = 36,
            Height    = 36,
            Location  = new Point(14, (card.Height - 36) / 2),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Título
        var lblTitulo = new Label
        {
            Text      = t.Status == StatusTarefa.Concluida ? $"✓ {t.Titulo}" : t.Titulo,
            Font      = t.Status == StatusTarefa.Concluida ? AppFonts.Body : AppFonts.BodyBold,
            ForeColor = t.Status == StatusTarefa.Concluida ? AppColors.TextMuted : AppColors.TextPrimary,
            AutoSize  = false,
            Width     = card.Width - 300,
            Height    = 22,
            Location  = new Point(56, t.Subtarefas.Count > 0 ? 10 : (card.Height / 2) - 22
            )
        };

        // Score de prioridade
        double score = t.CalcularScore();
        var lblScore = new Label
        {
            Text      = $"Score: {score:P0}",
            Font      = AppFonts.Small,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(56, lblTitulo.Bottom + 2)
        };

        // Badge de prioridade
        var badge = new BadgeLabel
        {
            Text       = t.Prioridade.ToString(),
            BadgeColor = AppColors.ParaPrioridade(t.Prioridade),
            Width      = 70,
            Location   = new Point(card.Width - 270, (card.Height - 22) / 2)
        };

        // Data de prazo
        var lblPrazo = new Label
        {
            Text      = t.DataPrazo.HasValue
                ? (t.EstaVencida() ? $"⚠ Venceu {t.DataPrazo.Value:dd/MM}" : $"📅 {t.DataPrazo.Value:dd/MM/yy}")
                : "Sem prazo",
            Font      = AppFonts.Small,
            ForeColor = t.EstaVencida() ? AppColors.Danger : t.EstaPrazoProximo() ? AppColors.Warning : AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(card.Width - 190, (card.Height - 18) / 2)
        };

        // XP
        var lblXp = new Label
        {
            Text      = $"+{t.XpRecompensa} XP",
            Font      = AppFonts.SmallBold,
            ForeColor = AppColors.Gold,
            AutoSize  = true,
            Location  = new Point(card.Width - 85, (card.Height - 18) / 2)
        };

        // Botões de ação
        int btnY = (card.Height - 26) / 2;

        var btnConcluir = new BotaoApp
        {
            Text     = t.Status == StatusTarefa.Concluida ? "✓" : "✔",
            Estilo   = t.Status == StatusTarefa.Concluida ? BotaoApp.EstiloBotao.Ghost : BotaoApp.EstiloBotao.Sucesso,
            Width    = 34,
            Height   = 28,
            Location = new Point(card.Width - 106, btnY - 2),
            Enabled  = t.Status != StatusTarefa.Concluida
        };
        btnConcluir.Click += (_, _) => ConcluirTarefa(t.Id);

        var btnEditar = new BotaoApp
        {
            Text     = "✏",
            Estilo   = BotaoApp.EstiloBotao.Secundario,
            Width    = 34,
            Height   = 28,
            Location = new Point(card.Width - 68, btnY - 2)
        };
        btnEditar.Click += (_, _) => AbrirFormTarefa(t);

        var btnExcluir = new BotaoApp
        {
            Text     = "🗑",
            Estilo   = BotaoApp.EstiloBotao.Perigo,
            Width    = 34,
            Height   = 28,
            Location = new Point(card.Width - 30, btnY - 2)
        };
        btnExcluir.Click += (_, _) => ExcluirTarefa(t.Id, t.Titulo);

        card.Controls.AddRange(new Control[]
            { barraLateral, lblIcone, lblTitulo, lblScore, badge, lblPrazo, lblXp, btnConcluir, btnEditar, btnExcluir });

        // Subtarefas
        if (t.Subtarefas.Count > 0)
        {
            var painelSubs = new FlowLayoutPanel
            {
                Location      = new Point(56, lblTitulo.Bottom + lblScore.Height + 6),
                Width         = card.Width - 300,
                Height        = 22,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor     = Color.Transparent,
                WrapContents  = false
            };
            int done  = t.Subtarefas.Count(s => s.Concluida);
            int total = t.Subtarefas.Count;
            var lblSubs = new Label
            {
                Text      = $"Subtarefas: {done}/{total}",
                Font      = AppFonts.Small,
                ForeColor = done == total ? AppColors.Success : AppColors.TextSecondary,
                AutoSize  = true
            };
            painelSubs.Controls.Add(lblSubs);
            card.Controls.Add(painelSubs);
        }

        return card;
    }

    private void ConcluirTarefa(int id)
    {
        var (xp, conquistas) = _tarefaCtrl.ConcluirTarefa(id);
        CarregarTarefas();
        _onXpAtualizado();

        // Feedback visual
        var msg = $"✅ Tarefa concluída! +{xp} XP";
        if (conquistas.Count > 0)
            msg += $"\n🏆 Nova conquista: {conquistas[0].Icone} {conquistas[0].Nome}!";

        MostrarToast(msg, conquistas.Count > 0 ? AppColors.Gold : AppColors.Success);
    }

    private void ExcluirTarefa(int id, string titulo)
    {
        var res = MessageBox.Show(
            $"Excluir a tarefa '{titulo}'?",
            "Confirmar exclusão",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (res != DialogResult.Yes) return;
        _tarefaCtrl.ExcluirTarefa(id);
        CarregarTarefas();
    }

    private void AbrirFormTarefa(Tarefa? tarefa)
    {
        using var form = new FormTarefa(_tarefaCtrl, tarefa);
        if (form.ShowDialog() == DialogResult.OK)
            CarregarTarefas();
    }

    private void MostrarToast(string mensagem, Color cor)
    {
        var toast = new Form
        {
            StartPosition   = FormStartPosition.Manual,
            FormBorderStyle = FormBorderStyle.None,
            Size            = new Size(320, 60),
            BackColor       = cor,
            Opacity         = 0.95,
            TopMost         = true,
            ShowInTaskbar   = false
        };
        var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        toast.Location = new Point(screen.Right - 340, screen.Bottom - 80);
        var lbl = new Label
        {
            Text      = mensagem,
            Font      = AppFonts.Small,
            ForeColor = Color.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding   = new Padding(8)
        };
        toast.Controls.Add(lbl);
        toast.Show();
        var t = new System.Windows.Forms.Timer { Interval = 3000 };
        t.Tick += (_, _) => { t.Stop(); toast.Close(); };
        t.Start();
    }

    private static ComboBox CriarComboEstilizado(int width, int x)
    {
        return new ComboBox
        {
            Width         = width,
            Height        = 36,
            Location      = new Point(x, 8),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor     = AppColors.SurfaceElevated,
            ForeColor     = AppColors.TextPrimary,
            FlatStyle     = FlatStyle.Flat,
            Font          = AppFonts.Body
        };
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        CarregarTarefas();
    }
}
