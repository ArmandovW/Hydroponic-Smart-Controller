using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using SmartHydroponicController.Services;
using Windows.Media.Devices;

namespace SmartHydroponicController.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	private readonly SerialPortService _serialService;

	[ObservableProperty] public string[] serialPortNames;
	[ObservableProperty] public Settings comSettings;

	public SettingsViewModel(SQLiteDatabase database, SerialPortService serialPortService)
	{
		_db = database;
		_serialService = serialPortService;
		SerialPortNames = _serialService.GetPorts();
		ComSettings = new Settings();
	}
	[RelayCommand]
	public async Task SaveSettings()
	{
		ComSettings.Id = 0;
		var result = await _db.AddItemAsync<Settings>(ComSettings);
		if (result != 0) await Application.Current.MainPage.DisplayAlert("Save settings?", $"Settings saved successfully", "Ok");
		else await Application.Current.MainPage.DisplayAlert("Save Failed?", $"Settings failed to save", "Ok");
	}
}