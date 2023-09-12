using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using After_Care.Helpers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI.Triggers;

using Microsoft.UI.Xaml.Controls;

using Windows.Storage;

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
        await InstallApkFilesAsync(folderPathForLocalFiles, getSelectedApksToInstall());
    }

    public static async Task InstallApkFilesAsync(string folderPathForLocalFiles, List<string> selectedApkFiles)
    {
        if (string.IsNullOrEmpty(folderPathForLocalFiles) || selectedApkFiles.Count == 0) {return;}

        var totalFiles = selectedApkFiles.Count;
        var processedFiles = 0;
        if (totalFiles > 0)
        {
            NotificationAndToasts.SendNotificationStartInstallation();
            var adbPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/adb/adb.exe")).AsTask().Result.Path;
            await Task.WhenAll(selectedApkFiles.Select(async apkFilePath =>
            {
                var apkFileName = Path.GetFileName(apkFilePath);
                await Task.Yield();

                ProcessStartInfo adbProcessInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = $"install -r \"{folderPathForLocalFiles}\\{apkFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process adbProcess = new Process { StartInfo = adbProcessInfo })
                {
                    adbProcess.Start();
                    var output = await adbProcess.StandardOutput.ReadToEndAsync();
                    await adbProcess.WaitForExitAsync();
                }
                processedFiles++;
            }));
            if (processedFiles == totalFiles) { NotificationAndToasts.SendNotificationApkInstalled(processedFiles); }
            else { NotificationAndToasts.SendNotificationApkFailed(); }
        }
        else
        {
            NotificationAndToasts.SendNotificationNoApkFound();
        }
    }
}
