using SmartHydroponicController.Data;
using System.IO.Ports;
using System.Text;

namespace SmartHydroponicController.Services;

public class SerialPortService
{
	private readonly SQLiteDatabase _db;

	private SerialPort _serialPort;
	private StringBuilder _receivedData = new StringBuilder();
	public event EventHandler<string> DataReceived;

	public SerialPortService(SQLiteDatabase database)
	{
		try
		{
			_db = database;
			_serialPort = new SerialPort();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	public string[] GetPorts()
	{
		return SerialPort.GetPortNames();
	}

	public bool OpenPort(string portName, int baudRate = 9600, int dataBits = 8)
	{
		try
		{
			var settings = _db.GetSettingsAsync();
			if (settings == null) return false;
			_serialPort = new SerialPort(portName, baudRate, Parity.None, dataBits, StopBits.One);
			_serialPort.DataReceived += SerialPort_DataReceived;
			_serialPort.ErrorReceived += SerialPort_ErrorReceived;
			_serialPort.ReadTimeout = 500; // Important: Set a timeout
			_serialPort.WriteTimeout = 500;
			_serialPort.Open();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error opening port: {ex.Message}");
			// Handle the exception (log, show error to user, etc.)
			ClosePort(); // Ensure port is closed on error
			return false;
		}
	}
	public void ClosePort()
	{
		if (_serialPort != null && _serialPort.IsOpen)
		{
			_serialPort.DataReceived -= SerialPort_DataReceived;
			_serialPort.ErrorReceived -= SerialPort_ErrorReceived;
			_serialPort.Close();
			_serialPort.Dispose(); // Release resources
			_serialPort = null;
		}
	}

	public void WriteData(string data)
	{
		if (_serialPort != null && _serialPort.IsOpen)
		{
			try
			{
				_serialPort.Write(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error writing to port: {ex.Message}");
			}
		}
	}

	private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		try
		{
			// Read all available data from the buffer.
			string incomingData = _serialPort.ReadExisting();
			_receivedData.Append(incomingData);

			// Check for a complete message (e.g., terminated by a newline).
			int newlineIndex = _receivedData.ToString().IndexOf('\n');
			if (newlineIndex >= 0)
			{
				string completeMessage = _receivedData.ToString(0, newlineIndex + 1); // Include newline
				_receivedData.Remove(0, newlineIndex + 1); // Remove processed data

				// Raise the event on the main thread (important for UI updates).
				MainThread.BeginInvokeOnMainThread(() =>
				{
					DataReceived?.Invoke(this, completeMessage.Trim()); // Remove whitespace
				});
			}
		}
		catch (TimeoutException)
		{
			// Handle read timeout (no data received within the timeout period)
			Console.WriteLine("Read timeout occurred.");
			// Optionally:  _receivedData.Clear();
		}
		catch (Exception ex)
		{
			// Handle other exceptions (e.g., port closed unexpectedly)
			Console.WriteLine($"Data received error: {ex.Message}");
			ClosePort();
		}
	}

	private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
	{
		Console.WriteLine($"Serial port error: {e.EventType}");
		// Handle errors (e.g., device disconnected)
		ClosePort(); // Often the best action on a serious error.
	}
}