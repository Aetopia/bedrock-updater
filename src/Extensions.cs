using System;
using System.Threading.Tasks;
using Bedrock.Updater.System;
using Bedrock.Updater.Windows;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.Shell.SFBS_FLAGS;

namespace Bedrock.Updater;

unsafe static class Extensions
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

    extension(in ulong @this)
    {
        bool AsBytes(uint count, out string? value)
        {
            value = null;

            var buffer = stackalloc char[(int)count];
            var result = StrFormatByteSizeEx(@this, SFBS_FLAGS_TRUNCATE_UNDISPLAYED_DECIMAL_DIGITS, buffer, count);

            value = result.Succeeded ? new(buffer) : null;
            return result != HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);
        }

        internal string AsBytes()
        {
            string? value;
            uint count = byte.MaxValue;

            while (!@this.AsBytes(count, out value))
                count += byte.MaxValue;

            return value ?? $"{@this}";
        }
    }
}