using System.Collections.ObjectModel;
using System.ComponentModel;
using After_Care.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace After_Care.ViewModels;

public partial class LocalFilesViewModel : ObservableRecipient, INotifyPropertyChanged
{
    public AndroidDevice Device { get; set; } = new AndroidDevice();
    public event PropertyChangedEventHandler PropertyChanged;
    // Collection of Local APK Files
    private ObservableCollection<CheckBox> _apkFiles;
    public ObservableCollection<CheckBox> ApkFiles
    {
        get
        {
            return _apkFiles;
        }
        set
        {
            _apkFiles = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFiles)));
        }
    }

    public LocalFilesViewModel()
    {
        ApkFiles = new ObservableCollection<CheckBox>();
        Device.GetDeviceDetails();
    }

    // Helper method to get the selected APKs to install since its static and can't be accessed easily
    private List<string> getSelectedApksToInstall()
    {
        var selectedApks = new List<string>();
        foreach (var apk in ApkFiles)
        {
            if (apk.IsChecked == true)
            {
                selectedApks.Add(apk.Content.ToString());
            }
        }
        return selectedApks;
    }

    public async Task InstallApkFiles(string folderPathForLocalFiles)
    {
        await ApkInstallerClass.InstallApkFilesAsync(folderPathForLocalFiles, getSelectedApksToInstall());
    }
}
