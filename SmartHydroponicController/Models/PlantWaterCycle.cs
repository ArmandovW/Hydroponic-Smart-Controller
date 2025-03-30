using SQLite;
using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class PlantWaterCycle
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public int PlantId { get; set; }
	public int Stage { get; set; }
	public DateTime? WaterCycleStarted { get; set; }
	public DateTime? WaterCycleEnded { get; set; }

}