using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Controllers;

public class TarefaController
{
    private readonly TarefaRepository _repo = new();
    private readonly GamificacaoController _gamificacao;
    private readonly LogRepository _log = new();

    public TarefaController(GamificacaoController gamificacao)
    {
        _gamificacao = gamificacao;
    }

    public List<Tarefa> ObterTarefas(
        int? categoriaId = null,
        StatusTarefa? status = null,
        string? filtroTexto = null,
        bool ordenarPorScore = true)
    {
        var tarefas = _repo.ObterTodas(categoriaId, status, filtroTexto);
        if (ordenarPorScore)
            tarefas = tarefas
                .OrderByDescending(t => t.CalcularScore())
                .ThenBy(t => t.DataPrazo ?? DateTime.MaxValue)
                .ToList();
        return tarefas;
    }

    public Tarefa? ObterPorId(int id) => _repo.ObterPorId(id);

    public List<Tarefa> ObterTarefasComAlertaPrazo()
    {
        return _repo.ObterTodas(status: StatusTarefa.Pendente)
            .Concat(_repo.ObterTodas(status: StatusTarefa.EmAndamento))
            .Where(t => t.EstaPrazoProximo() || t.EstaVencida())
            .ToList();
    }

    public Tarefa CriarTarefa(Tarefa tarefa)
    {
        tarefa.DataCriacao = DateTime.Now;
        tarefa.Status      = StatusTarefa.Pendente;
        tarefa.Id          = _repo.Inserir(tarefa);
        _log.Registrar(new LogAtividade
        {
            Tipo      = TipoLog.TarefaCriada,
            Descricao = $"Tarefa criada: {tarefa.Titulo}",
            TarefaId  = tarefa.Id
        });
        return tarefa;
    }

    public void AtualizarTarefa(Tarefa tarefa)
    {
        _repo.Atualizar(tarefa);
        _log.Registrar(new LogAtividade
        {
            Tipo      = TipoLog.TarefaEditada,
            Descricao = $"Tarefa editada: {tarefa.Titulo}",
            TarefaId  = tarefa.Id
        });
    }

    public void ExcluirTarefa(int id)
    {
        var t = _repo.ObterPorId(id);
        _repo.Excluir(id);
        if (t != null)
            _log.Registrar(new LogAtividade
            {
                Tipo      = TipoLog.TarefaCancelada,
                Descricao = $"Tarefa excluida: {t.Titulo}",
                TarefaId  = id
            });
    }

    public (int xpGanho, List<Conquista> conquistasDesbloqueadas) ConcluirTarefa(int id)
    {
        var tarefa = _repo.ObterPorId(id);
        if (tarefa == null) return (0, new List<Conquista>());

        tarefa.Status        = StatusTarefa.Concluida;
        tarefa.DataConclusao = DateTime.Now;
        _repo.AtualizarStatus(id, StatusTarefa.Concluida, DateTime.Now);

        int xp = tarefa.CalcularXpRecompensa();
        var conquistas = _gamificacao.ProcessarConclusao(xp);

        _log.Registrar(new LogAtividade
        {
            Tipo      = TipoLog.TarefaConcluida,
            Descricao = $"Tarefa concluida: {tarefa.Titulo} (+{xp} XP)",
            TarefaId  = id,
            XpGanho   = xp
        });

        if (tarefa.Recorrencia != RecorrenciaTarefa.Nenhuma)
            CriarProximaRecorrencia(tarefa);

        return (xp, conquistas);
    }

    private void CriarProximaRecorrencia(Tarefa original)
    {
        var proxima = new Tarefa
        {
            Titulo      = original.Titulo,
            Descricao   = original.Descricao,
            Prioridade  = original.Prioridade,
            CategoriaId = original.CategoriaId,
            Recorrencia = original.Recorrencia,
            DataPrazo   = original.DataPrazo.HasValue
                ? original.DataPrazo.Value.AddDays((int)original.Recorrencia)
                : null
        };
        foreach (var sub in original.Subtarefas)
            proxima.Subtarefas.Add(new Subtarefa { Titulo = sub.Titulo });
        _repo.Inserir(proxima);
    }

    public void AtualizarSubtarefa(int subtarefaId, bool concluida)
    {
        _repo.AtualizarSubtarefa(subtarefaId, concluida);
    }

    public int ContarConcluidasHoje() => _repo.ContarConcluidasHoje();
}
