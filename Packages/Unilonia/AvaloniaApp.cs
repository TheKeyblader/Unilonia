using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Unilonia.Settings;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Application = Avalonia.Application;

namespace Unilonia
{
    public static class AvaloniaApp
    {
#if UNITY_EDITOR

        public static void Load()
        {
            UniloniaSettings.Load();
            EditorApplication.playModeStateChanged += VerifySettings;
        }

        private static void VerifySettings(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                UniloniaSettings.Load();
        }
#endif

        public static void Start()
        {
            if (Application.Current == null)
            {
                var gameObject = new GameObject("AvaloniaThreading");
                gameObject.AddComponent<PlatformThreadingInterface>();

                var settings = UniloniaSettings.Load();
                if (settings.applicationType.Type == null) throw new ArgumentNullException(nameof(UniloniaSettings.applicationType));

                var builderType = typeof(AppBuilderBase<>).MakeGenericType(typeof(UnityAppBuilder));
                var configureMethod = builderType.GetMethod(nameof(UnityAppBuilder.Configure), BindingFlags.Public | BindingFlags.Static, null, new Type[0], null);
                var builder = (UnityAppBuilder)configureMethod.MakeGenericMethod(settings.applicationType.Type).Invoke(null, new object[0]);

                builder
                    .UseUnity()
                    .UseDirect2D1()
                    .UseReactiveUI()
                    .LogToUnityDebug();

                builder.SetupWithoutStarting();
            }
        }
    }
}

