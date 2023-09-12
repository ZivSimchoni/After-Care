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
        GetDeviceDetails();
    }

    // Get the device details or set the device to unknown using setDeviceUnkown() & formatStringText() methods
    public void GetDeviceDetails()
    {
        // Construct the path to adb.exe within the 'adb' folder
        var adbPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/adb/adb.exe")).AsTask().Result.Path;

        if (File.Exists(adbPath))
        {
            // Define a regular expression for repeated words.
            Regex rxProduct = new Regex(@"(ro.build.product]): \s*(.*)",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex rxModel = new Regex(@"(ro.product.model]): \s*(.*)",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex rxArchitecture = new Regex(@"(ro.odm.product.cpu.abilist]): \s*(.*)",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Start the child process.
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = adbPath,
                Arguments = "shell getprop",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            var p = Process.Start(pi);

            var text = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            // Find matches.
            MatchCollection matchesProduct = rxProduct.Matches(text);
            MatchCollection matchesModel = rxModel.Matches(text);
            MatchCollection matchesArchitecture = rxArchitecture.Matches(text);
            // format the strings and set the device details
            Device.Model = formatStringText(matchesModel);
            Device.Name = formatStringText(matchesProduct);
            Device.Architecture = formatStringText(matchesArchitecture);
        }
        else
        {
            setDeviceUnkown();
        }
    }

    // Format the string to get the device details
    private string formatStringText(MatchCollection stringToFormat)
    {
        try
        {
            return stringToFormat.First().ToString().Split(": ")[1].Replace("[", "").Replace("]", "");
        }
        catch (Exception)
        {
            // if format fails return unkown
            return ResourceExtensions.GetLocalized("UnkownDevice");
        }
    }

    // Set the device details to unkown if the device is not found
    private void setDeviceUnkown()
    {
        var unkownText = ResourceExtensions.GetLocalized("UnkownDevice");
        Device.Architecture = Device.Model = Device.Name = unkownText;
    }

    public async Task InstallApkFiles(string folderPathForLocalFiles)
    {
        await InstallApkFilesAsync(folderPathForLocalFiles, getSelectedApksToInstall());
    }

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

    public static async Task InstallApkFilesAsync(string folderPathForLocalFiles, List<string> selectedApkFiles)
    {
        if (string.IsNullOrEmpty(folderPathForLocalFiles) || selectedApkFiles.Count == 0) {return;}
        // Search for .apk files in the folder
        //var apkFiles = Directory.GetFiles(folderPathForLocalFiles, "*.apk").ToList();
        //var apkFiles = getSelectedApksToInstall();

        var totalFiles = selectedApkFiles.Count;
        var processedFiles = 0;

        if (totalFiles > 0)
        {
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
            Debug.WriteLine("Installation complete.");
        }
    }
}
