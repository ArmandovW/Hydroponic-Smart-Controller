using Microsoft.Maui.Controls;

namespace SmartHydroponicController;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        AppConfig.InitializeRoutes();
    }
}