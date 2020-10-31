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
            builder.UseWindowingSubsystem(Unilonia.UnityPlatform.Initialize, "Unity");
            return builder;
        }
    }
}

namespace Unilonia
{
    internal class UnityPlatform : IPlatformSettings, IWindowingPlatform
    {
        private static readonly UnityPlatform s_instance = new UnityPlatform();

        public Size DoubleClickSize => new Size(4, 4);

        public TimeSpan DoubleClickTime => TimeSpan.FromSeconds(0.2);

        public static void Initialize()
        {
            var threading = new UniloniaPlatformThreadingInterface();
            AvaloniaLocator.CurrentMutable
                .Bind<IClipboard>().ToSingleton<ClipboardImpl>()
                .Bind<IStandardCursorFactory>().ToSingleton<CursorFactory>()
                .Bind<IKeyboardDevice>().ToSingleton<KeyboardDevice>()
                .Bind<ISystemDialogImpl>().ToSingleton<SystemDialogImpl>()
                .Bind<IPlatformSettings>().ToConstant(s_instance)
                .Bind<IWindowingPlatform>().ToConstant(s_instance)
                .Bind<IPlatformIconLoader>().ToSingleton<PlatformIconLoader>()
                .Bind<IPlatformThreadingInterface>().ToConstant(threading)
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
                .Bind<IRenderLoop>().ToConstant(new RenderLoop())
                .Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60));
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
