using System;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

        private static object _lock;
        private static Thread avaloniaThread;
        private static EventWaitHandle init;

        static AvaloniaApp()
        {
            _lock = new object();
        }

        public static void Start(Type overrideApplicationType = null)
        {
            lock (_lock)
            {
                if (Application.Current == null)
                {
                    var gameObject = new GameObject("AvaloniaThreading");
                    gameObject.AddComponent<UnityDispatcher>();

                    var settings = UniloniaSettings.Load();
                    if (settings.applicationType.Type == null && overrideApplicationType == null) throw new ArgumentNullException(nameof(UniloniaSettings.applicationType));

                    init = new EventWaitHandle(false, EventResetMode.ManualReset);
                    avaloniaThread = new Thread(AvaloniaThread);
                    avaloniaThread.Name = "Avalonia Thread";
                    avaloniaThread.Start(overrideApplicationType ?? settings.applicationType.Type);
                    init.WaitOne();
                    init.Dispose();
                }
            }
        }

        public static void AvaloniaThread(object parameters)
        {
            var applicationType = (Type)parameters;

            var builderType = typeof(AppBuilderBase<>).MakeGenericType(typeof(UnityAppBuilder));
            var configureMethod = builderType.GetMethod(nameof(UnityAppBuilder.Configure), BindingFlags.Public | BindingFlags.Static, null, new Type[0], null);
            var builder = (UnityAppBuilder)configureMethod.MakeGenericMethod(applicationType).Invoke(null, new object[0]);

            builder
                .UseUnity()
                .UseDirect2D1()
                .UseReactiveUI()
                .LogToUnityDebug();

            var lifetime = new UnityLifetime();
            builder.SetupWithLifetime(lifetime);

            init.Set();

            lifetime.Start(Array.Empty<string>());
            builder.Instance.Run(lifetime.Token);
        }

        class UnityLifetime : IControlledApplicationLifetime
        {

            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            public CancellationToken Token => _cts.Token;

            public int ExitCode { get; private set; }
            public event EventHandler<ControlledApplicationLifetimeStartupEventArgs> Startup;
            public event EventHandler<ControlledApplicationLifetimeExitEventArgs> Exit;

            public void Start(string[] args)
            {
                Startup?.Invoke(this, new ControlledApplicationLifetimeStartupEventArgs(args));
            }

            public void Shutdown(int exitCode)
            {
                ExitCode = exitCode;
                var e = new ControlledApplicationLifetimeExitEventArgs(exitCode);
                Exit?.Invoke(this, e);
                ExitCode = e.ApplicationExitCode;
                _cts.Cancel();
            }
        }
    }
}

