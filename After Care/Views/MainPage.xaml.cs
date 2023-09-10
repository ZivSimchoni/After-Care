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
        //Loaded += CheckBoxPage_Loaded;
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear previous returned file name, if it exists, between iterations of this scenario
        PickFolderOutputTextBlock.Text = "";
        textApkFilesName.Text = "";

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
            //PickFolderOutputTextBlock.Text = ResourceExtensions.GetLocalized("FolderPicked") + folder.Path;
            PickFolderOutputTextBlock.Text = folder.Path;
            await GetApkFilesFromFolder(folder.Path);
        }
        else
        {
            PickFolderOutputTextBlock.Text = ResourceExtensions.GetLocalized("OperationCancelled"); ;
            textApkFilesName.Text = "";
            ViewModel.ApkFiles.Clear();
            OptionsAllCheckBox.IsEnabled = false;
        }
    }

    async Task GetApkFilesFromFolder(string folderPath)
    {
        var apkFiles = Directory.EnumerateFiles(folderPath, "*.apk").ToList();
        int totalFiles = apkFiles.Count;
        if (totalFiles > 0)
        {
            OptionsAllCheckBox.IsEnabled = true;
            textApkFilesName.Text = $"Found APK files (Total: {totalFiles}):";
            await Task.WhenAll(apkFiles.Select(async apkFilePath =>
            {
                ViewModel.ApkFiles.Add(new CheckBox() { Content = Path.GetFileName(apkFilePath), IsEnabled = true, Name = Path.GetFileName(apkFilePath).ToString(), IsChecked = true });
                await Task.Yield();
            }));
        }
        else
        {
            textApkFilesName.Text = ResourceExtensions.GetLocalized("FolderWithNoApk");
        }
    }

    #region SelectAllMethods
    private void SelectAll_Checked(object sender, RoutedEventArgs e)
    {
        ViewModel.CheckAll();
    }

    private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
    {
        ViewModel.UnCheckAll();
    }

  
    private void SetCheckedState()
    {
        if (ViewModel.ApkFiles.All(x => x.IsChecked == true))
        {
            OptionsAllCheckBox.IsChecked = true;
        }
        else if (ViewModel.ApkFiles.All(x => x.IsChecked == false))
        {
            OptionsAllCheckBox.IsChecked = false;
        }
        else
        {
            OptionsAllCheckBox.IsChecked = false;
        }
    }

    private void Option_Checked(object sender, RoutedEventArgs e)
    {
        SetCheckedState();
    }

    private void Option_Unchecked(object sender, RoutedEventArgs e)
    {
        SetCheckedState();
    }
    #endregion

    // What will happen when the user clicks the 'install' button
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Add check if the user has selected any apps to install or Folder contains any APK files
        if (ViewModel.Device.Model.Equals(ResourceExtensions.GetLocalized("UnkownDevice")))
        {
            SendNotificationToast(ResourceExtensions.GetLocalized("NoDevice"), ResourceExtensions.GetLocalized("ConnectDevice"));
        }
        else if (isCheckBoxSelected()
                || (!(textApkFilesName.Text.Contains('0') || textApkFilesName.Text.Equals("") || PickFolderOutputTextBlock.Equals(""))))
        {
            ViewModel.InstallApkFiles(PickFolderOutputTextBlock.Text);
        }
        else
        {
            SendNotificationToast(ResourceExtensions.GetLocalized("NoApps"), ResourceExtensions.GetLocalized("CannotInstall"));
        }
    }
    
    public static bool SendNotificationToast(string title, string message)
    {
        var toast = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(toast);
        return toast.Id != 0;
    }

    public bool isCheckBoxSelected()
    {
        bool anyApkFolder = ViewModel.ApkFiles.Any(x => x.IsChecked == true);
        bool anyCategory = ViewModel.categories.Any(x => x.Value.Apps.Any(y => y.IsChecked == true));
        return anyApkFolder || anyCategory;
    }
}