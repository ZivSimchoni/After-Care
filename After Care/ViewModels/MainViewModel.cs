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

    private List<string> getSelectedApksToInstall()
    {
        List<string> selectedApks = new List<string>();
        foreach (var category in Categories)
        {
            foreach (var app in category.Apps)
            {
                if (app.IsChecked)
                {
                    selectedApks.Add(app.Name);
                }
            }
        }
        return selectedApks;
    }   

    public async void InstallApkFiles()
    {
        var folderPath = Windows.ApplicationModel.Package.Current.InstalledPath;
        folderPath = folderPath.Replace(@"\bin\x86\Debug\net7.0-windows10.0.19041.0\win10-x86\AppX", @"\Helpers\apks");
        await ApkInstallerClass.InstallApkFilesAsync(folderPath, getSelectedApksToInstall(), true);
        // TODO: add clear all checkboxes
        // TODO: remove the apk files after installation
    }
}
