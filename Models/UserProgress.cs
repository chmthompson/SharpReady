namespace SharpReady.Models;

public class UserProgress : BaseModel
{
    public int TopicId { get; set; }
    public double MasteryPercent { get; set; }
    public int QuestionsAnswered { get; set; }
    public DateTime? LastAttempted { get; set; }
}
