using System.Text.Json;
using SharpReady.Models;
using SharpReady.Models.Enums;
using SQLite;

namespace SharpReady.Services;

// ── SQLite record DTOs ────────────────────────────────────────────────────────

[Table("topics")]
public class TopicRecord
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Category { get; set; }
    public string Description { get; set; } = string.Empty;

    public Topic ToTopic() => new()
    {
        Id = Id,
        Name = Name,
        Category = (CSharpCategory)Category,
        Description = Description
    };

    public static TopicRecord From(Topic t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Category = (int)t.Category,
        Description = t.Description
    };
}

[Table("questions")]
public class QuestionRecord
{
    [PrimaryKey]
    public int Id { get; set; }
    public int TopicId { get; set; }
    public int Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string OptionsJson { get; set; } = "[]";
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? ExampleCode { get; set; }
    public int Difficulty { get; set; }

    public Question ToQuestion() => new()
    {
        Id = Id,
        TopicId = TopicId,
        Type = (QuestionType)Type,
        Text = Text,
        CodeSnippet = CodeSnippet,
        Options = JsonSerializer.Deserialize<string[]>(OptionsJson) ?? [],
        CorrectAnswer = CorrectAnswer,
        Explanation = Explanation,
        ExampleCode = ExampleCode,
        Difficulty = (DifficultyLevel)Difficulty
    };

    public static QuestionRecord From(Question q) => new()
    {
        Id = q.Id,
        TopicId = q.TopicId,
        Type = (int)q.Type,
        Text = q.Text,
        CodeSnippet = q.CodeSnippet,
        OptionsJson = JsonSerializer.Serialize(q.Options),
        CorrectAnswer = q.CorrectAnswer,
        Explanation = q.Explanation,
        ExampleCode = q.ExampleCode,
        Difficulty = (int)q.Difficulty
    };
}

[Table("sessions")]
public class SessionRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int TopicId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string AnswersJson { get; set; } = "[]";

    public QuizSession ToQuizSession()
    {
        var answers = JsonSerializer.Deserialize<List<QuizAnswer>>(AnswersJson) ?? [];
        return new QuizSession
        {
            Id = Id,
            TopicId = TopicId,
            StartTime = StartTime,
            EndTime = EndTime,
            Answers = answers
        };
    }

    public static SessionRecord From(QuizSession s) => new()
    {
        Id = s.Id,
        TopicId = s.TopicId,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        AnswersJson = JsonSerializer.Serialize(s.Answers)
    };
}

[Table("progress")]
public class ProgressRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public int TopicId { get; set; }
    public double MasteryPercent { get; set; }
    public int QuestionsAnswered { get; set; }
    public DateTime? LastAttempted { get; set; }

    public UserProgress ToUserProgress() => new()
    {
        Id = Id,
        TopicId = TopicId,
        MasteryPercent = MasteryPercent,
        QuestionsAnswered = QuestionsAnswered,
        LastAttempted = LastAttempted
    };
}

// ── Service ───────────────────────────────────────────────────────────────────

public class SqliteQuizService : IQuizService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is null)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "quiz.db");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<TopicRecord>();
            await _db.CreateTableAsync<QuestionRecord>();
            await _db.CreateTableAsync<SessionRecord>();
            await _db.CreateTableAsync<ProgressRecord>();
            await SeedIfEmptyAsync(_db);
        }
        return _db;
    }

    private static async Task SeedIfEmptyAsync(SQLiteAsyncConnection db)
    {
        if (await db.Table<TopicRecord>().CountAsync() == 0)
        {
            var topics = MockQuizService.SeedTopics().Select(TopicRecord.From).ToList();
            await db.InsertAllAsync(topics);
        }

        if (await db.Table<QuestionRecord>().CountAsync() == 0)
        {
            var questions = MockQuizService.SeedQuestions().Select(QuestionRecord.From).ToList();
            await db.InsertAllAsync(questions);
        }
    }

    // ── Read-only seed data ───────────────────────────────────────────────────

    public async Task<List<Topic>> GetTopicsAsync()
    {
        var db = await GetDbAsync();
        var records = await db.Table<TopicRecord>().ToListAsync();
        return records.Select(r => r.ToTopic()).ToList();
    }

    public async Task<List<Question>> GetQuestionsAsync(int topicId, DifficultyLevel? difficulty = null, int count = 10)
    {
        var db = await GetDbAsync();

        // topicId == 0 means cross-topic (Quick Quiz) — draw from the full question bank
        var allForTopic = topicId == 0
            ? await db.Table<QuestionRecord>().ToListAsync()
            : await db.Table<QuestionRecord>().Where(q => q.TopicId == topicId).ToListAsync();

        IEnumerable<QuestionRecord> pool = allForTopic;

        if (difficulty.HasValue && topicId != 0)
        {
            int diffInt = (int)difficulty.Value;
            var atLevel = allForTopic.Where(q => q.Difficulty == diffInt).ToList();
            pool = atLevel.Count >= count ? atLevel : allForTopic;
        }

        var result = pool.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        return result.Select(r => r.ToQuestion()).ToList();
    }

    // ── Persistent sessions ───────────────────────────────────────────────────

    public async Task SaveSessionAsync(QuizSession session)
    {
        var db = await GetDbAsync();
        var record = SessionRecord.From(session);
        await db.InsertAsync(record);
        session.Id = record.Id;
    }

    public async Task<List<QuizSession>> GetSessionsAsync(int? topicId = null)
    {
        var db = await GetDbAsync();
        var records = topicId.HasValue
            ? await db.Table<SessionRecord>().Where(s => s.TopicId == topicId.Value).ToListAsync()
            : await db.Table<SessionRecord>().ToListAsync();

        return records
            .OrderByDescending(s => s.StartTime)
            .Select(r => r.ToQuizSession())
            .ToList();
    }

    // ── Persistent progress ───────────────────────────────────────────────────

    public async Task<List<UserProgress>> GetProgressAsync()
    {
        var db = await GetDbAsync();
        var records = await db.Table<ProgressRecord>().ToListAsync();
        return records.Select(r => r.ToUserProgress()).ToList();
    }

    public async Task<UserProgress?> GetTopicProgressAsync(int topicId)
    {
        var db = await GetDbAsync();
        var record = await db.Table<ProgressRecord>().Where(p => p.TopicId == topicId).FirstOrDefaultAsync();
        return record?.ToUserProgress();
    }

    public async Task UpdateProgressAsync(int topicId, QuizSession session)
    {
        var db = await GetDbAsync();
        var record = await db.Table<ProgressRecord>().Where(p => p.TopicId == topicId).FirstOrDefaultAsync();

        if (record is null)
        {
            await db.InsertAsync(new ProgressRecord
            {
                TopicId = topicId,
                MasteryPercent = session.ScorePercent,
                QuestionsAnswered = session.TotalQuestions,
                LastAttempted = DateTime.UtcNow
            });
        }
        else
        {
            record.QuestionsAnswered += session.TotalQuestions;
            record.LastAttempted = DateTime.UtcNow;
            record.MasteryPercent = record.QuestionsAnswered == session.TotalQuestions
                ? session.ScorePercent
                : record.MasteryPercent * 0.7 + session.ScorePercent * 0.3;
            await db.UpdateAsync(record);
        }
    }
}
