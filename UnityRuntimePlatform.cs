using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using UnityEngine;

namespace Unilonia
{
    internal class UnityRuntimePlatform : IRuntimePlatform
    {
        private static readonly Lazy<RuntimePlatformInfo> Info = new Lazy<RuntimePlatformInfo>(() =>
        {
            OperatingSystemType os;
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    os = OperatingSystemType.WinNT;
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    os = OperatingSystemType.OSX;
                    break;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    os = OperatingSystemType.Linux;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    os = OperatingSystemType.iOS;
                    break;
                case RuntimePlatform.Android:
                    os = OperatingSystemType.Android;
                    break;
                default:
                    os = OperatingSystemType.Unknown;
                    break;
            }

            return new RuntimePlatformInfo
            {
                IsMono = true,
                IsDesktop = os != OperatingSystemType.Android && os != OperatingSystemType.iOS,
                IsMobile = os != OperatingSystemType.Android && os != OperatingSystemType.iOS,
                IsUnix = os != OperatingSystemType.WinNT && os != OperatingSystemType.Android,
                OperatingSystem = os
            };
        });

        public RuntimePlatformInfo GetRuntimeInfo() => Info.Value;

        public IDisposable StartSystemTimer(TimeSpan interval, Action tick)
        {
            return new Timer(_ => tick(), null, interval, interval);
        }

        public IUnmanagedBlob AllocBlob(int size) => new UnmanagedBlob(this, size);

        class UnmanagedBlob : IUnmanagedBlob
        {
            private readonly UnityRuntimePlatform _plat;
            private IntPtr _address;
            private readonly object _lock = new object();
#if DEBUG
            private static readonly List<string> Backtraces = new List<string>();
            private static Thread GCThread;
            private readonly string _backtrace;
            private static readonly object _btlock = new object();

            class GCThreadDetector
            {
                ~GCThreadDetector()
                {
                    GCThread = Thread.CurrentThread;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void Spawn() => new GCThreadDetector();

            static UnmanagedBlob()
            {
                Spawn();
                GC.WaitForPendingFinalizers();
            }

#endif

            public UnmanagedBlob(UnityRuntimePlatform plat, int size)
            {
                if (size <= 0)
                    throw new ArgumentException("Positive number required", nameof(size));
                _plat = plat;
                _address = plat.Alloc(size);
                GC.AddMemoryPressure(size);
                Size = size;
#if DEBUG
                _backtrace = Environment.StackTrace;
                lock (_btlock)
                    Backtraces.Add(_backtrace);
#endif
            }

            void DoDispose()
            {
                lock (_lock)
                {
                    if (!IsDisposed)
                    {
#if DEBUG
                        lock (_btlock)
                            Backtraces.Remove(_backtrace);
#endif
                        _plat?.Free(_address, Size);
                        GC.RemoveMemoryPressure(Size);
                        IsDisposed = true;
                        _address = IntPtr.Zero;
                        Size = 0;
                    }
                }
            }

            public void Dispose()
            {
#if DEBUG
                if (Thread.CurrentThread.ManagedThreadId == GCThread?.ManagedThreadId)
                {
                    lock (_lock)
                    {
                        if (!IsDisposed)
                        {
                            Console.Error.WriteLine("Native blob disposal from finalizer thread\nBacktrace: "
                                                 + Environment.StackTrace
                                                 + "\n\nBlob created by " + _backtrace);
                        }
                    }
                }
#endif
                DoDispose();
                GC.SuppressFinalize(this);
            }

            ~UnmanagedBlob()
            {
#if DEBUG
                Console.Error.WriteLine("Undisposed native blob created by " + _backtrace);
#endif
                DoDispose();
            }

            public IntPtr Address => IsDisposed ? throw new ObjectDisposedException("UnmanagedBlob") : _address;
            public int Size { get; private set; }
            public bool IsDisposed { get; private set; }
        }

        IntPtr Alloc(int size) => Marshal.AllocHGlobal(size);
        void Free(IntPtr ptr, int len) => Marshal.FreeHGlobal(ptr);
    }
}
