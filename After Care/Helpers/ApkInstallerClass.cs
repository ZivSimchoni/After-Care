using System.Diagnostics;

using Windows.ApplicationModel.Chat;
using Windows.Storage;

namespace After_Care.Helpers;
internal class ApkInstallerClass
{
    public static async Task InstallApkFilesAsync(string folderPathForLocalFiles, List<string> selectedApkFiles)
    {
        if (string.IsNullOrEmpty(folderPathForLocalFiles) || selectedApkFiles.Count == 0) 
        { 
            NotificationAndToasts.SendNotificationNoApkSelected();
            return; 
        }
        if (!downloadFilesUsingPythonScript(selectedApkFiles))
        {
            // TODO: add notification for failed download or something
            return;
        }    
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
                string folderPathForApksDownload = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\AfterCareApks\";
                ProcessStartInfo adbProcessInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = $"install -r \"{folderPathForApksDownload}\"",
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
            // TODO: maybe add 3rd notification for failed installs and show the failed APKs or the amount of failed APKs
            if (processedFiles == totalFiles) { NotificationAndToasts.SendNotificationApkInstalled(processedFiles); }
            else { NotificationAndToasts.SendNotificationApkFailed(); }
        }
        else
        {
            NotificationAndToasts.SendNotificationNoApkFound();
        }
    }

    public static bool downloadFilesUsingPythonScript(List<string> selectedApkFiles)
    {
        try
        {
            var pythonScriptPath = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/MainFunc.py")).AsTask().Result.Path;
            string apkFilesToPassAsArgument = "";
            foreach (var selctedApkInList in selectedApkFiles)
            {
                apkFilesToPassAsArgument += '\"' + selctedApkInList +'\"' + " ";
            }
            apkFilesToPassAsArgument = apkFilesToPassAsArgument.Substring(0, apkFilesToPassAsArgument.Length - 1);

            if (File.Exists(pythonScriptPath))
            {              
                var downloadBetaIfPresent = "True"; // beta - true or false


                

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python", // Use the Python interpreter from the system environment variable
                    Arguments = $"\"{pythonScriptPath}\" {apkFilesToPassAsArgument} {downloadBetaIfPresent}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    // You can capture the output or handle errors if needed
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    Debug.WriteLine(error);
                    // TODO: Process the output and error as needed
                }
                return true;
            }
            else return false;
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur when running the script
            // ex.Message contains the error message
            return false;
        }
    }
}
