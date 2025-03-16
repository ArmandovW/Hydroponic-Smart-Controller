using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class PlantStatistics
{
    [Key]
    public int PlantStatisticsId { get; set; }
    public int PlantId { get; set; }
    public int Stage { get; set; } // e.g., "Seedling", "Vegetative", "Flowering", "Fruiting"
    public decimal WaterTemperatureC { get; set; }
    public bool IdealWaterTemperatureAchieved { get; set; }
    public decimal PH { get; set; }
    public bool IdealPHAchieved { get; set; }
    public decimal TDS { get; set; } // Total Dissolved Solids (in parts per million)
    public bool IdealTDSAchieved { get; set; }
    public decimal AirTemperatureC { get; set; }
    public bool IdealAirTemperatureAchieved { get; set; }
    public decimal Humidity { get; set; } // Relative Humidity (percentage)
    public bool IdealHumidityAchieved { get; set; }
    public string Notes { get; set; }  // Additional notes about the profile
}