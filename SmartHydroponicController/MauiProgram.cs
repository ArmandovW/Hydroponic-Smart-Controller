using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace SmartHydroponicController;

public static class MauiProgram
{
	public static string httpBaseAddress = "http://localhost:5000";
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
				.UseMauiApp<App>()
				.ConfigureSyncfusionCore()
				.ApplicationConfiguration()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}