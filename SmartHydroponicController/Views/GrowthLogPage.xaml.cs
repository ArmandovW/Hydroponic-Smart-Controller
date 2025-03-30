using SmartHydroponicController.ViewModels;

namespace SmartHydroponicController.Views;

public partial class GrowthLogPage : ContentPage
{
	public GrowthLogPage(GrowthLogViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}