using SmartHydroponicController.Data;
using SmartHydroponicController.Services;
using SmartHydroponicController.ViewModels;
using SmartHydroponicController.Views;

namespace SmartHydroponicController;

internal static class AppConfig
{
	public static MauiAppBuilder ApplicationConfiguration(this MauiAppBuilder builder)
	{
		builder.Services.AddKeyedTransient("Anonymous", (sp, key) =>
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri(MauiProgram.httpBaseAddress);
			client.Timeout = TimeSpan.FromSeconds(30);
			return client;
		});
		builder.Services.AddSingleton<SQLiteDatabase>();
		builder.Services.AddSingleton<SerialPortService>();

		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<DashboardViewModel>();

		builder.Services.AddTransient<GrowthLogPage>();
		builder.Services.AddTransient<GrowthLogViewModel>();

		builder.Services.AddTransient<PlantProfilePage>();
		builder.Services.AddTransient<PlantProfileViewModel>();

		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<SettingsViewModel>();
		return builder;
	}
	public static void InitializeRoutes()
	{
		//Routing.RegisterRoute(nameof(AddEquipmentToBookingPage), typeof(AddEquipmentToBookingPage));
	}
}