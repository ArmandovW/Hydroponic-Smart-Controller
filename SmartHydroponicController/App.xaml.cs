using SmartHydroponicController.Views;

namespace SmartHydroponicController;

public partial class App : Application
{
	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXxeeXRTR2lZVEVwWkY=");
		InitializeComponent();
		MainPage = new AppShell();
		Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
	}
}