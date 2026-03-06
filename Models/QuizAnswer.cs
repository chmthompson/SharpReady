namespace DotNetStudyAssistant.Models;

public class QuizAnswer
{
    public int QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
