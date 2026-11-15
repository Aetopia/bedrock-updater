using System;
using System.Threading.Tasks;
using Bedrock.Updater.System;
using Bedrock.Updater.Windows;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Bedrock.Updater;

static class Extensions
{
    extension(AppMetadata @this)
    {
        internal bool IsInstalled
        {
            get
            {
                return AppManager.IsInstalled(@this.PackageFamilyName);
            }
        }

        internal Task<InstallRequest<T>?> InstallAsync<T>(T progress) where T : IProgress<AppInstallStatus>
        {
            return MicrosoftStore.InstallAsync(@this, progress);
        }
    }

    extension(AppInstallItem @this)
    {
        internal bool IsInstalled
        {
            get
            {
                return AppManager.IsInstalled(@this.PackageFamilyName);
            }
        }
    }

    static readonly string[] s_bytes = ["B", "KB", "MB", "GB"];

    extension(in ulong @this)
    {
        internal string AsBytes()
        {
            var index = 0;

            for (; index < s_bytes.Length; index++)
                if (@this < 1U << (index + 1) * 10)
                    break;

            var value = @this / (double)(1U << index * 10);
            return $"{value:0.##} {s_bytes[index]}";
        }
    }
}