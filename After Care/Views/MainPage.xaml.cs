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
        //dark_switch.SetValue(ToggleSwitch.IsOnProperty, true);
    }

    //void CheckBoxPage_Loaded(object sender, RoutedEventArgs e)
    //{
    //    SetCheckedState();
    //}


    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear previous returned file name, if it exists, between iterations of this scenario
        PickFolderOutputTextBlock.Text = "";

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
            PickFolderOutputTextBlock.Text = "Picked folder: " + folder.Path;
            await GetApkFilesFromFolder(folder.Path);
        }
        else
        {
            PickFolderOutputTextBlock.Text = "Operation cancelled.";
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
            textApkFilesName.Text = $"Found APK files (Total: {totalFiles}):";
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

}