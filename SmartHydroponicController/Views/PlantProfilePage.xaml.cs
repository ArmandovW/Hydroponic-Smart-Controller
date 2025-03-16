using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SmartHydroponicController.ViewModels;

namespace SmartHydroponicController.Views;

public partial class PlantProfilePage : ContentPage
{
    public PlantProfilePage(PlantProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}