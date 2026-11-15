using System;
using System.Threading.Tasks;
using Bedrock.Updater.System;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static System.StringComparison;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;

namespace Bedrock.Updater.Windows;

static class MicrosoftStore
{
    static readonly AppInstallManager s_manager = new();

    static Task<AppInstallItem?> GetAsync(string productId) => Task.Run(() =>
    {
        AppInstallItem? value = null;

        foreach (var item in s_manager.AppInstallItems)
        {
            var status = item.GetCurrentStatus();

            switch (status.InstallState)
            {
                case Error:
                    item.Cancel();
                    continue;

                case ReadyToDownload:
                case PausedLowBattery:
                case PausedWiFiRequired:
                case PausedWiFiRecommended:
                    item.Restart();
                    break;
            }

            if (productId.Equals(item.ProductId, OrdinalIgnoreCase))
                s_manager.MoveToFrontOfDownloadQueue((value = item).ProductId, string.Empty);
        }

        return value;
    });

    internal static async Task<InstallRequest<T>?> InstallAsync<T>(AppMetadata app, T progress) where T : IProgress<AppInstallStatus>
    {
        var item = await GetAsync(app.ProductId);
        if (item is { }) return new(item, progress);

        if (app.IsInstalled)
            item = await s_manager.UpdateAppByPackageFamilyNameAsync(app.PackageFamilyName);
        else
            item = await s_manager.StartAppInstallAsync(app.ProductId, string.Empty, false, false);

        return item is { } ? new(item, progress) : null;
    }
}