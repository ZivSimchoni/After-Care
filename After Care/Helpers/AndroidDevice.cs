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
using System.Reflection;
using Windows.ApplicationModel;

namespace After_Care.Helpers;

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
        get; set;
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
            Model = formatStringText(matchesModel);
            Name = formatStringText(matchesProduct);
            Architecture = formatStringText(matchesArchitecture);
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
        Architecture = Model = Name = unkownText;
    }
}

