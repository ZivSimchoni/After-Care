using After_Care.Helpers;
using After_Care.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.AccessCache;

using Windows.Storage.Pickers;

using Windows.Storage;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;

namespace After_Care.Views;

public sealed partial class LocalFilesPage : Page
{
    public LocalFilesViewModel ViewModel
    {
        get;
    }

    public LocalFilesPage()
    {
        ViewModel = App.GetService<LocalFilesViewModel>();
        InitializeComponent();
    }

    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear previous returned file name, if it exists, between iterations of this scenario
        PickFolderOutputTextBlock.Text = "";
        textApkFilesName.Text = "";
        ViewModel.ApkFiles.Clear();

        // Create a folder picker
        FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = HwndExtensions.GetActiveWindow();

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a folder
        StorageFolder folder = await openPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            PickFolderOutputTextBlock.Text = folder.Path;
            await GetApkFilesFromFolder(folder.Path);
        }
        else
        {
            PickFolderOutputTextBlock.Text = ResourceExtensions.GetLocalized("FolderOperationCancelled"); ;
            textApkFilesName.Text = "";
        }
    }

    async Task GetApkFilesFromFolder(string folderPath)
    {
        var apkFiles = Directory.EnumerateFiles(folderPath, "*.apk").ToList();
        int totalFiles = apkFiles.Count;
        if (totalFiles > 0)
        {
            textApkFilesName.Text = totalFiles + ResourceExtensions.GetLocalized("FolderWithApk");
            await Task.WhenAll(apkFiles.Select(async apkFilePath =>
            {
                ViewModel.ApkFiles.Add(new CheckBox() { Content = Path.GetFileName(apkFilePath), IsEnabled = true, Name = Path.GetFileName(apkFilePath).ToString(), IsChecked = true });
                await Task.Yield();
            }));
        }
        else
        {
            textApkFilesName.Text = ResourceExtensions.GetLocalized("FolderWithNoApk");
            NotificationAndToasts.SendNotificationNoApkFound();
        }
    }

    private async void InstallApkFilesButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Device.Model.Equals(ResourceExtensions.GetLocalized("UnkownDevice")))
        {
            NotificationAndToasts.SendNotificationNoDeviceIsConnected();
        }
        else if (textApkFilesName.Text.Equals(""))
        {
            NotificationAndToasts.SendNotificationNoApkFound();
        }
        else if (!isCheckBoxSelected())
        {
            NotificationAndToasts.SendNotificationNoApkSelected();
        }
        else
        {
            await ViewModel.InstallApkFiles(PickFolderOutputTextBlock.Text);
        }
    }

    public bool isCheckBoxSelected()
    {
        // Check if any checkbox is selected
        return ViewModel.ApkFiles.Any(x => x.IsChecked == true);
    }
}
