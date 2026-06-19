using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Controllers;

public class GamificacaoController
{
    private readonly UsuarioRepository _usuarioRepo = new();
    private readonly ConquistaRepository _conquistaRepo = new();
    private readonly LogRepository _log = new();

    public Usuario ObterUsuario() => _usuarioRepo.ObterUsuario();

    public List<Conquista> ProcessarConclusao(int xpGanho)
    {
        var usuario = _usuarioRepo.ObterUsuario();
        int nivelAnterior = usuario.Nivel;

        usuario.TarefasConcluidasTotal++;
        usuario.AdicionarXp(xpGanho);
        AtualizarStreak(usuario);
        _usuarioRepo.Salvar(usuario);

        var novasConquistas = VerificarConquistas(usuario);

        if (usuario.Nivel > nivelAnterior)
            _log.Registrar(new LogAtividade
            {
                Tipo      = TipoLog.NivelSubiu,
                Descricao = $"Subiu para nivel {usuario.Nivel}: {usuario.TituloNivel}!",
                XpGanho   = 0
            });

        return novasConquistas;
    }

    private static void AtualizarStreak(Usuario usuario)
    {
        var hoje = DateTime.Today;
        if (usuario.UltimoAcessoAtivo.HasValue)
        {
            var ultimoDia = usuario.UltimoAcessoAtivo.Value.Date;
            if (ultimoDia == hoje)       return;
            if (ultimoDia == hoje.AddDays(-1)) usuario.StreakAtual++;
            else                         usuario.StreakAtual = 1;
        }
        else
        {
            usuario.StreakAtual = 1;
        }

        if (usuario.StreakAtual > usuario.MaiorStreak)
            usuario.MaiorStreak = usuario.StreakAtual;

        usuario.UltimoAcessoAtivo = DateTime.Now;
    }

    private List<Conquista> VerificarConquistas(Usuario usuario)
    {
        var todas = _conquistaRepo.ObterTodas();
        var novas = new List<Conquista>();

        foreach (var c in todas.Where(x => !x.Desbloqueada))
        {
            bool desbloqueada = c.Tipo switch
            {
                TipoConquista.TarefasConcluidas => usuario.TarefasConcluidasTotal >= c.MetaValor,
                TipoConquista.Streak            => usuario.StreakAtual >= c.MetaValor,
                TipoConquista.Nivel             => usuario.Nivel >= c.MetaValor,
                _                               => false
            };

            if (desbloqueada)
            {
                _conquistaRepo.MarcarDesbloqueada(c.Id);
                usuario.AdicionarXp(c.XpBonus);
                novas.Add(c);
                _log.Registrar(new LogAtividade
                {
                    Tipo      = TipoLog.ConquistaDesbloqueada,
                    Descricao = $"Conquista desbloqueada: {c.Nome}",
                    XpGanho   = c.XpBonus
                });
            }
        }

        if (novas.Count > 0) _usuarioRepo.Salvar(usuario);
        return novas;
    }

    public List<Conquista> ObterConquistas() => _conquistaRepo.ObterTodas();

    public void AtualizarMetaDiaria(int meta)
    {
        var u = _usuarioRepo.ObterUsuario();
        u.MetaDiaria = meta;
        _usuarioRepo.Salvar(u);
    }

    public void AtualizarNomeUsuario(string nome)
    {
        var u = _usuarioRepo.ObterUsuario();
        u.Nome = nome;
        _usuarioRepo.Salvar(u);
    }
}
