using SQLite;
using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class Settings
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? PortName { get; set; }
	public int BaudeRate { get; set; }
	public int DataBits { get; set; }
}