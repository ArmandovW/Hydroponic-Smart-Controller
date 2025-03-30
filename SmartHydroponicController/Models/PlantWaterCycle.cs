using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class PlantWaterCycle
{
	[Key]
	public int PlantId { get; set; }
	public int Stage { get; set; }
	public DateTime? WaterCycleStarted { get; set; }
	public DateTime? WaterCycleEnded { get; set; }

}