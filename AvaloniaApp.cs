using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using TypeReferences;
using UnityEngine;
using UnityEngine.UI;
using Canvas = UnityEngine.Canvas;

namespace Unilonia
{
    [RequireComponent(typeof(PlatformThreadingInterface))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RawImage))]
    public sealed class AvaloniaApp : MonoBehaviour
    {
        [CustomInlcudeTypes]
        [InspectorName("Application Type")]
        public TypeReference applicationType;

        public void Start()
        {
            if (applicationType.Type == null) throw new ArgumentNullException("Application Type");

            var image = GetComponent<RawImage>();
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var builderType = typeof(AppBuilderBase<>).MakeGenericType(typeof(UnityAppBuilder));
            var configureMethod = builderType.GetMethod(nameof(UnityAppBuilder.Configure), BindingFlags.Public | BindingFlags.Static, null, new Type[0], null);
            var builder = (UnityAppBuilder)configureMethod.MakeGenericMethod(applicationType.Type).Invoke(null, new object[0]);

            builder
                .UseUnity()
                .UseDirect2D1()
                .UseReactiveUI()
                .LogToUnityDebug();

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
