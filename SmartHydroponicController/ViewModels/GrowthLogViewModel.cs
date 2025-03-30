using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartHydroponicController.Data;
using SmartHydroponicController.Models;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;

namespace SmartHydroponicController.ViewModels;

public partial class GrowthLogViewModel : ObservableObject
{
	private readonly SQLiteDatabase _db;
	[ObservableProperty] private ObservableCollection<PlantStatistics> growthLog;
	public GrowthLogViewModel(SQLiteDatabase database)
	{
		_db = database;
		GrowthLog = new ObservableCollection<PlantStatistics>();
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await LoadData();
		});

	}
	private async Task LoadData()
	{
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
	[RelayCommand]
	private async Task RefreshData()
	{
		GrowthLog.Clear();
		try
		{
			await LoadData();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

	}

}