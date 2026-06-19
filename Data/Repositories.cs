using Microsoft.Data.Sqlite;
using TaskMaster.Models;

namespace TaskMaster.Data;

// ─── CategoriaRepository ──────────────────────────────────────────────────────
public class CategoriaRepository
{
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public List<Categoria> ObterTodas()
    {
        var lista = new List<Categoria>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Categorias ORDER BY Nome";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Categoria
            {
                Id    = r.GetInt32(0),
                Nome  = r.GetString(1),
                Cor   = r.GetString(2),
                Icone = r.GetString(3)
            });
        return lista;
    }

    public int Inserir(Categoria c)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Categorias(Nome,Cor,Icone) VALUES($n,$c,$i); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$n", c.Nome);
        cmd.Parameters.AddWithValue("$c", c.Cor);
        cmd.Parameters.AddWithValue("$i", c.Icone);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Excluir(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Categorias WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}

// ─── UsuarioRepository ────────────────────────────────────────────────────────
public class UsuarioRepository
{
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public Usuario ObterUsuario()
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Usuario WHERE Id=1";
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return new Usuario();
        return new Usuario
        {
            Id                    = 1,
            Nome                  = r.GetString(r.GetOrdinal("Nome")),
            XpAtual               = r.GetInt32(r.GetOrdinal("XpAtual")),
            Nivel                 = r.GetInt32(r.GetOrdinal("Nivel")),
            StreakAtual           = r.GetInt32(r.GetOrdinal("StreakAtual")),
            MaiorStreak           = r.GetInt32(r.GetOrdinal("MaiorStreak")),
            TarefasConcluidasTotal= r.GetInt32(r.GetOrdinal("TarefasConcluidasTotal")),
            MetaDiaria            = r.GetInt32(r.GetOrdinal("MetaDiaria")),
            UltimoAcessoAtivo     = r.IsDBNull(r.GetOrdinal("UltimoAcessoAtivo"))
                                    ? null
                                    : DateTime.Parse(r.GetString(r.GetOrdinal("UltimoAcessoAtivo")))
        };
    }

    public void Salvar(Usuario u)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Usuario SET
            Nome=$nome, XpAtual=$xp, Nivel=$nivel,
            StreakAtual=$streak, MaiorStreak=$maior,
            UltimoAcessoAtivo=$acesso,
            TarefasConcluidasTotal=$total,
            MetaDiaria=$meta
            WHERE Id=1";
        cmd.Parameters.AddWithValue("$nome",   u.Nome);
        cmd.Parameters.AddWithValue("$xp",     u.XpAtual);
        cmd.Parameters.AddWithValue("$nivel",  u.Nivel);
        cmd.Parameters.AddWithValue("$streak", u.StreakAtual);
        cmd.Parameters.AddWithValue("$maior",  u.MaiorStreak);
        cmd.Parameters.AddWithValue("$acesso", u.UltimoAcessoAtivo.HasValue
                                               ? u.UltimoAcessoAtivo.Value.ToString("o")
                                               : DBNull.Value);
        cmd.Parameters.AddWithValue("$total",  u.TarefasConcluidasTotal);
        cmd.Parameters.AddWithValue("$meta",   u.MetaDiaria);
        cmd.ExecuteNonQuery();
    }
}

// ─── ConquistaRepository ──────────────────────────────────────────────────────
public class ConquistaRepository
{
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public List<Conquista> ObterTodas()
    {
        var lista = new List<Conquista>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Conquistas ORDER BY Tipo, MetaValor";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(LerConquista(r));
        return lista;
    }

    public void MarcarDesbloqueada(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Conquistas SET Desbloqueada=1, DataDesbloqueio=$d WHERE Id=$id";
        cmd.Parameters.AddWithValue("$d",  DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static Conquista LerConquista(SqliteDataReader r) => new()
    {
        Id              = r.GetInt32(r.GetOrdinal("Id")),
        Nome            = r.GetString(r.GetOrdinal("Nome")),
        Descricao       = r.GetString(r.GetOrdinal("Descricao")),
        Icone           = r.GetString(r.GetOrdinal("Icone")),
        Tipo            = (TipoConquista)r.GetInt32(r.GetOrdinal("Tipo")),
        MetaValor       = r.GetInt32(r.GetOrdinal("MetaValor")),
        Desbloqueada    = r.GetInt32(r.GetOrdinal("Desbloqueada")) == 1,
        DataDesbloqueio = r.IsDBNull(r.GetOrdinal("DataDesbloqueio"))
                          ? null
                          : DateTime.Parse(r.GetString(r.GetOrdinal("DataDesbloqueio"))),
        XpBonus         = r.GetInt32(r.GetOrdinal("XpBonus"))
    };
}

// ─── LogRepository ────────────────────────────────────────────────────────────
public class LogRepository
{
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public void Registrar(LogAtividade log)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO LogAtividades(DataHora,Tipo,Descricao,TarefaId,XpGanho)
                            VALUES($d,$t,$desc,$tid,$xp)";
        cmd.Parameters.AddWithValue("$d",    log.DataHora.ToString("o"));
        cmd.Parameters.AddWithValue("$t",    (int)log.Tipo);
        cmd.Parameters.AddWithValue("$desc", log.Descricao);
        cmd.Parameters.AddWithValue("$tid",  log.TarefaId.HasValue ? log.TarefaId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("$xp",   log.XpGanho);
        cmd.ExecuteNonQuery();
    }

    public List<LogAtividade> ObterRecentes(int limite = 50)
    {
        var lista = new List<LogAtividade>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM LogAtividades ORDER BY DataHora DESC LIMIT $l";
        cmd.Parameters.AddWithValue("$l", limite);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new LogAtividade
            {
                Id        = r.GetInt32(r.GetOrdinal("Id")),
                DataHora  = DateTime.Parse(r.GetString(r.GetOrdinal("DataHora"))),
                Tipo      = (TipoLog)r.GetInt32(r.GetOrdinal("Tipo")),
                Descricao = r.GetString(r.GetOrdinal("Descricao")),
                TarefaId  = r.IsDBNull(r.GetOrdinal("TarefaId")) ? null : r.GetInt32(r.GetOrdinal("TarefaId")),
                XpGanho   = r.GetInt32(r.GetOrdinal("XpGanho"))
            });
        return lista;
    }
}
