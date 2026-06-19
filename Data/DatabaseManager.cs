using Microsoft.Data.Sqlite;

namespace TaskMaster.Data;

public class DatabaseManager
{
    private static DatabaseManager? _instance;
    public static DatabaseManager Instance => _instance ??= new DatabaseManager();

    private readonly string _dbPath;
    public string ConnectionString { get; }

    private DatabaseManager()
    {
        string appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TaskMaster");
        Directory.CreateDirectory(appData);
        _dbPath = Path.Combine(appData, "taskmaster.db");
        ConnectionString = $"Data Source={_dbPath}";
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public void Initialize()
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = GetSchemaSql();
        cmd.ExecuteNonQuery();
        SeedDadosIniciais(conn);
    }

    private static string GetSchemaSql() => @"
        PRAGMA journal_mode=WAL;
        PRAGMA foreign_keys=ON;

        CREATE TABLE IF NOT EXISTS Categorias (
            Id    INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome  TEXT NOT NULL,
            Cor   TEXT NOT NULL DEFAULT '#4A90D9',
            Icone TEXT NOT NULL DEFAULT '📁'
        );

        CREATE TABLE IF NOT EXISTS Tarefas (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            Titulo        TEXT NOT NULL,
            Descricao     TEXT,
            DataCriacao   TEXT NOT NULL,
            DataPrazo     TEXT,
            DataConclusao TEXT,
            Prioridade    INTEGER NOT NULL DEFAULT 2,
            Status        INTEGER NOT NULL DEFAULT 0,
            CategoriaId   INTEGER NOT NULL DEFAULT 1,
            Recorrencia   INTEGER NOT NULL DEFAULT 0,
            XpRecompensa  INTEGER NOT NULL DEFAULT 10,
            FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
        );

        CREATE TABLE IF NOT EXISTS Subtarefas (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            TarefaId    INTEGER NOT NULL,
            Titulo      TEXT NOT NULL,
            Concluida   INTEGER NOT NULL DEFAULT 0,
            DataCriacao TEXT NOT NULL,
            FOREIGN KEY (TarefaId) REFERENCES Tarefas(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Usuario (
            Id                     INTEGER PRIMARY KEY DEFAULT 1,
            Nome                   TEXT NOT NULL DEFAULT 'Jogador',
            XpAtual                INTEGER NOT NULL DEFAULT 0,
            Nivel                  INTEGER NOT NULL DEFAULT 1,
            StreakAtual            INTEGER NOT NULL DEFAULT 0,
            MaiorStreak            INTEGER NOT NULL DEFAULT 0,
            UltimoAcessoAtivo      TEXT,
            TarefasConcluidasTotal INTEGER NOT NULL DEFAULT 0,
            MetaDiaria             INTEGER NOT NULL DEFAULT 5
        );

        CREATE TABLE IF NOT EXISTS Conquistas (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome            TEXT NOT NULL,
            Descricao       TEXT NOT NULL,
            Icone           TEXT NOT NULL DEFAULT '🏆',
            Tipo            INTEGER NOT NULL,
            MetaValor       INTEGER NOT NULL,
            Desbloqueada    INTEGER NOT NULL DEFAULT 0,
            DataDesbloqueio TEXT,
            XpBonus         INTEGER NOT NULL DEFAULT 50
        );

        CREATE TABLE IF NOT EXISTS LogAtividades (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            DataHora  TEXT NOT NULL,
            Tipo      INTEGER NOT NULL,
            Descricao TEXT NOT NULL,
            TarefaId  INTEGER,
            XpGanho   INTEGER NOT NULL DEFAULT 0
        );
    ";

    private static void SeedDadosIniciais(SqliteConnection conn)
    {
        var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = "SELECT COUNT(*) FROM Categorias";
        long count = (long)(cmdCheck.ExecuteScalar() ?? 0L);
        if (count == 0)
        {
            var categorias = new[]
            {
                ("Geral",      "#607D8B", "📋"),
                ("Trabalho",   "#1976D2", "💼"),
                ("Estudo",     "#388E3C", "📚"),
                ("Pessoal",    "#7B1FA2", "🏠"),
                ("Saúde",      "#D32F2F", "❤️"),
                ("Financeiro", "#F57C00", "💰"),
            };
            foreach (var (nome, cor, icone) in categorias)
            {
                var c = conn.CreateCommand();
                c.CommandText = "INSERT INTO Categorias (Nome, Cor, Icone) VALUES ($n,$c,$i)";
                c.Parameters.AddWithValue("$n", nome);
                c.Parameters.AddWithValue("$c", cor);
                c.Parameters.AddWithValue("$i", icone);
                c.ExecuteNonQuery();
            }
        }

        var cmdU = conn.CreateCommand();
        cmdU.CommandText = "INSERT OR IGNORE INTO Usuario (Id, Nome) VALUES (1, 'Jogador')";
        cmdU.ExecuteNonQuery();

        var cmdCq = conn.CreateCommand();
        cmdCq.CommandText = "SELECT COUNT(*) FROM Conquistas";
        long cq = (long)(cmdCq.ExecuteScalar() ?? 0L);
        if (cq == 0)
        {
            var conquistas = new[]
            {
                ("Primeira Tarefa",  "Conclua sua primeira tarefa",    "⭐", 0, 1,   25),
                ("Em Ritmo",         "Conclua 10 tarefas",             "🔥", 0, 10,  50),
                ("Produtivo",        "Conclua 50 tarefas",             "💪", 0, 50,  100),
                ("Centenário",       "Conclua 100 tarefas",            "🏅", 0, 100, 200),
                ("3 Dias Seguidos",  "Mantenha streak por 3 dias",     "📅", 1, 3,   50),
                ("Semana Perfeita",  "Mantenha streak por 7 dias",     "🗓️", 1, 7,  100),
                ("Mês Dedicado",     "Mantenha streak por 30 dias",    "🌟", 1, 30,  300),
                ("Sobe de Nível",    "Alcance o nível 5",              "⬆️", 2, 5,   75),
                ("Expert",           "Alcance o nível 10",             "🎯", 2, 10,  150),
                ("Sem Desculpas",    "Conclua uma tarefa vencida",     "⚡", 3, 1,   30),
            };
            foreach (var (nome, desc, icone, tipo, meta, xp) in conquistas)
            {
                var c = conn.CreateCommand();
                c.CommandText = @"INSERT INTO Conquistas 
                    (Nome,Descricao,Icone,Tipo,MetaValor,XpBonus) 
                    VALUES ($n,$d,$i,$t,$m,$x)";
                c.Parameters.AddWithValue("$n", nome);
                c.Parameters.AddWithValue("$d", desc);
                c.Parameters.AddWithValue("$i", icone);
                c.Parameters.AddWithValue("$t", tipo);
                c.Parameters.AddWithValue("$m", meta);
                c.Parameters.AddWithValue("$x", xp);
                c.ExecuteNonQuery();
            }
        }
    }
}
