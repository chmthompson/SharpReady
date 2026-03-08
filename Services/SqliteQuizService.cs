using System.Text.Json;
using SharpReady.Models;
using SharpReady.Models.Enums;
using SQLite;

namespace SharpReady.Services;

// ── SQLite record DTOs ────────────────────────────────────────────────────────

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

    private readonly List<Topic> _topics = MockQuizService.SeedTopics();
    private readonly List<Question> _questions = MockQuizService.SeedQuestions();

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is null)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "quiz.db");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<SessionRecord>();
            await _db.CreateTableAsync<ProgressRecord>();
        }
        return _db;
    }

    // ── Read-only seed data ───────────────────────────────────────────────────

    public Task<List<Topic>> GetTopicsAsync() => Task.FromResult(_topics);

    public Task<List<Question>> GetQuestionsAsync(int topicId, DifficultyLevel? difficulty = null, int count = 10)
    {
        // topicId == 0 means cross-topic (Quick Quiz) — draw from the full question bank
        var allForTopic = topicId == 0
            ? _questions.ToList()
            : _questions.Where(q => q.TopicId == topicId).ToList();

        IEnumerable<Question> pool = allForTopic;

        if (difficulty.HasValue && topicId != 0)
        {
            var atLevel = allForTopic.Where(q => q.Difficulty == difficulty.Value).ToList();
            pool = atLevel.Count >= count ? atLevel : allForTopic;
        }

        var result = pool.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        return Task.FromResult(result);
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
