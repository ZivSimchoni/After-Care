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

namespace After_Care.ViewModels;

public partial class MainViewModel : ObservableRecipient, INotifyPropertyChanged
{
    private static int _instanceCount = 0;
    private static string _deviceName;
    private static string _deviceModel;
    private static string _deviceArchitecture;

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

    // Collection of Remote APK Files
    private ObservableCollection<CheckBox> _apkFilesRemote;
    public ObservableCollection<CheckBox> ApkFilesRemote
    {
        get
        {
            return _apkFilesRemote;
        }
        set
        {
            _apkFilesRemote = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesRemote)));
        }
    }

    // Collection of Category TextBlocks
    private ObservableCollection<TextBlock> _apkFilesCategoryRemote;
    public ObservableCollection<TextBlock> ApkFilesCategoryRemote
    {
        get
        {
            return _apkFilesCategoryRemote;
        }
        set
        {
            _apkFilesCategoryRemote = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ApkFilesCategoryRemote)));
        }
    }

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

    public MainViewModel()
    {
        // get textDeviceName details and update view
        TextDeviceModel = new TextBlock();
        TextDeviceName = new TextBlock();
        TextDeviceArchitecture = new TextBlock();

        if (_instanceCount == 0)
        {
            //var deviceFound = GetDeviceDetails(TextDeviceName, TextDeviceModel, TextDeviceArchitecture);
            var deviceFound = false; // TODO: remove later
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
        ApkFilesRemote = new ObservableCollection<CheckBox>();
        ApkFilesCategoryRemote = new ObservableCollection<TextBlock>();
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

        // Get the current directory of the application
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Construct the path to adb.exe within the 'adb' folder
        var adbPath = Path.Combine(currentDirectory, "adb", "adb.exe");
        //var adbPath = @"After Care\After Care\Helpers\adb\adb.exe";
        //var adbPath = @"C:\Users\Ziv S\source\repos\AfterCare\After Care\Helpers\adb\adb.exe";

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
            //Console.WriteLine("adb.exe not found in " + adbPath);
            return false;
        }
    }

    // Load Remote Apk Files
    /*    private void LoadApkFromJson()
        {
            //ApkFilesRemote.Add(new CheckBox() { Content = Path.GetFileName(apkFilePath), IsEnabled = true, Name = Path.GetFileName(apkFilePath).ToString(), IsChecked = true });
            // Load Json File
            // C:\Users\Ziv S\source\repos\AfterCare\After Care\bin\x86\Debug\net7.0-windows10.0.19041.0\win10-x86\AppX\Helpers\appLinkDict.json
            var jsonPath = @"C:\Users\Ziv S\source\repos\AfterCare\After Care\Helpers\appsLinkDict.json";
            var json = File.ReadAllText(jsonPath);
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            RecurseDeserialize(result);

            // Add to ApkFilesRemote
            foreach (var item in result.Values)
            {

                //ApkFilesRemote.Add(new CheckBox() { Content = Path.GetFileName(apkFilePath), IsEnabled = true, Name = Path.GetFileName(apkFilePath).ToString(), IsChecked = true });
            }

        }*/

    // Load Remote Apk Files
    private void LoadApkFromJson()
    {
        //ApkFilesRemote.Add(new CheckBox() { Content = Path.GetFileName(apkFilePath), IsEnabled = true, Name = Path.GetFileName(apkFilePath).ToString(), IsChecked = true });
        // Load Json File
        // C:\Users\Ziv S\source\repos\AfterCare\After Care\bin\x86\Debug\net7.0-windows10.0.19041.0\win10-x86\AppX\Helpers\appLinkDict.json
        var jsonPath = @"C:\Users\Ziv S\source\repos\AfterCare\After Care\Helpers\appsLinkDict.json";
        var json = File.ReadAllText(jsonPath);
        JObject data = JObject.Parse(json);
        //Dictionary<string, Dictionary<string, Dictionary<string, string>>> data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
        // Add to ApkFilesRemote
        
        foreach (var category in data)
        {
            ApkFilesCategoryRemote.Add(new TextBlock() { Name = category.Key, Text = category.Key});
            foreach (var app in category.Value)
            {
                var nameToInsert = app.Path.Replace(category.Key, "").Replace(".", "");
                ApkFilesRemote.Add(new CheckBox() { Content = nameToInsert, IsEnabled = true, Name = nameToInsert, IsChecked = false });
            }
        }

    }

    // JSON Helper
    private static void RecurseDeserialize(Dictionary<string, object> result)
    {
        //Iterate throgh key/value pairs
        foreach (var keyValuePair in result.ToArray())
        {
            //Check to see if Newtonsoft thinks this is a JArray
            var jarray = keyValuePair.Value as JArray;

            if (jarray != null)
            {
                //We have a JArray

                //Convert JArray back to json and deserialize to a list of dictionaries
                var dictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jarray.ToString());

                //Set the result as the dictionary
                result[keyValuePair.Key] = dictionaries;

                //Iterate throught the dictionaries
                foreach (var dictionary in dictionaries)
                {
                    //Recurse
                    RecurseDeserialize(dictionary);
                }
            }
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
    //

}
