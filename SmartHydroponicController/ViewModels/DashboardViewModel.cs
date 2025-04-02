
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
	private int insertLogCounter = 0;

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
	[ObservableProperty] public bool showPumpGif = false;

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
		SetSerialReadings(data);
		if (DataReadings.Contains("OFF")) PumpStatus = "PUMPOFF";
		else PumpStatus = "PUMPON";
		if (PumpStatus == "PUMPON") ShowPumpGif = true;
		else ShowPumpGif = false;
		insertLogCounter++;
		if(insertLogCounter >= 5)
		{
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await LogPlantProgress();
				await PlantHealth();
				insertLogCounter = 0;
			});
		}
	}

	private void SetSerialReadings(string data)
	{
		var values = data.Split('$').ToList();
		try
		{
			CurrentWaterTemp = Convert.ToDecimal(values[0].Substring(2));
			CurrentHumidity = Convert.ToDecimal(values[1].Substring(2));
			CurrentPH = Convert.ToDecimal(values[3].Substring(2));

			if (CurrentWaterTemp >= WaterReadings.Min && CurrentWaterTemp <= WaterReadings.Max) WaterReadingStatusColor = "ForestGreen";
			else WaterReadingStatusColor = "Red";
			if (CurrentHumidity >= HumidityReadings.Min && CurrentHumidity <= HumidityReadings.Max) HumidityReadingStatusColor = "ForestGreen";
			else HumidityReadingStatusColor = "Red";
			if (CurrentPH >= PHReadings.Min && CurrentPH <= PHReadings.Max) PHReadingStatusColor = "ForestGreen";
			else PHReadingStatusColor = "Red";
			if (CurrentTDS >= TdsReadings.Min && CurrentTDS <= TdsReadings.Max) TdsReadingStatusColor = "ForestGreen";
			else TdsReadingStatusColor = "Red";

		}
		catch (Exception)
		{

			WaterReadings.Current = 0M;
		}
		
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
		if (plants.Count() > 0 && CurrentPlantProfile != null)
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
		if(waterCycle.Count() == 0 && PumpStatus == "PUMPOFF")
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
			if(lastWaterCycle.WaterCycleEnded == null) return;
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
		if (log.Count() == 0) return;
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
	}

	private void SetTDSReadings(PlantProfile profile)
	{
		TdsReadings = new SerialReadings
		{
			Min = profile.IdealTDSMinPPM,
			Max = profile.IdealTDSMaxPPM,
			Current = 0M
		};
	}

	private void SetPHReadings(PlantProfile profile)
	{
		PHReadings = new SerialReadings
		{
			Min = profile.IdealPHMin,
			Max = profile.IdealPHMax,
			Current = 0M
		};
	}
	private void SetHumidityReadings(PlantProfile profile)
	{
		HumidityReadings = new SerialReadings
		{
			Min = profile.IdealHumidityMin,
			Max = profile.IdealHumidityMax,
			Current = 0M
		};
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
	private async Task RestartProcess()
	{
		await _db.ClearAllDatabaseTables();
	}
}