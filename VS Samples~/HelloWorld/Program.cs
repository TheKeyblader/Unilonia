using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;

namespace HelloWorld
{
#if NETCOREAPP
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
#endif
}

