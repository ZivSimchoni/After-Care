using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using After_Care.Views;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

using Windows.Services.Maps.LocalSearch;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using After_Care.Core.Helpers;
using Newtonsoft.Json.Linq;
using ColorCode.Compilation.Languages;
using Microsoft.Windows.ApplicationModel.Resources;
using CommunityToolkit.WinUI.UI.Controls;
using WinUIEx.Messaging;

namespace After_Care.ViewModels;

public class Category
{
    public string Name { get; set; }
    public List<CheckBox> Apps { get; set; } = new List<CheckBox>();
}

public partial class MainViewModel : ObservableRecipient, INotifyPropertyChanged
{
    private static int _instanceCount = 0;
    private static string _deviceName;
    private static string _deviceModel;
    private static string _deviceArchitecture;

    public event PropertyChangedEventHandler PropertyChanged;



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
    // Device Details
    public TextBlock TextDeviceModel
    {
        get; set;
    }
    public TextBlock TextDeviceName
    {
        get; set;
    }
    public TextBlock TextDeviceArchitecture
    {
        get; set;
    }
    // Collection of One Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesWebBrowsers;
    public ObservableCollection<CheckBox> ApkFilesWebBrowsers
    {
        get
        {
            return _apkFilesWebBrowsers;
        }
        set
        {
            _apkFilesWebBrowsers = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesWebBrowsers)));
        }
    }
    // Collection of Two Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesEmail;
    public ObservableCollection<CheckBox> ApkFilesEmail
    {
        get
        {
            return _apkFilesEmail;
        }
        set
        {
            _apkFilesEmail = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesEmail)));
        }
    }
    // Collection of Three Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesFilesAndUtils;
    public ObservableCollection<CheckBox> ApkFilesFilesAndUtils
    {
        get
        {
            return _apkFilesFilesAndUtils;
        }
        set
        {
            _apkFilesFilesAndUtils = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesFilesAndUtils)));
        }
    }
    // Collection of Four Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesNavigation;
    public ObservableCollection<CheckBox> ApkFilesNavigation
    {
        get
        {
            return _apkFilesNavigation;
        }
        set
        {
            _apkFilesNavigation = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesNavigation)));
        }
    }
    // Collection of Five Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesMedia;
    public ObservableCollection<CheckBox> ApkFilesMedia
    {
        get
        {
            return _apkFilesMedia;
        }
        set
        {
            _apkFilesMedia = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesMedia)));
        }
    }
    // Collection of Six Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesSocialMesseging;
    public ObservableCollection<CheckBox> ApkFilesSocialMesseging
    {
        get
        {
            return _apkFilesSocialMesseging;
        }
        set
        {
            _apkFilesSocialMesseging = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesSocialMesseging)));
        }
    }
    // Collection of Seven Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesNetworkAndAdBlocker;
    public ObservableCollection<CheckBox> ApkFilesNetworkAndAdBlocker
    {
        get
        {
            return _apkFilesNetworkAndAdBlocker;
        }
        set
        {
            _apkFilesNetworkAndAdBlocker = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesNetworkAndAdBlocker)));
        }
    }
    // Collection of Eight Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesAltStores;
    public ObservableCollection<CheckBox> ApkFilesAltStores
    {
        get
        {
            return _apkFilesAltStores;
        }
        set
        {
            _apkFilesAltStores = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesAltStores)));
        }
    }
    // Collection of Nine Category of APK Files
    private ObservableCollection<CheckBox> _apkFilesAnime;
    public ObservableCollection<CheckBox> ApkFilesAnime
    {
        get
        {
            return _apkFilesAnime;
        }
        set
        {
            _apkFilesAnime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesAnime)));
        }
    }


    public MainViewModel()
    {
        // get textDeviceName details and update view
        TextDeviceModel = new TextBlock();
        TextDeviceName = new TextBlock();
        TextDeviceArchitecture = new TextBlock();

        if (_instanceCount == 0)
        {
            var deviceFound = GetDeviceDetails(TextDeviceName, TextDeviceModel, TextDeviceArchitecture);
            //var deviceFound = false; // TODO: remove later
            if (deviceFound)
            {
                _instanceCount++;
                _deviceName = TextDeviceName.Text;
                _deviceModel = TextDeviceModel.Text;
                _deviceArchitecture = TextDeviceArchitecture.Text;
            }
            else
            {
                TextDeviceName.Text = "Unknown";
                TextDeviceModel.Text = "Unknown";
                TextDeviceArchitecture.Text = "Unknown";
            }
        }
        else
        {
            TextDeviceModel.Text = _deviceModel;
            TextDeviceName.Text = _deviceName;
            TextDeviceArchitecture.Text = _deviceArchitecture;
            TextDeviceArchitecture.UpdateLayout();
            TextDeviceModel.UpdateLayout();
            TextDeviceName.UpdateLayout();
        }
        ApkFiles = new ObservableCollection<CheckBox>();
        ApkFilesWebBrowsers = new ObservableCollection<CheckBox>();
        ApkFilesEmail = new ObservableCollection<CheckBox>();
        ApkFilesFilesAndUtils = new ObservableCollection<CheckBox>();
        ApkFilesNavigation = new ObservableCollection<CheckBox>();
        ApkFilesMedia = new ObservableCollection<CheckBox>();
        ApkFilesSocialMesseging = new ObservableCollection<CheckBox>();
        ApkFilesNetworkAndAdBlocker = new ObservableCollection<CheckBox>();
        ApkFilesAltStores = new ObservableCollection<CheckBox>();
        ApkFilesAnime = new ObservableCollection<CheckBox>();
        LoadApkFromJson();
    }

    static bool GetDeviceDetails(TextBlock name, TextBlock model, TextBlock arch)
    {
        // Define a regular expression for repeated words.
        Regex rxProduct = new Regex(@"(ro.build.product]): \s*(.*)",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex rxModel = new Regex(@"(ro.product.model]): \s*(.*)",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex rxArchitecture = new Regex(@"(ro.odm.product.cpu.abilist]): \s*(.*)",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);
        // Construct the path to adb.exe within the 'adb' folder
        var adbPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/adb/adb.exe")).AsTask().Result.Path;
        //StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
        //string directoryPath = Path.Combine(installedLocation.Path, "Helpers", "adb");
        //string adbPath = Path.Combine(directoryPath, "adb.exe");

        if (File.Exists(adbPath))
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = adbPath,
                Arguments = "shell getprop",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var p = Process.Start(pi);

            var text = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            // Find matches.
            MatchCollection matchesProduct = rxProduct.Matches(text);
            MatchCollection matchesModel = rxModel.Matches(text);
            MatchCollection matchesArchitecture = rxArchitecture.Matches(text);

            // store easily 
            var textDeviceName = "";
            var textDeviceModel = "";
            var textDeviceArchitecture = "";
            // Report the number of matches found.
            try
            {
                textDeviceModel = matchesModel.First().ToString().Split(": ")[1].Replace("[", "").Replace("]", "");
                textDeviceName = matchesProduct.First().ToString().Split(": ")[1].Replace("[", "").Replace("]", "");
                //Console.WriteLine("Device Codename: {0}", textDeviceName);
                textDeviceArchitecture = matchesArchitecture.First().ToString().Split(": ")[1].Replace("[", "").Replace("]", "");
                model.Text = textDeviceModel;
                name.Text = textDeviceName;
                arch.Text = textDeviceArchitecture;
                model.UpdateLayout();
                name.UpdateLayout();
                arch.UpdateLayout();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
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
                    categoryInfo.Apps.Add(new CheckBox { Content = nameToInsert, IsEnabled = true, Name = nameToInsert, IsChecked = false });
                }
            }
        }
        foreach (var categoryInfo in categories.Values)
        {
            Categories.Add(categoryInfo);
        }
    }



    // CheckBoxes Logic
    public void CheckAll()
    {
        if (ApkFiles == null){return;}

        foreach (var item in ApkFiles)
        {
            item.IsChecked = true;
        }
    }

    public void UnCheckAll()
    {
        if (ApkFiles == null) { return; }

        foreach (var item in ApkFiles)
        {
            item.IsChecked = false;
        }
    }
}
