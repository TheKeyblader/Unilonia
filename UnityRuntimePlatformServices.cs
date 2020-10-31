using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;

namespace Unilonia
{
    internal static class UnityRuntimePlatformServices
    {
        public static void Register(Assembly assembly = null)
        {
            var standardPlatform = new UnityRuntimePlatform();
            AssetLoader.RegisterResUriParsers();
            AvaloniaLocator.CurrentMutable
                .Bind<IRuntimePlatform>().ToConstant(standardPlatform)
                .Bind<IAssetLoader>().ToConstant(new AssetLoader(assembly));
        }
    }
}
