using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using UnityEngine;
using UnityEngine.UI;
using AvaloniaApplication = Avalonia.Application;

namespace Unilonia
{
    [RequireComponent(typeof(PlatformThreadingInterface))]
    public class AvaloniaApp<T> : MonoBehaviour
        where T : AvaloniaApplication, new()
    {
        public RawImage image;

        public UnityAppBuilder SetupWithWindows()
        {
            return UnityAppBuilder.Configure<T>();
        }

        public void SetupWithTopLevel(Action<UnityAppBuilder> customize)
        {
            var builder = UnityAppBuilder.Configure<T>();
            customize(builder);
            var lifetime = new Lifetime();
            builder.AfterSetup(_ =>
            {
                var view = gameObject.AddComponent<TopLevelImpl>();
                view.Setup(image);
                lifetime.View = view;
            });

            builder.SetupWithLifetime(lifetime);
        }

        class Lifetime : ISingleViewApplicationLifetime
        {
            public TopLevelImpl View { get; set; }
            public Control MainView
            {
                get => View.Content;
                set => View.Content = value;
            }
        }
    }
}
