using System.ComponentModel.DataAnnotations;

namespace SmartHydroponicController.Models;

public class WaterTemp
{
    public string Range { get; set; }
    public decimal Temprature { get; set; }
}