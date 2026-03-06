using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Models.Enums;

namespace DotNetStudyAssistant.Services;

public interface IQuizService
{
    Task<List<Topic>> GetTopicsAsync();
    Task<List<Question>> GetQuestionsAsync(int topicId, DifficultyLevel? difficulty = null, int count = 10);
    Task SaveSessionAsync(QuizSession session);
    Task<List<QuizSession>> GetSessionsAsync(int? topicId = null);
    Task<List<UserProgress>> GetProgressAsync();
    Task<UserProgress?> GetTopicProgressAsync(int topicId);
    Task UpdateProgressAsync(int topicId, QuizSession session);
}
