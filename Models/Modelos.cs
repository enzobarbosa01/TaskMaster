namespace TaskMaster.Models;

// ─── Categoria ────────────────────────────────────────────────────────────────
public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cor { get; set; } = "#4A90D9"; // hex color
    public string Icone { get; set; } = "📁";
}

// ─── Subtarefa ────────────────────────────────────────────────────────────────
public class Subtarefa
{
    public int Id { get; set; }
    public int TarefaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Concluida { get; set; } = false;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

// ─── Usuário / Perfil de Gamificação ─────────────────────────────────────────
public class Usuario
{
    public int Id { get; set; } = 1; // sistema single-user
    public string Nome { get; set; } = "Jogador";
    public int XpAtual { get; set; } = 0;
    public int Nivel { get; set; } = 1;
    public int StreakAtual { get; set; } = 0;
    public int MaiorStreak { get; set; } = 0;
    public DateTime? UltimoAcessoAtivo { get; set; }
    public int TarefasConcluidasTotal { get; set; } = 0;
    public int MetaDiaria { get; set; } = 5;

    // XP necessário para o próximo nível: cresce 100 por nível
    public int XpParaProximoNivel => Nivel * 100;

    // Progresso percentual no nível atual
    public double ProgressoNivel
    {
        get
        {
            int xpNivelAnterior = (Nivel - 1) * 100;
            int xpNivelAtual = Nivel * 100;
            double progresso = (double)(XpAtual - xpNivelAnterior) / (xpNivelAtual - xpNivelAnterior);
            return Math.Max(0, Math.Min(1, progresso));
        }
    }

    public string TituloNivel => Nivel switch
    {
        1  => "Iniciante",
        2  => "Aprendiz",
        3  => "Dedicado",
        4  => "Focado",
        5  => "Eficiente",
        6  => "Organizado",
        7  => "Produtivo",
        8  => "Expert",
        9  => "Mestre",
        10 => "Lendário",
        _  => $"Nível {Nivel}"
    };

    public void AdicionarXp(int xp)
    {
        XpAtual += xp;
        // Verifica subida de nível
        while (XpAtual >= Nivel * 100)
            Nivel++;
    }
}

// ─── Conquista ────────────────────────────────────────────────────────────────
public enum TipoConquista
{
    TarefasConcluidas,
    Streak,
    Nivel,
    Especial
}

public class Conquista
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = "🏆";
    public TipoConquista Tipo { get; set; }
    public int MetaValor { get; set; } // ex: 10 tarefas, streak de 7 dias
    public bool Desbloqueada { get; set; } = false;
    public DateTime? DataDesbloqueio { get; set; }
    public int XpBonus { get; set; } = 50;
}

// ─── Log de Atividade ─────────────────────────────────────────────────────────
public enum TipoLog
{
    TarefaCriada,
    TarefaConcluida,
    TarefaEditada,
    TarefaCancelada,
    ConquistaDesbloqueada,
    NivelSubiu,
    StreakAtualizado
}

public class LogAtividade
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public TipoLog Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int? TarefaId { get; set; }
    public int XpGanho { get; set; } = 0;
}
