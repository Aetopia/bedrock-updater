using System;
using Bedrock.Updater.System;
using Windows.System;

namespace Bedrock.Updater.System;

abstract class AppMetadata
{
    private protected AppMetadata() { }
    internal abstract string ProductId { get; }
    internal abstract string PackageFamilyName { get; }
}

abstract class AppMetadata<T> : AppMetadata where T : AppMetadata<T>, new()
{
    internal static readonly T _ = new();

    private protected AppMetadata()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }
}

sealed class MinecraftUWP : AppMetadata<MinecraftUWP>
{
    internal override string ProductId => "9NBLGGH2JHXJ";
    internal override string PackageFamilyName => "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
}

sealed class GamingServices : AppMetadata<GamingServices>
{
    internal override string ProductId => "9MWPM2CQNLHN";
    internal override string PackageFamilyName => "Microsoft.GamingServices_8wekyb3d8bbwe";
}

sealed class MinecraftWindowsBeta : AppMetadata<MinecraftWindowsBeta>
{
    internal override string ProductId => "9P5X4QVLC2XR";
    internal override string PackageFamilyName => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";
}