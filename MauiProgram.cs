using DotNetStudyAssistant.Services;
using DotNetStudyAssistant.ViewModels;
using DotNetStudyAssistant.Views;
using Microsoft.Extensions.Logging;

namespace DotNetStudyAssistant;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.RegisterServices()
			.RegisterViewModels()
			.RegisterViews();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<IDataService, MockDataService>();
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddSingleton<IQuizService, MockQuizService>();
		return builder;
	}

	private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<DashboardViewModel>();
		builder.Services.AddSingleton<TopicsViewModel>();
		builder.Services.AddTransient<TopicDetailViewModel>();
		builder.Services.AddTransient<QuizViewModel>();
		builder.Services.AddTransient<QuizResultsViewModel>();
		builder.Services.AddSingleton<ProgressViewModel>();
		builder.Services.AddSingleton<SettingsViewModel>();
		return builder;
	}

	private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<DashboardPage>();
		builder.Services.AddSingleton<TopicsPage>();
		builder.Services.AddTransient<TopicDetailPage>();
		builder.Services.AddTransient<QuizPage>();
		builder.Services.AddTransient<QuizResultsPage>();
		builder.Services.AddSingleton<ProgressPage>();
		builder.Services.AddSingleton<SettingsPage>();
		return builder;
	}
}
