using System.Linq;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Management.Deployment;

namespace Bedrock.Updater.System;

static class AppManager
{
    static readonly PackageManager s_packageManager = new();

    static bool IsPackageInstalled(string packageFamilyName)
    {
        var packages = s_packageManager.FindPackagesForUser(string.Empty, packageFamilyName);
        return packages.Any();
    }

    extension(AppMetadata @this)
    {
        internal bool IsInstalled => IsPackageInstalled(@this.PackageFamilyName);
    }

    extension(AppInstallItem @this)
    {
        internal bool IsInstalled => IsPackageInstalled(@this.PackageFamilyName);
    }
}