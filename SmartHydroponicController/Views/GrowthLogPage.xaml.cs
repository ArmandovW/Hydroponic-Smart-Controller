using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SmartHydroponicController.ViewModels;

namespace SmartHydroponicController.Views;

public partial class GrowthLogPage : ContentPage
{
    public GrowthLogPage(GrowthLogViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}