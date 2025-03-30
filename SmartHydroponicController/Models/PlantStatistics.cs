using SQLite;
using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class PlantStatistics
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public int PlantId { get; set; }
	public int Stage { get; set; } // e.g., "Seedling", "Vegetative", "Flowering", "Fruiting"
	public decimal WaterTemperatureC { get; set; }
	public bool IdealWaterTemperatureAchieved { get; set; }
	public decimal PH { get; set; }
	public bool IdealPHAchieved { get; set; }
	public decimal TDS { get; set; } // Total Dissolved Solids (in parts per million)
	public bool IdealTDSAchieved { get; set; }
	public decimal Humidity { get; set; } // Relative Humidity (percentage)
	public bool IdealHumidityAchieved { get; set; }
	public DateTime? DateAdded { get; set; }
	public string? Notes { get; set; }  // Additional notes about the profile
}