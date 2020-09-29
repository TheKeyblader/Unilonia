using System;
using Avalonia;
using Avalonia.ReactiveUI;
using TodoApp;
using TodoApp.Views;

namespace Exe
{
    class Program
    {
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
             AppBuilder.Configure<App>()
                .LogToDebug()
                .UsePlatformDetect()
                .UseReactiveUI();
    }
}
