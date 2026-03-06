namespace DotNetStudyAssistant;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("TopicDetail", typeof(Views.TopicDetailPage));
		Routing.RegisterRoute("Quiz", typeof(Views.QuizPage));
		Routing.RegisterRoute("QuizResults", typeof(Views.QuizResultsPage));
	}
}
