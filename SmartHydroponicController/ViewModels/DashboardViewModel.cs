
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using SmartHydroponicController.Services;
using System.Collections.ObjectModel;

namespace SmartHydroponicController.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	private readonly SerialPortService _serialService;

	[ObservableProperty] public string currentSetPlantProfile;
	[ObservableProperty] public string plantHealthStatus;
	[ObservableProperty] public string dataReadings;
	[ObservableProperty] public string pumpStatus;

	[ObservableProperty] public ObservableCollection<WaterTemp> waterTemprature;
	[ObservableProperty] public ObservableCollection<Brush> customBrushes;
	public DashboardViewModel(SQLiteDatabase database, SerialPortService serialPortService)
	{
		_db = database;
		_serialService = serialPortService;
		_serialService.DataReceived += SerialServiceOnDataReceived;
		MainThread.InvokeOnMainThreadAsync(async () =>
		{
			await LoadDataAsync();
			PlantHealthStatus = "excellent.png";
		});
	}

	private void SerialServiceOnDataReceived(object? sender, string data)
	{
		DataReadings = string.Empty;
		DataReadings = data;
	}

	private async Task LoadDataAsync()
	{
		var settings = await _db.GetSettingsAsync();
		if (settings != null)
		{
			_serialService.OpenPort(settings.PortName, settings.BaudeRate, settings.DataBits);
			PumpStatus = "PUMPOFF";
		}
		else
		{
			CurrentSetPlantProfile = "Add COM Settings";
		}
		var plants = await _db.GetPlantsAsync();
		var plantProfiles = await _db.GetPlantProfilesAsync();
		if (plants.Count() > 0)
		{
			CurrentSetPlantProfile = plants.Where(x => x.PlantId == plantProfiles.First().PlantId).First().PlantName;
		}
		else
		{
			CurrentSetPlantProfile = "No Plant Profile Set";
		}

		WaterTemprature = new ObservableCollection<WaterTemp>()
				{
						new WaterTemp { Range = "Max", Temprature = plantProfiles.First().IdealWaterTemperatureMaxC },
						new WaterTemp { Range = "Min", Temprature = plantProfiles.First().IdealWaterTemperatureMinC },
						new WaterTemp { Range = "Current", Temprature = 22.6M }
				};
		CustomBrushes = new ObservableCollection<Brush>();
		foreach (var item in WaterTemprature)
		{
			if (item.Range == "Max") CustomBrushes.Add(new SolidColorBrush(Colors.DarkRed));
			else if (item.Range == "Min") CustomBrushes.Add(new SolidColorBrush(Colors.Orange));
		}

		var max = WaterTemprature.Where(x => x.Range == "Max").FirstOrDefault().Temprature;
		var min = WaterTemprature.Where(x => x.Range == "Min").FirstOrDefault().Temprature;
		var current = WaterTemprature.Where(x => x.Range == "Current").FirstOrDefault().Temprature;
		if (current < min || current > max) CustomBrushes.Add(new SolidColorBrush(Colors.Red));
		else CustomBrushes.Add(new SolidColorBrush(Colors.Green));
	}
	[RelayCommand]
	public async Task RefreshData()
	{
		await LoadDataAsync();
		await PlantHealth();
	}

	[RelayCommand]
	public async Task PlantHealth()
	{
		if (PlantHealthStatus == "excellent.png")
		{
			PlantHealthStatus = "good.png";
		}
		else if (PlantHealthStatus == "good.png")
		{
			PlantHealthStatus = "bad.png";
		}
		else
		{
			PlantHealthStatus = "verybad.png";
		}
	}
	[RelayCommand]
	private void PumpOn()
	{
		if(PumpStatus == "PUMPOFF")
		{
			PumpStatus = "PUMPON";
			_serialService.WriteData(PumpStatus);

		}
		else
		{
			PumpStatus = "PUMPOFF";
			_serialService.WriteData(PumpStatus);

		}
	}
}