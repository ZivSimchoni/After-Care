using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using ColorCode.Compilation.Languages;
using Microsoft.Windows.ApplicationModel.Resources;
using CommunityToolkit.WinUI.UI.Controls;
using WinUIEx.Messaging;
using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using After_Care.Helpers;

namespace After_Care.ViewModels;


public class Category
{
    public string Name { get; set; }
    public List<CheckBoxItem> Apps { get; set; } = new List<CheckBoxItem>();
}

public class CheckBoxItem
{
    public string Name { get; set; }
    public string Icon { get; set; } // Icon URL from the web
    public bool IsChecked { get; set; }
}

public class AndroidDevice
{
    public string Name
    {
        get; set;
    }
    public string Model
    {
        get; set;
    }
    public string Architecture
    {
        get;set;
    }
}

public partial class MainViewModel : ObservableRecipient, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public AndroidDevice Device { get; set; } = new AndroidDevice();

    public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

    public Dictionary<string, Category> categories = new Dictionary<string, Category>
{
    { "Web-Browsers", new Category { Name = "Web Browsers" } },
    { "Email-Clients", new Category { Name = "Email Clients" } },
    { "Files-And-Utilities", new Category { Name = "Files and Utilities" } },
    { "Navigation-Apps", new Category { Name = "Navigation Apps" } },
    { "Media-And-Social", new Category { Name = "Media and Social" } },
    { "Social-And-Messeging", new Category { Name = "Social and Messaging" } },
    { "Network-And-Ad-Blockers", new Category { Name = "Network and Ad Blockers" } },
    { "Alternative-Stores", new Category { Name = "Alternative Stores" } },
    { "Anime", new Category { Name = "Anime" } },
};

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

    public MainViewModel()
    {
        GetDeviceDetails();
        ApkFiles = new ObservableCollection<CheckBox>();
        LoadApkFromJson();
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

    // Load Remote Apk Files
    private void LoadApkFromJson()
    {
        // Load Json File
        var jsonPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/appsLinkDict.json")).AsTask().Result.Path; ;
        var json = File.ReadAllText(jsonPath);
        JObject data = JObject.Parse(json);
        // Add to ObservableCollections
        foreach (var category in data)
        {
            if (categories.TryGetValue(category.Key, out Category categoryInfo))
            {
                foreach (var app in category.Value)
                {
                    var nameToInsert = app.Path.Replace(category.Key, "").Replace(".", "");
                    //var iconUrl = app.ElementAt(0)["icon"].ToString(Formatting.None).Substring(1);
                    try
                    {
                        var iconDir = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + nameToInsert.Replace("-", " ") + ".png")).AsTask().Result.Path;
                        categoryInfo.Apps.Add(new CheckBoxItem { Name = nameToInsert, Icon = iconDir, IsChecked = false});
                    }
                    catch (Exception) 
                    {
                        Debug.WriteLine(nameToInsert);
                    }
                    
                }
            }
        }
        foreach (var categoryInfo in categories.Values)
        {
            Categories.Add(categoryInfo);
        }
    }

    // Install Apk Files
    public void InstallApkFiles(string folderPath)    
    {
        InstallApkFilesAsync(folderPath).Wait();
    }

    public static async Task InstallApkFilesAsync(string folderPath)
    {
        // TODO: Fix this
        var apkFiles = Directory.EnumerateFiles(folderPath, "*.apk").ToList();
        var totalFiles = apkFiles.Count;
        var processedFiles = 0;

        if (totalFiles > 0)
        {
            var adbPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/adb/adb.exe")).AsTask().Result.Path;
            await Task.WhenAll(apkFiles.Select(async apkFilePath =>
            {
                var apkFileName = Path.GetFileName(apkFilePath);
                await Task.Yield();

                ProcessStartInfo adbProcessInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = $"install -r \"{apkFilePath}\"",
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
                    Debug.WriteLine($"Installed: {apkFileName}");
                    Debug.WriteLine(output);
                }
                processedFiles++;
            }));
            Debug.WriteLine("Installation complete.");
        }
        else
        { // TODO: Add stuff here or remove
            
        }
    }
}
