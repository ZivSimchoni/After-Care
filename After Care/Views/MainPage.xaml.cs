using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using After_Care.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using CommunityToolkit.WinUI.UI.Converters;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI;
using After_Care.Helpers;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;

namespace After_Care.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Device.Model.Equals(ResourceExtensions.GetLocalized("UnkownDevice")))
        {
            NotificationAndToasts.SendNotificationNoDeviceIsConnected();
        }
        else if (!isCheckBoxSelected())
        {
            NotificationAndToasts.SendNotificationNoApkSelected();
        }
        else
        {
            ViewModel.InstallApkFiles();
        }
    }

    public bool isCheckBoxSelected()
    {
        // Check if any checkbox is selected
        return ViewModel.categories.Any(x => x.Value.Apps.Any(y => y.IsChecked == true));
    }
}