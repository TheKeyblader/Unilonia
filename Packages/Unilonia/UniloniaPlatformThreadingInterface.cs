using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unilonia
{
    public class UniloniaPlatformThreadingInterface : IPlatformThreadingInterface
    {
        public UniloniaPlatformThreadingInterface()
        {
            TlsCurrentThreadIsLoopThread = true;
        }

        private readonly AutoResetEvent _signaled = new AutoResetEvent(false);

        public void RunLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Signaled?.Invoke(null);
                _signaled.WaitOne();
            }
        }

        class TimerImpl : IDisposable
        {
            private readonly DispatcherPriority _priority;
            private readonly TimeSpan _interval;
            private readonly Action _tick;
            private Timer _timer;
            private GCHandle _handle;

            public TimerImpl(DispatcherPriority priority, TimeSpan interval, Action tick)
            {
                _priority = priority;
                _interval = interval;
                _tick = tick;
                _timer = new Timer(OnTimer, null, interval, TimeSpan.FromMilliseconds(-1));
                _handle = GCHandle.Alloc(_timer);
            }

            private void OnTimer(object state)
            {
                if (_timer == null)
                    return;
                Dispatcher.UIThread.Post(() =>
                {

                    if (_timer == null)
                        return;
                    _tick();
                    _timer?.Change(_interval, TimeSpan.FromMilliseconds(-1));
                });
            }


            public void Dispose()
            {
                _handle.Free();
                _timer.Dispose();
                _timer = null;
            }
        }

        public IDisposable StartTimer(DispatcherPriority priority, TimeSpan interval, Action tick)
        {
            return new TimerImpl(priority, interval, tick);
        }

        public void Signal(DispatcherPriority prio)
        {
            _signaled.Set();
        }

        [ThreadStatic] private static bool TlsCurrentThreadIsLoopThread;

        public bool CurrentThreadIsLoopThread => TlsCurrentThreadIsLoopThread;
        public event Action<DispatcherPriority?> Signaled;
        public event Action<TimeSpan> Tick;

    }
}
