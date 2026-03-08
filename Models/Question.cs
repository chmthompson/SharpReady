using SharpReady.Models.Enums;

namespace SharpReady.Models;

public class Question : BaseModel
{
    public int TopicId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string[] Options { get; set; } = [];
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? ExampleCode { get; set; }
    public DifficultyLevel Difficulty { get; set; }
}
