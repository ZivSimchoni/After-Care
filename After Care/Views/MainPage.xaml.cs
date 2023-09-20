using After_Care.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using After_Care.Helpers;

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