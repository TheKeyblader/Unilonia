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
    [InitializeOnLoad]
    public static class AvaloniaApp
    {
#if UNITY_EDITOR
        static AvaloniaApp()
        {
            var settings = UniloniaSettings.Load();
            EditorApplication.playModeStateChanged += VerifySettings;
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    Stop();
                }
            };
            UnityEngine.Application.quitting += Stop;
            if (settings.applicationType.Type != null)
                Start();
        }
        private static void VerifySettings(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                UniloniaSettings.Load();
        }
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void SelfStart()
        {
            Start();
        }

        private static readonly object _lock = new object();
        private static Thread avaloniaThread;
        private static EventWaitHandle init;

        public static void Start()
        {
            if (UnityEngine.Application.isPlaying && UnityEngine.Object.FindObjectOfType<UnityDispatcher>() == null)
            {
                var gameObject = new GameObject("AvaloniaThreading");
                gameObject.AddComponent<UnityDispatcher>();
            }

            lock (_lock)
            {
                if (Application.Current == null)
                {
                    var settings = UniloniaSettings.Load();
                    if (settings.applicationType.Type == null) throw new ArgumentNullException(nameof(UniloniaSettings.applicationType));

                    init = new EventWaitHandle(false, EventResetMode.ManualReset);
                    avaloniaThread = new Thread(AvaloniaThread);
                    avaloniaThread.Name = "Avalonia Thread";
                    avaloniaThread.Start(settings.applicationType.Type);
                    if (UnityEngine.Application.isPlaying)
                        init.WaitOne();
                    init.Dispose();
                    init = null;
                }
            }
        }

        public static void Stop() => Stop(0);
        public static void Stop(int exitCode = 0)
        {
            if (Application.Current != null)
            {
                var lifetime = Application.Current.ApplicationLifetime as IControlledApplicationLifetime;
                lifetime.Shutdown(exitCode);
            }
        }

        private static void AvaloniaThread(object parameters)
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

            if (init != null)
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
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                _cts.Cancel();
                UnityEngine.Application.Quit(exitCode);                
#endif
            }
        }
    }
}

