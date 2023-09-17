using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;

namespace After_Care.Helpers;

internal class NotificationAndToasts
{
    public static bool SendNotificationToast(string title, string message)
    {
        var toast = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(toast);
        return toast.Id != 0;
    }

    public static void SendNotificationNoDeviceIsConnected()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("NoDevice"), ResourceExtensions.GetLocalized("ConnectDevice"));
    }

    public static void SendNotificationStartDownload()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("StartDownloadTitle"), ResourceExtensions.GetLocalized("StartDownloadMessage"));
    }

    public static void SendNotificationNoApkFound()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("NoApps"), ResourceExtensions.GetLocalized("CannotInstall"));
    }

    public static void SendNotificationNoApkSelected()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("NoAppSelected"), ResourceExtensions.GetLocalized("CannotInstall"));
    }

    public static void SendNotificationApkInstalled()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("InstallationComplete"), ResourceExtensions.GetLocalized("ApkInstallationCompleted"));
    }

    public static void SendNotificationApkInstalled(string nameOfInstalledAPK)
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("InstallationComplete"), nameOfInstalledAPK + " " + ResourceExtensions.GetLocalized("ApkInstallationCompleted"));
    }

    public static void SendNotificationApkInstalled(int apkInstalledCounter)
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("InstallationComplete"), apkInstalledCounter + ResourceExtensions.GetLocalized("ApkInstallationCompletedWithCounter"));
    }

    public static void SendNotificationApkFailed()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("InstallationDidNotComplete"), ResourceExtensions.GetLocalized("ApkInstallationFailed"));
    }

    public static void SendNotificationStartInstallation()
    {
        SendNotificationToast(ResourceExtensions.GetLocalized("StartInstallationTitle"), ResourceExtensions.GetLocalized("StartInstallationBody"));
    }
}
