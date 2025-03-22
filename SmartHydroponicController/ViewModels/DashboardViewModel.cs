
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Services;

namespace SmartHydroponicController.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SQLiteDatabase _db;
    private readonly SerialPortService _serialService;
    
    [ObservableProperty] public string currentSetPlantProfile;
    [ObservableProperty] public string plantHealthStatus;
    [ObservableProperty] public string dataReadings;
    public DashboardViewModel(SQLiteDatabase database, SerialPortService serialPortService)
    {
        _db = database;
        _serialService = serialPortService;
        _serialService.DataReceived += SerialServiceOnDataReceived;
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await LoadDataAsync();
            PlantHealthStatus = "excellent";
        });
    }

    private void SerialServiceOnDataReceived(object? sender, string data)
    {
        DataReadings = data;
    }

    private async Task LoadDataAsync()
    {
        var plants = await _db.GetPlantsAsync();
        var plantProfiles = await _db.GetPlantProfilesAsync();
        if (plants.Count() > 0)
        {
            CurrentSetPlantProfile = plants.Where(x => x.PlantId == plantProfiles.First().PlantId).First().PlantName;
        }
        else
        {
            CurrentSetPlantProfile = "No Plant Profile Set";
        }
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
        if (PlantHealthStatus =="excellent")
        {
            PlantHealthStatus = "good";
        }
        else if(PlantHealthStatus =="good")
        {
            PlantHealthStatus = "bad";
        }
        else
        {
            PlantHealthStatus = "verybad";
        }
    }

    private async Task LoadSensorDataAsync()
    {
        
    }
}