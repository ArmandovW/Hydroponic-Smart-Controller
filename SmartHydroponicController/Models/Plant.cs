using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class Plant
{
    [Key]
    public int PlantId { get; set; }
    public string PlantName { get; set; }
    public string Description { get; set; }
}