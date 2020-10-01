using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;
using Unilonia.Input;

namespace Avalonia
{
    public static class UnityApplicationExtensions
    {
        public static T UseUnity<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            builder.UseWindowingSubsystem(() => Unilonia.UnityPlatform.Initialize(builder.ApplicationType), "Unity");
            return builder;
        }
    }
}

namespace Unilonia
{
    class UnityPlatform : IPlatformSettings, IWindowingPlatform
    {
        private static readonly UnityPlatform s_instance = new UnityPlatform();

        public Size DoubleClickSize => new Size(4, 4);

        public TimeSpan DoubleClickTime => TimeSpan.FromSeconds(0.2);

        public static void Initialize(Type appType)
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IClipboard>().ToSingleton<ClipboardImpl>()
                .Bind<IStandardCursorFactory>().ToSingleton<CursorFactory>()
                .Bind<IKeyboardDevice>().ToSingleton<KeyboardDevice>()
                .Bind<IPlatformSettings>().ToConstant(s_instance)
                .Bind<ISystemDialogImpl>().ToSingleton<SystemDialogImpl>()
                .Bind<IWindowingPlatform>().ToConstant(s_instance)
                .Bind<IPlatformIconLoader>().ToSingleton<PlatformIconLoader>()
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
                .Bind<IRenderLoop>().ToSingleton<RenderLoop>()
                .Bind<IPlatformThreadingInterface>().ToConstant(PlatformThreadingInterface.Instance)
                .Bind<IRenderTimer>().ToConstant(PlatformThreadingInterface.Instance);

        }

        public IWindowImpl CreateEmbeddableWindow()
        {
            throw new NotSupportedException();
        }

        public IWindowImpl CreateWindow()
        {
            throw new NotSupportedException();
        }
    }
}
