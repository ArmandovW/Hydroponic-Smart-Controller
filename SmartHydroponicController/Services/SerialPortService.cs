using System.IO.Ports;
using System.Text;

namespace SmartHydroponicController.Services;

public class SerialPortService
{
    private SerialPort _serialPort;
    private StringBuilder _receivedData = new StringBuilder();
    public event EventHandler<string> DataReceived;

    public SerialPortService()
    {
        try
        {
            OpenPort("COM3", 9600);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public string[] GetPortNames()
    {
        return GetAndroidSerialPorts(); // Implementation below
    }
    private string[] GetAndroidSerialPorts()
    {
        // This is a *simplified* example.  A full implementation involves:
        // 1. Getting the UsbManager from the Android context.
        // 2. Enumerating connected USB devices.
        // 3. Checking if each device is a serial device.
        // 4. Requesting permission to access the device if needed.
        // 5. Getting the device's interface and endpoints.
        // ... It's quite involved.  This example *might* work for some simple cases.

        List<string> portNames = new List<string>();
        try{
            var usbManager = (Android.Hardware.Usb.UsbManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.UsbService);
            if (usbManager != null)
            {
                var deviceList = usbManager.DeviceList;
                foreach (var device in deviceList.Values)
                {
                   
                    //Very simplified check to see if it's a serial device
                    if(device.InterfaceCount > 0)
                    {
                        for (int i = 0; i < device.InterfaceCount; i++)
                        {
                            var usbInterface = device.GetInterface(i);
                            if(usbInterface.InterfaceClass == Android.Hardware.Usb.UsbClass.CdcData || usbInterface.InterfaceClass == Android.Hardware.Usb.UsbClass.VendorSpec) //Common serial classes
                            {
                                portNames.Add(device.DeviceName);
                                break; // Assume only one serial interface per device
                            }
                        }
                    }
                    
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error getting usb device list {ex.Message}");
        }
        return portNames.ToArray();
    }
    
    public bool OpenPort(string portName, int baudRate = 9600)
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
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
            catch(Exception ex)
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