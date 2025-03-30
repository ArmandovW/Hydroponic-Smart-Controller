using SmartHydroponicController.Models;
using SQLite;

namespace SmartHydroponicController.Data;

public class SQLiteDatabase
{
	private readonly string _databasePath;
	private SQLiteAsyncConnection _database;
	public SQLiteDatabase()
	{
		_databasePath = GetDatabasePath();
	}

	private string GetDatabasePath()
	{

#if ANDROID
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "HydroponicPlant.db3");
#elif IOS
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // iOS requires a different path
        string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Go up one level and then into Library
        string dbPath = Path.Combine(libraryPath, "HydroponicPlant.db3");
#elif WINDOWS
		//string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "MyDatabase.db3"); //older way
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "HydroponicPlant.db3");
#else
		//For other platforms (if any)
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "HydroponicPlant.db3");
#endif

		return dbPath;
	}

	private async Task Init()
	{
		if (_database != null)
			return;

		_database = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
		// Create tables if they don't exist.  This is crucial.
		await _database.CreateTableAsync<Plant>();
		await _database.CreateTableAsync<PlantProfile>();
		await _database.CreateTableAsync<PlantStatistics>();
		await _database.CreateTableAsync<PlantWaterCycle>();
		await _database.CreateTableAsync<Settings>();
	}

	// Create
	public async Task<int> AddItemAsync<T>(T item)
	{
		await Init();
		return await _database.InsertAsync(item);
	}

	// Insert All Plant Profiles
	public async Task<int> AddPlantsAsync(IEnumerable<Plant> item)
	{
		await Init();
		return await _database.InsertAllAsync(item);
	}
	public async Task<int> AddPlantProfilesAsync(IEnumerable<PlantProfile> item)
	{
		await Init();
		return await _database.InsertAllAsync(item);
	}
	// Get Settings
	public async Task<Settings> GetSettingsAsync()
	{
		await Init();
		return await _database.Table<Settings>().FirstOrDefaultAsync();
	}
	// Get Plant Water Cycle
	public async Task<List<PlantWaterCycle>> GetPlantWaterCycleAsync()
	{
		await Init();
		return await _database.Table<PlantWaterCycle>().ToListAsync();
	}
	// Get All Plants
	public async Task<List<Plant>> GetPlantsAsync()
	{
		await Init();
		return await _database.Table<Plant>().ToListAsync();
	}
	// Get All Plant Profiles
	public async Task<List<PlantProfile>> GetPlantProfilesAsync()
	{
		await Init();
		return await _database.Table<PlantProfile>().ToListAsync();
	}
	// Get All Plant Statistics
	public async Task<List<PlantStatistics>> GetPlantStatisticsAsync()
	{
		await Init();
		return await _database.Table<PlantStatistics>().ToListAsync();
	}
	// Get the plant profile stage
	public async Task<PlantProfile> GetPlantProfileByStageAsync(int stage)
	{
		await Init();
		return await _database.FindAsync<PlantProfile>(stage);
	}
	// Update
	public async Task<int> UpdateItemAsync(PlantProfile item)
	{
		await Init();
		return await _database.UpdateAsync(item);
	}

	// Update
	public async Task<int> UpdateWaterCycleAsync(PlantWaterCycle item)
	{
		await Init();
		return await _database.UpdateAsync(item);
	}
	// Delete
	public async Task<int> DeleteItemAsync(PlantProfile item)
	{
		await Init();
		return await _database.DeleteAsync(item);
	}

	public async Task ClearAllDatabaseTables()
	{
		await _database.DeleteAllAsync<PlantProfile>();
		await _database.DeleteAllAsync<PlantStatistics>();
		await Task.CompletedTask;
	}
}