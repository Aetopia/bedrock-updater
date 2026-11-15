using System.Linq;
using Windows.Management.Deployment;

namespace Bedrock.Updater.System;

static class AppManager
{
    static readonly PackageManager s_packageManager = new();

    internal static bool IsInstalled(string packageFamilyName)
    {
        var packages = s_packageManager.FindPackagesForUser(string.Empty, packageFamilyName);
        return packages.Any();
    }
}