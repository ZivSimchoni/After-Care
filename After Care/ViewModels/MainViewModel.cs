using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
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

   public MainViewModel()
    {
        Device.GetDeviceDetails();
        LoadApkFromJson();
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

    public async void InstallApkFiles()
    {
        await InstallApkFilesAsync();
    }

    public static async Task InstallApkFilesAsync()
    {
        // Get a reference to the installed location of your app
        string folderPath = Windows.ApplicationModel.Package.Current.InstalledPath;
        folderPath = folderPath.Replace(@"\bin\x86\Debug\net7.0-windows10.0.19041.0\win10-x86\AppX", @"\Helpers\apks");
        // Search for .apk files in the folder
        var apkFiles = Directory.GetFiles(folderPath, "*.apk").ToList();
        var totalFiles = apkFiles.Count;
        var processedFiles = 0;

        if (totalFiles > 0)
        {
            NotificationAndToasts.SendNotificationStartInstallation();
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
            if (processedFiles == totalFiles) { NotificationAndToasts.SendNotificationApkInstalled(); }
            else { NotificationAndToasts.SendNotificationApkFailed(); }
        }
        else
        {
            NotificationAndToasts.SendNotificationNoApkFound();
        }
    }
}
