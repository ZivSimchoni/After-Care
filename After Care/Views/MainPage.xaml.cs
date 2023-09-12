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

    // What will happen when the user clicks the 'install' button
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        // Device is not connected
        if (ViewModel.Device.Model.Equals(ResourceExtensions.GetLocalized("UnkownDevice")))
        {
            SendNotificationToast(ResourceExtensions.GetLocalized("NoDevice"), ResourceExtensions.GetLocalized("ConnectDevice"));
        }
        // Device is connected - Check if the user has selected any apps to install
        else if (isCheckBoxSelected())
        {
            ViewModel.InstallApkFiles();
        }
        // No apps selected (via checkbox or folder)
        else
        {
            SendNotificationToast(ResourceExtensions.GetLocalized("NoApps"), ResourceExtensions.GetLocalized("CannotInstall"));
        }
    }
    
    public static bool SendNotificationToast(string title, string message)
    {
        // Notification toast to show
        var toast = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(toast);
        return toast.Id != 0;
    }

    public bool isCheckBoxSelected()
    {
        // Check if any checkbox is selected (either from the folder or from the categories) 
        // If so, return true (to install the apps) otherwise return false
        //bool anyCategory = ViewModel.categories.Any(x => x.Value.Apps.Any(y => y.IsChecked == true));
        //return  anyCategory;
        return ViewModel.categories.Any(x => x.Value.Apps.Any(y => y.IsChecked == true));
    }
}