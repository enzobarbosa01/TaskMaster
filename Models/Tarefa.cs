namespace TaskMaster.Models;

public enum PrioridadeTarefa
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Critica = 4
}

public enum StatusTarefa
{
    Pendente = 0,
    EmAndamento = 1,
    Concluida = 2,
    Cancelada = 3
}

public enum RecorrenciaTarefa
{
    Nenhuma = 0,
    Diaria = 1,
    Semanal = 7,
    Mensal = 30
}

public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataPrazo { get; set; }
    public DateTime? DataConclusao { get; set; }
    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;
    public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;
    public int CategoriaId { get; set; }
    public RecorrenciaTarefa Recorrencia { get; set; } = RecorrenciaTarefa.Nenhuma;
    public int XpRecompensa { get; set; } = 10;

    // Propriedades de navegação (não persistidas)
    public Categoria? Categoria { get; set; }
    public List<Subtarefa> Subtarefas { get; set; } = new();

    // ─── Algoritmo de Score de Priorização ───────────────────────────────────
    // Fórmula: Score = (Importância × 0.6) + (Urgência × 0.4)
    // Importância = valor da enum Prioridade normalizado (0.25 a 1.0)
    // Urgência = 1 / (dias_restantes + 1), limitado entre 0 e 1
    // Referência: Allen (GTD) e Pressman (Engenharia de Software)
    public double CalcularScore()
    {
        double importancia = (double)Prioridade / 4.0; // normaliza 1-4 para 0.25-1.0

        double urgencia = 0.0;
        if (DataPrazo.HasValue)
        {
            double diasRestantes = (DataPrazo.Value - DateTime.Now).TotalDays;
            if (diasRestantes < 0) urgencia = 1.0; // vencida = urgência máxima
            else urgencia = 1.0 / (diasRestantes + 1.0);
            urgencia = Math.Min(urgencia, 1.0);
        }

        return (importancia * 0.6) + (urgencia * 0.4);
    }

    public bool EstaPrazoProximo()
    {
        if (!DataPrazo.HasValue) return false;
        return (DataPrazo.Value - DateTime.Now).TotalHours <= 24;
    }

    public bool EstaVencida()
    {
        if (!DataPrazo.HasValue || Status == StatusTarefa.Concluida) return false;
        return DataPrazo.Value < DateTime.Now;
    }

    public int CalcularXpRecompensa()
    {
        int base_xp = Prioridade switch
        {
            PrioridadeTarefa.Baixa    => 10,
            PrioridadeTarefa.Media    => 20,
            PrioridadeTarefa.Alta     => 35,
            PrioridadeTarefa.Critica  => 50,
            _                         => 10
        };

        // Bônus por conclusão antes do prazo
        if (DataPrazo.HasValue && DateTime.Now < DataPrazo.Value)
            base_xp += 10;

        return base_xp;
    }
}
