using DotNetStudyAssistant.Models.Enums;

namespace DotNetStudyAssistant.Models;

public class Question : BaseModel
{
    public int TopicId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
    public string[] Options { get; set; } = [];
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
}
