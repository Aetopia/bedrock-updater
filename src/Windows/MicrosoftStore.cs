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
                    continue;
            }

            if (productId.Equals(item.ProductId, OrdinalIgnoreCase))
                value = item;
        }

        return value;
    });

    extension(AppMetadata @this)
    {
        internal async Task<InstallRequest<T>?> InstallAsync<T>(T progress) where T : IProgress<AppInstallStatus>
        {
            var item = await GetAsync(@this.ProductId);
            if (item is { }) return new(item, progress);

            if (@this.IsInstalled)
                item = await s_manager.UpdateAppByPackageFamilyNameAsync(@this.PackageFamilyName);
            else
                item = await s_manager.StartAppInstallAsync(@this.ProductId, string.Empty, false, false);

            if (item is { })
            {
                s_manager.MoveToFrontOfDownloadQueue(item.ProductId, string.Empty);
                return new(item, progress);
            }

            return null;
        }
    }
}