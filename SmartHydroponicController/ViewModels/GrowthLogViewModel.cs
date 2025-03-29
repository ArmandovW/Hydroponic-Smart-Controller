using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataSource.Extensions;

namespace SmartHydroponicController.ViewModels;

public partial class GrowthLogViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	[ObservableProperty] private ObservableCollection<PlantStatistics> growthLog;
	public GrowthLogViewModel(SQLiteDatabase database)
	{
		_db = database;
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await LoadData();
		});

	}

	private async Task LoadData()
	{
		GrowthLog = new ObservableCollection<PlantStatistics>();
		try
		{
			var logs = await _db.GetPlantStatisticsAsync();
			GrowthLog = logs.ToObservableCollection();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			GrowthLog.Clear();
		}

	}
}