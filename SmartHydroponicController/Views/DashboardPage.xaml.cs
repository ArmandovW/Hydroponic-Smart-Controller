using SmartHydroponicController.ViewModels;

namespace SmartHydroponicController.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}