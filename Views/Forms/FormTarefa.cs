using TaskMaster.Controllers;
using TaskMaster.Models;
using TaskMaster.Utils;
using TaskMaster.Views.Controls;

namespace TaskMaster.Views.Forms;

public class FormTarefa : Form
{
    private readonly TarefaController _ctrl;
    private readonly Tarefa? _tarefaExistente;
    private bool _editando;

    // Campos
    private InputApp  _txtTitulo      = null!;
    private TextBox   _txtDescricao   = null!;
    private ComboBox  _cmbPrioridade  = null!;
    private ComboBox  _cmbCategoria   = null!;
    private ComboBox  _cmbRecorrencia = null!;
    private DateTimePicker _dtpPrazo  = null!;
    private CheckBox  _chkUsarPrazo   = null!;

    // Subtarefas
    private FlowLayoutPanel _painelSubs = null!;
    private InputApp        _txtNovoSub = null!;
    private List<Subtarefa> _subtarefas = new();

    public FormTarefa(TarefaController ctrl, Tarefa? tarefa = null)
    {
        _ctrl            = ctrl;
        _tarefaExistente = tarefa;
        _editando        = tarefa != null;
        if (tarefa != null)
            _subtarefas  = tarefa.Subtarefas.ToList();
        InicializarForm();
        ConstruirUI();
        if (_editando) PreencherCampos();
    }

    private void InicializarForm()
    {
        Text            = _editando ? "Editar Tarefa" : "Nova Tarefa";
        Size            = new Size(540, 620);
        MinimumSize     = new Size(480, 560);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppColors.Background;
        ForeColor       = AppColors.TextPrimary;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
    }

    private void ConstruirUI()
    {
        var painelPrincipal = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(24)
        };

        // Título do form
        var lblTitulo = new Label
        {
            Text      = _editando ? "✏️  Editar Tarefa" : "➕  Nova Tarefa",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 42
        };

        int y = 50;

        // Título da tarefa
        AdicionarLabel(painelPrincipal, "Título *", ref y);
        _txtTitulo = new InputApp { Location = new Point(0, y), Width = 470 };
        painelPrincipal.Controls.Add(_txtTitulo);
        y += 42;

        // Descrição
        AdicionarLabel(painelPrincipal, "Descrição", ref y);
        _txtDescricao = new TextBox
        {
            Location    = new Point(0, y),
            Width       = 470,
            Height      = 70,
            Multiline   = true,
            ScrollBars  = ScrollBars.Vertical,
            BackColor   = AppColors.SurfaceElevated,
            ForeColor   = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = AppFonts.Body
        };
        painelPrincipal.Controls.Add(_txtDescricao);
        y += 80;

        // Linha: Prioridade + Categoria
        AdicionarLabel(painelPrincipal, "Prioridade", ref y, 230);
        AdicionarLabel(painelPrincipal, "Categoria",  ref y, 0, 240);
        y -= 20; // volta para mesma linha

        _cmbPrioridade = CriarCombo(new Point(0, y), 220);
        _cmbPrioridade.Items.AddRange(new object[] { "Baixa", "Média", "Alta", "Crítica" });
        _cmbPrioridade.SelectedIndex = 1;

        _cmbCategoria = CriarCombo(new Point(240, y), 230);
        _cmbCategoria.Items.Add(new Categoria { Id = 1, Nome = "Carregando..." });
        CarregarCategorias();

        painelPrincipal.Controls.Add(_cmbPrioridade);
        painelPrincipal.Controls.Add(_cmbCategoria);
        y += 42;

        // Recorrência
        AdicionarLabel(painelPrincipal, "Recorrência", ref y);
        _cmbRecorrencia = CriarCombo(new Point(0, y), 220);
        _cmbRecorrencia.Items.AddRange(new object[] { "Sem recorrência", "Diária", "Semanal", "Mensal" });
        _cmbRecorrencia.SelectedIndex = 0;
        painelPrincipal.Controls.Add(_cmbRecorrencia);
        y += 42;

        // Prazo
        _chkUsarPrazo = new CheckBox
        {
            Text      = "Definir prazo",
            Location  = new Point(0, y),
            Font      = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true
        };
        _chkUsarPrazo.CheckedChanged += (_, _) => _dtpPrazo.Enabled = _chkUsarPrazo.Checked;
        painelPrincipal.Controls.Add(_chkUsarPrazo);

        _dtpPrazo = new DateTimePicker
        {
            Location   = new Point(130, y - 2),
            Width      = 200,
            Format     = DateTimePickerFormat.Short,
            MinDate    = DateTime.Today,
            Value      = DateTime.Today.AddDays(3),
            BackColor  = AppColors.SurfaceElevated,
            ForeColor  = AppColors.TextPrimary,
            CalendarForeColor  = AppColors.TextPrimary,
            CalendarMonthBackground = AppColors.Surface,
            Enabled    = false,
            Font       = AppFonts.Body
        };
        painelPrincipal.Controls.Add(_dtpPrazo);
        y += 36;

        // Subtarefas
        AdicionarLabel(painelPrincipal, "Subtarefas (checklist)", ref y);
        _painelSubs = new FlowLayoutPanel
        {
            Location      = new Point(0, y),
            Width         = 470,
            Height        = 80,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoScroll    = true,
            BackColor     = AppColors.SurfaceElevated,
            BorderStyle   = BorderStyle.FixedSingle
        };
        painelPrincipal.Controls.Add(_painelSubs);
        y += 88;

        // Adicionar subtarefa
        _txtNovoSub = new InputApp
        {
            Location        = new Point(0, y),
            Width           = 390,
            PlaceholderText = "Nova subtarefa..."
        };
        var btnAdSub = new BotaoApp
        {
            Text     = "+ Add",
            Width    = 74,
            Height   = 32,
            Location = new Point(396, y)
        };
        btnAdSub.Click += (_, _) => AdicionarSubtarefa();
        painelPrincipal.Controls.Add(_txtNovoSub);
        painelPrincipal.Controls.Add(btnAdSub);
        y += 42;

        RenderizarSubtarefas();

        // Botões de ação
        var btnCancelar = new BotaoApp
        {
            Text     = "Cancelar",
            Estilo   = BotaoApp.EstiloBotao.Secundario,
            Width    = 120,
            Height   = 38,
            Location = new Point(240, y + 8)
        };
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var btnSalvar = new BotaoApp
        {
            Text     = _editando ? "Salvar alterações" : "Criar tarefa",
            Width    = 160,
            Height   = 38,
            Location = new Point(370 - 50, y + 8)
        };
        btnSalvar.Click += (_, _) => Salvar();

        painelPrincipal.Controls.Add(btnCancelar);
        painelPrincipal.Controls.Add(btnSalvar);
        painelPrincipal.Controls.Add(lblTitulo);

        Controls.Add(painelPrincipal);
    }

    private void AdicionarLabel(Panel p, string texto, ref int y, int largura = 0, int offsetX = 0)
    {
        p.Controls.Add(new Label
        {
            Text      = texto,
            Font      = AppFonts.SmallBold,
            ForeColor = AppColors.TextSecondary,
            Location  = new Point(offsetX, y),
            AutoSize  = true
        });
        y += 18;
    }

    private static ComboBox CriarCombo(Point loc, int width) => new()
    {
        Location      = loc,
        Width         = width,
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor     = AppColors.SurfaceElevated,
        ForeColor     = AppColors.TextPrimary,
        FlatStyle     = FlatStyle.Flat,
        Font          = AppFonts.Body,
        Height        = 30
    };

    private void CarregarCategorias()
    {
        var repo = new Data.CategoriaRepository();
        _cmbCategoria.Items.Clear();
        foreach (var cat in repo.ObterTodas())
            _cmbCategoria.Items.Add(cat);
        _cmbCategoria.DisplayMember = "Nome";
        if (_cmbCategoria.Items.Count > 0)
            _cmbCategoria.SelectedIndex = 0;
    }

    private void PreencherCampos()
    {
        if (_tarefaExistente == null) return;
        _txtTitulo.Text         = _tarefaExistente.Titulo;
        _txtDescricao.Text      = _tarefaExistente.Descricao;
        _cmbPrioridade.SelectedIndex = (int)_tarefaExistente.Prioridade - 1;
        _cmbRecorrencia.SelectedIndex = _tarefaExistente.Recorrencia switch
        {
            RecorrenciaTarefa.Diaria  => 1,
            RecorrenciaTarefa.Semanal => 2,
            RecorrenciaTarefa.Mensal  => 3,
            _                         => 0
        };
        if (_tarefaExistente.DataPrazo.HasValue)
        {
            _chkUsarPrazo.Checked = true;
            _dtpPrazo.Value       = _tarefaExistente.DataPrazo.Value;
            _dtpPrazo.Enabled     = true;
        }
        // Selecionar categoria
        for (int i = 0; i < _cmbCategoria.Items.Count; i++)
        {
            if (_cmbCategoria.Items[i] is Categoria cat && cat.Id == _tarefaExistente.CategoriaId)
            {
                _cmbCategoria.SelectedIndex = i;
                break;
            }
        }
    }

    private void AdicionarSubtarefa()
    {
        if (string.IsNullOrWhiteSpace(_txtNovoSub.Text)) return;
        _subtarefas.Add(new Subtarefa { Titulo = _txtNovoSub.Text.Trim() });
        _txtNovoSub.Clear();
        RenderizarSubtarefas();
    }

    private void RenderizarSubtarefas()
    {
        _painelSubs.Controls.Clear();
        for (int i = 0; i < _subtarefas.Count; i++)
        {
            int idx  = i;
            var sub  = _subtarefas[i];
            var linha = new Panel { Width = 440, Height = 24, BackColor = Color.Transparent };
            var chk  = new CheckBox
            {
                Text      = sub.Titulo,
                Checked   = sub.Concluida,
                Font      = AppFonts.Small,
                ForeColor = AppColors.TextPrimary,
                AutoSize  = false,
                Width     = 380,
                Height    = 22,
                Location  = new Point(0, 1)
            };
            chk.CheckedChanged += (_, _) => _subtarefas[idx].Concluida = chk.Checked;

            var btnDel = new Label
            {
                Text      = "✕",
                Font      = AppFonts.Small,
                ForeColor = AppColors.Danger,
                AutoSize  = false,
                Width     = 20,
                Height    = 22,
                Location  = new Point(392, 1),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnDel.Click += (_, _) => { _subtarefas.RemoveAt(idx); RenderizarSubtarefas(); };

            linha.Controls.Add(chk);
            linha.Controls.Add(btnDel);
            _painelSubs.Controls.Add(linha);
        }
    }

    private void Salvar()
    {
        if (string.IsNullOrWhiteSpace(_txtTitulo.Text))
        {
            MessageBox.Show("O título é obrigatório.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtTitulo.Focus();
            return;
        }

        var cat = _cmbCategoria.SelectedItem as Categoria;

        var tarefa = _tarefaExistente ?? new Tarefa();
        tarefa.Titulo      = _txtTitulo.Text.Trim();
        tarefa.Descricao   = _txtDescricao.Text.Trim();
        tarefa.Prioridade  = (PrioridadeTarefa)(_cmbPrioridade.SelectedIndex + 1);
        tarefa.CategoriaId = cat?.Id ?? 1;
        tarefa.Recorrencia = _cmbRecorrencia.SelectedIndex switch
        {
            1 => RecorrenciaTarefa.Diaria,
            2 => RecorrenciaTarefa.Semanal,
            3 => RecorrenciaTarefa.Mensal,
            _ => RecorrenciaTarefa.Nenhuma
        };
        tarefa.DataPrazo   = _chkUsarPrazo.Checked ? _dtpPrazo.Value.Date : null;
        tarefa.Subtarefas  = _subtarefas;

        if (_editando) _ctrl.AtualizarTarefa(tarefa);
        else           _ctrl.CriarTarefa(tarefa);

        DialogResult = DialogResult.OK;
        Close();
    }
}
