using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;

namespace SmartHydroponicController.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SQLiteDatabase _db;
    [ObservableProperty] public string currentSetPlantProfile;
    [ObservableProperty] public string plantHealthStatus;
    public DashboardViewModel(SQLiteDatabase database)
    {
        _db = database;
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await LoadDataAsync();
            PlantHealthStatus = "excellent";
        });
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