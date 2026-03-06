namespace DotNetStudyAssistant.Models;

public class QuizSession : BaseModel
{
    public int TopicId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public List<QuizAnswer> Answers { get; set; } = [];

    public int Score => Answers.Count(a => a.IsCorrect);
    public int TotalQuestions => Answers.Count;
    public double ScorePercent => TotalQuestions > 0 ? (double)Score / TotalQuestions * 100 : 0;
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
}
