using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class PlantProfile
{
	[Key]
	public int PlantId { get; set; }
	public int Stage { get; set; } // e.g., "Seedling", "Vegetative", "Flowering", "Fruiting"
	public int StageDurationDays { get; set; }   //How long tha plant should be on this stage before moving to the next
	public decimal IdealWaterTemperatureMinC { get; set; }
	public decimal IdealWaterTemperatureMaxC { get; set; }
	public decimal IdealPHMin { get; set; }
	public decimal IdealPHMax { get; set; }
	public decimal IdealTDSMinPPM { get; set; } // Total Dissolved Solids (in parts per million)
	public decimal IdealTDSMaxPPM { get; set; }
	public decimal IdealAirTemperatureMinC { get; set; }
	public decimal IdealAirTemperatureMaxC { get; set; }
	public decimal IdealHumidityMin { get; set; } // Relative Humidity (percentage)
	public decimal IdealHumidityMax { get; set; }
	public string? NutrientSolution { get; set; } // e.g., "General Hydroponics Flora Series", "Masterblend", "Custom Mix"
	public string? Notes { get; set; }  // Additional notes about the profile
	public string? PlantRootWaterLevel { get; set; } //How much of the root should be submerged, e.g "Tip of roots"
	public string? Airflow { get; set; }
	public decimal WateringScheduleDailyDosingMinutes { get; set; }
	public decimal WateringScheduleDailyCycle { get; set; }
}