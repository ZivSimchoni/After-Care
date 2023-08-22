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

namespace After_Care.ViewModels;

public partial class MainViewModel : ObservableRecipient, INotifyPropertyChanged
{
    private static int _instanceCount = 0;
    private static string _deviceName;
    private static string _deviceModel;
    private static string _deviceArchitecture;

    public event PropertyChangedEventHandler PropertyChanged;

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
            var deviceFound = GetDeviceDetails(TextDeviceName, TextDeviceModel, TextDeviceArchitecture);
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
        //string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Construct the path to adb.exe within the 'adb' folder
        //string adbPath = Path.Combine(currentDirectory, "adb", "adb.exe");
        //var adbPath = @"adb\adb.exe";
        var adbPath = @"Enter_Path_Here";

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
            Console.WriteLine("adb.exe not found in " + adbPath);
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
