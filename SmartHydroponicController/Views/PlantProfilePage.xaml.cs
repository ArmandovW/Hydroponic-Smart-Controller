using SmartHydroponicController.ViewModels;

namespace SmartHydroponicController.Views;

public partial class PlantProfilePage : ContentPage
{
	public PlantProfilePage(PlantProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}