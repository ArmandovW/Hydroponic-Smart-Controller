using CommunityToolkit.Mvvm.ComponentModel;
using SmartHydroponicController.Data;
using SmartHydroponicController.Services;

namespace SmartHydroponicController.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SQLiteDatabase _db;
    private readonly SerialPortService _serialService;
    
    [ObservableProperty] public string[] serialPortNames;
    public SettingsViewModel(SQLiteDatabase database, SerialPortService serialPortService)
    {
        _db = database;
        _serialService = serialPortService;
        SerialPortNames = _serialService.GetPortNames();
    }
}