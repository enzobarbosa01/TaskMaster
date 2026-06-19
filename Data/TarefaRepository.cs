using Microsoft.Data.Sqlite;
using TaskMaster.Models;

namespace TaskMaster.Data;

public class TarefaRepository
{
    private readonly DatabaseManager _db = DatabaseManager.Instance;

    public List<Tarefa> ObterTodas(int? categoriaId = null, StatusTarefa? status = null, string? filtroTexto = null)
    {
        var tarefas = new List<Tarefa>();
        using var conn = _db.GetConnection();
        conn.Open();

        var sql = @"SELECT t.*, c.Nome as CatNome, c.Cor as CatCor, c.Icone as CatIcone
                    FROM Tarefas t
                    LEFT JOIN Categorias c ON t.CategoriaId = c.Id
                    WHERE 1=1";

        if (categoriaId.HasValue) sql += " AND t.CategoriaId = $catId";
        if (status.HasValue)     sql += " AND t.Status = $status";
        if (!string.IsNullOrWhiteSpace(filtroTexto)) sql += " AND (t.Titulo LIKE $filtro OR t.Descricao LIKE $filtro)";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (categoriaId.HasValue) cmd.Parameters.AddWithValue("$catId", categoriaId.Value);
        if (status.HasValue)     cmd.Parameters.AddWithValue("$status", (int)status.Value);
        if (!string.IsNullOrWhiteSpace(filtroTexto))
            cmd.Parameters.AddWithValue("$filtro", $"%{filtroTexto}%");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var t = LerTarefa(reader);
            tarefas.Add(t);
        }

        foreach (var t in tarefas)
            t.Subtarefas = ObterSubtarefas(conn, t.Id);

        return tarefas;
    }

    public Tarefa? ObterPorId(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT t.*, c.Nome as CatNome, c.Cor as CatCor, c.Icone as CatIcone
                            FROM Tarefas t
                            LEFT JOIN Categorias c ON t.CategoriaId = c.Id
                            WHERE t.Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var tarefa = LerTarefa(reader);
        tarefa.Subtarefas = ObterSubtarefas(conn, tarefa.Id);
        return tarefa;
    }

    public int Inserir(Tarefa t)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Tarefas 
            (Titulo,Descricao,DataCriacao,DataPrazo,Prioridade,Status,CategoriaId,Recorrencia,XpRecompensa)
            VALUES ($titulo,$desc,$criacao,$prazo,$prio,$status,$cat,$rec,$xp);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$titulo",  t.Titulo);
        cmd.Parameters.AddWithValue("$desc",    t.Descricao);
        cmd.Parameters.AddWithValue("$criacao", t.DataCriacao.ToString("o"));
        cmd.Parameters.AddWithValue("$prazo",   t.DataPrazo.HasValue ? t.DataPrazo.Value.ToString("o") : DBNull.Value);
        cmd.Parameters.AddWithValue("$prio",    (int)t.Prioridade);
        cmd.Parameters.AddWithValue("$status",  (int)t.Status);
        cmd.Parameters.AddWithValue("$cat",     t.CategoriaId);
        cmd.Parameters.AddWithValue("$rec",     (int)t.Recorrencia);
        cmd.Parameters.AddWithValue("$xp",      t.CalcularXpRecompensa());
        t.Id = Convert.ToInt32(cmd.ExecuteScalar());

        foreach (var sub in t.Subtarefas)
        {
            sub.TarefaId = t.Id;
            InserirSubtarefa(conn, sub);
        }
        return t.Id;
    }

    public void Atualizar(Tarefa t)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Tarefas SET
            Titulo=$titulo, Descricao=$desc, DataPrazo=$prazo,
            DataConclusao=$conclusao, Prioridade=$prio, Status=$status,
            CategoriaId=$cat, Recorrencia=$rec, XpRecompensa=$xp
            WHERE Id=$id";
        cmd.Parameters.AddWithValue("$titulo",    t.Titulo);
        cmd.Parameters.AddWithValue("$desc",      t.Descricao);
        cmd.Parameters.AddWithValue("$prazo",     t.DataPrazo.HasValue ? t.DataPrazo.Value.ToString("o") : DBNull.Value);
        cmd.Parameters.AddWithValue("$conclusao", t.DataConclusao.HasValue ? t.DataConclusao.Value.ToString("o") : DBNull.Value);
        cmd.Parameters.AddWithValue("$prio",      (int)t.Prioridade);
        cmd.Parameters.AddWithValue("$status",    (int)t.Status);
        cmd.Parameters.AddWithValue("$cat",       t.CategoriaId);
        cmd.Parameters.AddWithValue("$rec",       (int)t.Recorrencia);
        cmd.Parameters.AddWithValue("$xp",        t.CalcularXpRecompensa());
        cmd.Parameters.AddWithValue("$id",        t.Id);
        cmd.ExecuteNonQuery();

        // Atualiza subtarefas: remove as antigas e reinsere
        var del = conn.CreateCommand();
        del.CommandText = "DELETE FROM Subtarefas WHERE TarefaId=$id";
        del.Parameters.AddWithValue("$id", t.Id);
        del.ExecuteNonQuery();
        foreach (var sub in t.Subtarefas)
        {
            sub.TarefaId = t.Id;
            InserirSubtarefa(conn, sub);
        }
    }

    public void Excluir(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Tarefas WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void AtualizarStatus(int id, StatusTarefa status, DateTime? dataConclusao = null)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Tarefas SET Status=$s, DataConclusao=$d WHERE Id=$id";
        cmd.Parameters.AddWithValue("$s",   (int)status);
        cmd.Parameters.AddWithValue("$d",   dataConclusao.HasValue ? dataConclusao.Value.ToString("o") : DBNull.Value);
        cmd.Parameters.AddWithValue("$id",  id);
        cmd.ExecuteNonQuery();
    }

    public int ContarConcluidasHoje()
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT COUNT(*) FROM Tarefas 
                            WHERE Status=2 AND date(DataConclusao)=date('now','localtime')";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // ─── Subtarefas ──────────────────────────────────────────────────────────
    private static List<Subtarefa> ObterSubtarefas(SqliteConnection conn, int tarefaId)
    {
        var lista = new List<Subtarefa>();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Subtarefas WHERE TarefaId=$id ORDER BY Id";
        cmd.Parameters.AddWithValue("$id", tarefaId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            lista.Add(new Subtarefa
            {
                Id          = r.GetInt32(r.GetOrdinal("Id")),
                TarefaId    = r.GetInt32(r.GetOrdinal("TarefaId")),
                Titulo      = r.GetString(r.GetOrdinal("Titulo")),
                Concluida   = r.GetInt32(r.GetOrdinal("Concluida")) == 1,
                DataCriacao = DateTime.Parse(r.GetString(r.GetOrdinal("DataCriacao")))
            });
        }
        return lista;
    }

    private static void InserirSubtarefa(SqliteConnection conn, Subtarefa sub)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Subtarefas (TarefaId,Titulo,Concluida,DataCriacao) VALUES ($t,$titulo,$c,$d)";
        cmd.Parameters.AddWithValue("$t",     sub.TarefaId);
        cmd.Parameters.AddWithValue("$titulo",sub.Titulo);
        cmd.Parameters.AddWithValue("$c",     sub.Concluida ? 1 : 0);
        cmd.Parameters.AddWithValue("$d",     sub.DataCriacao.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public void AtualizarSubtarefa(int id, bool concluida)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Subtarefas SET Concluida=$c WHERE Id=$id";
        cmd.Parameters.AddWithValue("$c",  concluida ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    // ─── Helper ──────────────────────────────────────────────────────────────
    private static Tarefa LerTarefa(SqliteDataReader r)
    {
        var t = new Tarefa
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            Titulo      = r.GetString(r.GetOrdinal("Titulo")),
            Descricao   = r.IsDBNull(r.GetOrdinal("Descricao")) ? "" : r.GetString(r.GetOrdinal("Descricao")),
            DataCriacao = DateTime.Parse(r.GetString(r.GetOrdinal("DataCriacao"))),
            Prioridade  = (PrioridadeTarefa)r.GetInt32(r.GetOrdinal("Prioridade")),
            Status      = (StatusTarefa)r.GetInt32(r.GetOrdinal("Status")),
            CategoriaId = r.GetInt32(r.GetOrdinal("CategoriaId")),
            Recorrencia = (RecorrenciaTarefa)r.GetInt32(r.GetOrdinal("Recorrencia")),
            XpRecompensa= r.GetInt32(r.GetOrdinal("XpRecompensa")),
        };
        if (!r.IsDBNull(r.GetOrdinal("DataPrazo")))
            t.DataPrazo = DateTime.Parse(r.GetString(r.GetOrdinal("DataPrazo")));
        if (!r.IsDBNull(r.GetOrdinal("DataConclusao")))
            t.DataConclusao = DateTime.Parse(r.GetString(r.GetOrdinal("DataConclusao")));

        // Categoria inline
        try
        {
            t.Categoria = new Categoria
            {
                Id    = t.CategoriaId,
                Nome  = r.IsDBNull(r.GetOrdinal("CatNome"))  ? "Geral" : r.GetString(r.GetOrdinal("CatNome")),
                Cor   = r.IsDBNull(r.GetOrdinal("CatCor"))   ? "#607D8B" : r.GetString(r.GetOrdinal("CatCor")),
                Icone = r.IsDBNull(r.GetOrdinal("CatIcone")) ? "📋" : r.GetString(r.GetOrdinal("CatIcone")),
            };
        }
        catch { }
        return t;
    }
}
