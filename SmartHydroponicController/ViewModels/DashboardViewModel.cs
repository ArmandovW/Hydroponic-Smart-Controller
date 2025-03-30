
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using SmartHydroponicController.Services;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SmartHydroponicController.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	private readonly SerialPortService _serialService;

	[ObservableProperty] public string currentSetPlantProfile;
	[ObservableProperty] public string plantHealthStatus;
	[ObservableProperty] public string dataReadings;
	[ObservableProperty] public string pumpStatus;

	[ObservableProperty] public decimal currentWaterTemp = 0;
	[ObservableProperty] public decimal currentPH = 0;
	[ObservableProperty] public decimal currentTDS = 0;
	[ObservableProperty] public decimal currentHumidity = 0;

	[ObservableProperty] public string waterReadingStatusColor;
	[ObservableProperty] public string tdsReadingStatusColor;
	[ObservableProperty] public string pHReadingStatusColor;
	[ObservableProperty] public string humidityReadingStatusColor;

	[ObservableProperty] public int currentPlantStage;
	[ObservableProperty] public PlantProfile currentPlantProfile;

	[ObservableProperty] public SerialReadings waterReadings = new();
	[ObservableProperty] public SerialReadings tdsReadings = new();
	[ObservableProperty] public SerialReadings pHReadings = new();
	[ObservableProperty] public SerialReadings humidityReadings = new();
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
		MainThread.BeginInvokeOnMainThread(async () => 
		{
			await LogPlantProgress();
		});
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
		var plantStats = await _db.GetPlantStatisticsAsync();
		CurrentPlantStage = plantStats.LastOrDefault() is not null ? plantStats.LastOrDefault().Stage : 1;
		CurrentPlantProfile = await _db.GetPlantProfileByStageAsync(CurrentPlantStage);
		if (plants.Count() > 0)
		{
			CurrentSetPlantProfile = plants.Where(x => x.PlantId == CurrentPlantProfile.PlantId).First().PlantName;
			SetWaterReadings(CurrentPlantProfile);
			SetTDSReadings(CurrentPlantProfile);
			SetHumidityReadings(CurrentPlantProfile);
			SetPHReadings(CurrentPlantProfile);
		}
		else
		{
			CurrentSetPlantProfile = "No Plant Profile Set";
		}

	}

	private async Task LogPlantProgress()
	{
		await CheckPlantStage();
		PlantStatistics plantStatistics = new PlantStatistics()
		{
			PlantId = CurrentPlantProfile.PlantId,
			Stage = CurrentPlantStage,
			WaterTemperatureC = CurrentWaterTemp,
			IdealWaterTemperatureAchieved = (CurrentWaterTemp > CurrentPlantProfile.IdealWaterTemperatureMinC && CurrentWaterTemp < CurrentPlantProfile.IdealWaterTemperatureMaxC) ? true : false,
			PH = CurrentPH,
			IdealPHAchieved = (CurrentWaterTemp > CurrentPlantProfile.IdealWaterTemperatureMinC && CurrentWaterTemp < CurrentPlantProfile.IdealWaterTemperatureMaxC) ? true : false,
			TDS = CurrentTDS,
			IdealTDSAchieved = (CurrentWaterTemp > CurrentPlantProfile.IdealWaterTemperatureMinC && CurrentWaterTemp < CurrentPlantProfile.IdealWaterTemperatureMaxC) ? true : false,
			Humidity = CurrentHumidity,
			IdealHumidityAchieved = (CurrentWaterTemp > CurrentPlantProfile.IdealWaterTemperatureMinC && CurrentWaterTemp < CurrentPlantProfile.IdealWaterTemperatureMaxC) ? true : false,
			DateAdded = DateTime.Now,
			Notes = $"{PumpStatus}"
		};
		await _db.AddItemAsync<PlantStatistics>(plantStatistics);
		var waterCycle = await _db.GetPlantWaterCycleAsync();
		if(waterCycle is null && PumpStatus == "PUMPOFF")
		{
			_serialService.WriteData("PUMPON");
			PlantWaterCycle plantWaterCycle = new PlantWaterCycle
			{
				PlantId = CurrentPlantProfile.PlantId,
				Stage = CurrentPlantStage,
				WaterCycleStarted = DateTime.Now,
				WaterCycleEnded = null
			};
			await _db.AddItemAsync(plantWaterCycle);
		}
		else if (PumpStatus == "PUMPON")
		{
			var currentWaterCycle = waterCycle.Where(x => x.WaterCycleEnded is null).FirstOrDefault();
			var pumpRunningTime = (DateTime.Now - currentWaterCycle.WaterCycleStarted).Value.Minutes;
			if(pumpRunningTime >= CurrentPlantProfile.WateringScheduleDailyDosingMinutes)
			{
				currentWaterCycle.WaterCycleEnded = DateTime.Now;
				await _db.UpdateWaterCycleAsync(currentWaterCycle);
				_serialService.WriteData("PUMPOFF");
			}
		}
		else
		{
			var lastWaterCycle = waterCycle.LastOrDefault();
			var today = lastWaterCycle.WaterCycleEnded.Value.Date;
			var cycleCount = waterCycle.Where(x => x.WaterCycleEnded.Value.Date == today).Count();
			var hourInterval = 24 / CurrentPlantProfile.WateringScheduleDailyCycle;
			var pumpDailyCycle = (DateTime.Now - lastWaterCycle.WaterCycleEnded).Value.Hours;
			if(pumpDailyCycle >= hourInterval)
			{
				_serialService.WriteData("PUMPON");
				PlantWaterCycle plantWaterCycle = new PlantWaterCycle
				{
					PlantId = CurrentPlantProfile.PlantId,
					Stage = CurrentPlantStage,
					WaterCycleStarted = DateTime.Now,
					WaterCycleEnded = null
				};
				await _db.AddItemAsync(plantWaterCycle);
			}
		}

	}

	private async Task CheckPlantStage()
	{
		var log = await _db.GetPlantStatisticsAsync();
		var currentDate = DateTime.Now;
		var elapsedDays = (currentDate - log.Where(x => x.Stage == CurrentPlantStage).FirstOrDefault().DateAdded.Value).Days;
		if (elapsedDays >= CurrentPlantProfile.StageDurationDays)
		{
			CurrentPlantStage++;
			CurrentPlantProfile = await _db.GetPlantProfileByStageAsync(CurrentPlantStage);
		}
	}

	private void SetWaterReadings(PlantProfile profile)
	{
		WaterReadings = new SerialReadings
		{
			Min = profile.IdealWaterTemperatureMinC,
			Max = profile.IdealWaterTemperatureMaxC,
			Current = 22.4M
		};
		if (WaterReadings.Current >= WaterReadings.Min && WaterReadings.Current <= WaterReadings.Max) WaterReadingStatusColor = "ForestGreen";
		else WaterReadingStatusColor = "Red";
	}

	private void SetTDSReadings(PlantProfile profile)
	{
		TdsReadings = new SerialReadings
		{
			Min = profile.IdealTDSMinPPM,
			Max = profile.IdealTDSMaxPPM,
			Current = 0M
		};
		if (TdsReadings.Current >= TdsReadings.Min && TdsReadings.Current <= TdsReadings.Max) TdsReadingStatusColor = "ForestGreen";
		else TdsReadingStatusColor = "Red";
	}

	private void SetPHReadings(PlantProfile profile)
	{
		PHReadings = new SerialReadings
		{
			Min = profile.IdealPHMin,
			Max = profile.IdealPHMax,
			Current = 0M
		};
		if (PHReadings.Current >= PHReadings.Min && PHReadings.Current <= PHReadings.Max) PHReadingStatusColor = "ForestGreen";
		else PHReadingStatusColor = "Red";
	}
	private void SetHumidityReadings(PlantProfile profile)
	{
		HumidityReadings = new SerialReadings
		{
			Min = profile.IdealHumidityMin,
			Max = profile.IdealHumidityMax,
			Current = 0M
		};
		if (HumidityReadings.Current >= HumidityReadings.Min && HumidityReadings.Current <= HumidityReadings.Max) HumidityReadingStatusColor = "ForestGreen";
		else HumidityReadingStatusColor = "Red";
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
		int falseCount = 0;
		var log = await _db.GetPlantStatisticsAsync();
		if (log.Count == 0) return;
		else
		{
			foreach (PropertyInfo property in log.LastOrDefault().GetType().GetProperties())
			{
				if (property.PropertyType == typeof(bool))
				{
					bool value = (bool)property.GetValue(log.LastOrDefault());
					if (!value)
					{
						falseCount++;
					}
				}
			}
			switch (falseCount)
			{
				case 0:
					PlantHealthStatus = "excellent.png";
					break;
				case 1:
					PlantHealthStatus = "good.png";
					break;
				case 2:
					PlantHealthStatus = "unhappy.png";
					break;
				case 3:
					PlantHealthStatus = "bad.png";
					break;
				case 4:
					PlantHealthStatus = "verybad.png";
					break;
				default:
					break;
			}
		}
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
		if (PumpStatus == "PUMPOFF")
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