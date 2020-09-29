﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TodoApp.Views;

namespace TodoApp
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            if (ApplicationLifetime is ISingleViewApplicationLifetime single)
            {
                single.MainView = new MainView();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
