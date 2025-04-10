using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;

namespace SmartHydroponicController.ViewModels;

public partial class PlantProfileViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	[ObservableProperty]
	ObservableCollection<PlantProfile> plantProfiles = new ObservableCollection<PlantProfile>();
	[ObservableProperty]
	ObservableCollection<Plant> plantToGrow = new ObservableCollection<Plant>();

	[ObservableProperty] private string currentSetPlantProfile;
	public PlantProfileViewModel(SQLiteDatabase database)
	{
		_db = database;
		MainThread.InvokeOnMainThreadAsync(async () =>
		{
			await LoadData();
		});
	}

	private async Task LoadData()
	{
		PlantToGrow.Clear();
		PlantToGrow = new ObservableCollection<Plant>()
				{
						new Plant()
						{
								PlantId = 1,
								PlantName = "Butter Lettuce",
								Description = "Butter lettuce is a tender, sweet, and mild-flavored leafy green with soft, buttery-textured leaves that form loose, round heads."
						},
						new Plant()
						{
								PlantId = 2,
								PlantName = "Tomato",
								Description = "A tomato is a juicy, tangy fruit with a smooth skin and a rich, vibrant color, commonly red, that ranges in flavor from sweet to savory."
						}
				};
		var plants = await _db.GetPlantsAsync();
		if (plants.Count() == 0) await _db.AddPlantsAsync(PlantToGrow);
		var currentSetProfiles = await _db.GetPlantProfilesAsync();
		if (currentSetProfiles.Count() > 0)
		{
			var plantId = currentSetProfiles.First().PlantId;
			CurrentSetPlantProfile = PlantToGrow.Where(x => x.PlantId == plantId).FirstOrDefault().PlantName;
			PlantProfiles = currentSetProfiles.ToObservableCollection();
		}
		else
			CurrentSetPlantProfile = "No Profile Set";
	}

	[RelayCommand]
	public async Task SelectPlantToGrow(DataGridCellTappedEventArgs args)
	{
		var plant = args.RowData as Plant;
		if (plant == null) return;
		var pp = await _db.GetPlantProfilesAsync();
		if (CurrentSetPlantProfile == "No Profile Set")
		{
			await SetPlantProfile(plant.PlantId);
			return;
		}
		else if (pp.Count() > 0 && pp.FirstOrDefault().PlantId != plant.PlantId)
		{
			var response = await Application.Current.MainPage.DisplayAlert("Set New Plant Profile?", $"Do you want to set {plant.PlantName} as your new plant profile?", "Yes", "No");
			if (response)
			{
				await _db.ClearSelectedDatabaseTables();
				await SetPlantProfile(plant.PlantId);
			}
			else
			{
				return;
			}
		}
		return;
	}

	private async Task SetPlantProfile(int plantId)
	{
		switch (plantId)
		{
			case 1:
				PlantProfiles = ButterLettuceProfiles().ToObservableCollection();
				break;
			case 2:
				PlantProfiles = TomatoProfiles().ToObservableCollection();
				break;
		}

		foreach (var item in PlantProfiles)
		{
			item.PlantId = plantId;
		}
		await _db.AddPlantProfilesAsync(PlantProfiles);
		CurrentSetPlantProfile = PlantToGrow.Where(x => x.PlantId == plantId).FirstOrDefault().PlantName;
		await Application.Current.MainPage.DisplayAlert("Plant Profile Set Success",
				$"Your new plant profile of {CurrentSetPlantProfile} has successfully been set.", "Ok");
		return;
	}
	public List<PlantProfile> ButterLettuceProfiles()
	{
		List<PlantProfile> _profiles = new List<PlantProfile>();
		// --- Seedling Stage ---
		PlantProfile seedling = new PlantProfile()
		{
			Stage = 1,
			StageDurationDays = 7,
			IdealWaterTemperatureMinC = 18, // Slightly lower for germination is often better.
			IdealWaterTemperatureMaxC = 24,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.8M,
			IdealTDSMinPPM = 0,   // No nutrients initially, just pure water.
			IdealTDSMaxPPM = 100,  // Allow for some minerals in the water.
			IdealAirTemperatureMinC = 20,
			IdealAirTemperatureMaxC = 26,
			IdealHumidityMin = 70, // Higher humidity for germination.
			IdealHumidityMax = 80,
			NutrientSolution = "None (Pure Water initially)",
			Notes = "Use a humidity dome. Monitor moisture levels in starter plugs closely.",
			PlantRootWaterLevel = "Moist starter plug, roots not yet in nutrient solution",
			Airflow = "Very Gentle, almost none",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 24
		};

		_profiles.Add(seedling);

		// --- Vegetative Stage ---
		PlantProfile vegetative = new PlantProfile()
		{
			Stage = 2,
			StageDurationDays = 14,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 24,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.8M,
			IdealTDSMinPPM = 400,
			IdealTDSMaxPPM = 600,
			IdealAirTemperatureMinC = 18,
			IdealAirTemperatureMaxC = 25,
			IdealHumidityMin = 65,
			IdealHumidityMax = 75,
			NutrientSolution = "General Hydroponics FloraMicro + FloraGro + FloraBloom (1/4 - 1/2 Strength)",
			Notes = "Start with a diluted nutrient solution and gradually increase strength.",
			PlantRootWaterLevel = "Roots in media",
			Airflow = "Gentle breeze",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 18
		};

		_profiles.Add(vegetative);

		// --- Flowering Stage ---
		PlantProfile flowering = new PlantProfile()
		{
			Stage = 3,
			StageDurationDays = 17,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 24,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.8M,
			IdealTDSMinPPM = 600,
			IdealTDSMaxPPM = 800,
			IdealAirTemperatureMinC = 18,
			IdealAirTemperatureMaxC = 24,
			IdealHumidityMin = 60,
			IdealHumidityMax = 70,
			NutrientSolution = "General Hydroponics FloraMicro + FloraGro + FloraBloom (Full Strength, adjust as needed)",
			Notes = "Monitor EC and pH closely. Adjust nutrient ratios based on plant response.",
			PlantRootWaterLevel = "Roots in media",
			Airflow = "Moderate breeze",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 14
		};

		_profiles.Add(flowering);
		// --- Fruiting Stage ---
		PlantProfile fruiting = new PlantProfile()
		{
			Stage = 4,
			StageDurationDays = 7,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 24,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.8M,
			IdealTDSMinPPM = 400,    // Reduce TDS before harvest
			IdealTDSMaxPPM = 600,
			IdealAirTemperatureMinC = 18,
			IdealAirTemperatureMaxC = 24,
			IdealHumidityMin = 55,  //Slightly lower humity
			IdealHumidityMax = 65,
			NutrientSolution = "General Hydroponics FloraMicro + FloraGro + FloraBloom (Reduced Strength or Flush)", // Flushing is optional
			Notes = "Reduce watering slightly.  Consider flushing with plain pH-balanced water for a cleaner taste",
			PlantRootWaterLevel = "Roots in media",
			Airflow = "Moderate breeze",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 12
		};
		_profiles.Add(fruiting);
		return _profiles;
	}
	public List<PlantProfile> TomatoProfiles()
	{
		List<PlantProfile> _profiles = new List<PlantProfile>();
		// --- Seedling Stage ---
		PlantProfile seedling = new PlantProfile()
		{
			Stage = 1,
			StageDurationDays = 15,
			IdealWaterTemperatureMinC = 20,
			IdealWaterTemperatureMaxC = 25,
			IdealPHMin = 5.8M,
			IdealPHMax = 6.2M,
			IdealTDSMinPPM = 350,
			IdealTDSMaxPPM = 500,
			IdealAirTemperatureMinC = 22,
			IdealAirTemperatureMaxC = 28,
			IdealHumidityMin = 60,
			IdealHumidityMax = 70,
			NutrientSolution = "General Hydroponics FloraMicro + FloraGro (Reduced Strength)",
			Notes = "Use a diluted nutrient solution.",
			PlantRootWaterLevel = "Tip of roots",
			Airflow = "Gentle breeze",
			WateringScheduleDailyDosingMinutes = 20,
			WateringScheduleDailyCycle = 6
		};

		_profiles.Add(seedling);

		// --- Vegetative Stage ---
		PlantProfile vegetative = new PlantProfile()
		{
			Stage = 2,
			StageDurationDays = 21,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 24,
			IdealPHMin = 5.8M,
			IdealPHMax = 6.3M,
			IdealTDSMinPPM = 800,
			IdealTDSMaxPPM = 1200,
			IdealAirTemperatureMinC = 20,
			IdealAirTemperatureMaxC = 26,
			IdealHumidityMin = 50,
			IdealHumidityMax = 65,
			NutrientSolution = "General Hydroponics Flora Series (Full Strength)",
			Notes = "Increase nutrient strength gradually.",
			PlantRootWaterLevel = "Tip of roots",
			Airflow = "Moderate ci",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 6
		};

		_profiles.Add(vegetative);

		// --- Flowering Stage ---
		PlantProfile flowering = new PlantProfile()
		{
			Stage = 3,
			StageDurationDays = 14,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 23,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.5M,
			IdealTDSMinPPM = 1000,
			IdealTDSMaxPPM = 1800, //  High EC during flowering/fruiting
			IdealAirTemperatureMinC = 18,
			IdealAirTemperatureMaxC = 25,
			IdealHumidityMin = 45,
			IdealHumidityMax = 55,
			NutrientSolution = "General Hydroponics Flora Series (Bloom Formula)",
			Notes = "Lower humidity to prevent mold. Increase potassium and phosphorus.",
			PlantRootWaterLevel = "Tip of roots",
			Airflow = "Good circulation to prevent fungal diseases",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 7
		};

		_profiles.Add(flowering);
		// --- Fruiting Stage ---
		PlantProfile fruiting = new PlantProfile()
		{
			Stage = 4,
			StageDurationDays = 7,
			IdealWaterTemperatureMinC = 18,
			IdealWaterTemperatureMaxC = 23,
			IdealPHMin = 6.0M,
			IdealPHMax = 6.5M,
			IdealTDSMinPPM = 1400, // Higher TDS during fruiting
			IdealTDSMaxPPM = 2000, // Even higher.  These are guidelines
			IdealAirTemperatureMinC = 18,
			IdealAirTemperatureMaxC = 24,
			IdealHumidityMin = 40,
			IdealHumidityMax = 50,
			NutrientSolution = "General Hydroponics Flora Series (Bloom Formula)",
			Notes = "Maintain low humidity. Provide support for heavy fruit. Continue pruning.",
			PlantRootWaterLevel = "Tip of roots",
			Airflow = "Good circulation",
			WateringScheduleDailyDosingMinutes = 15,
			WateringScheduleDailyCycle = 10
		};
		_profiles.Add(fruiting);
		return _profiles;
	}
}