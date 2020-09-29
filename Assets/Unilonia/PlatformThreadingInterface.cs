using System;
using System.Collections;
using System.Reactive.Disposables;
using System.Threading;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using UnityEngine;

namespace Unilonia
{
    public class PlatformThreadingInterface : MonoBehaviour, IPlatformThreadingInterface, IRenderTimer
    {
        private bool _signaled;
        public static PlatformThreadingInterface Instance { get; private set; }

        public event Action<TimeSpan> Tick;

        public void Awake()
        {
            if (Instance != null) throw new InvalidOperationException("Can't instancied multiple threading platform");
            Instance = this;
        }

        public bool CurrentThreadIsLoopThread => true;

        public event Action<DispatcherPriority?> Signaled;

        public void RunLoop(CancellationToken cancellationToken)
        {

        }

        public void Update()
        {
            Tick?.Invoke(TimeSpan.FromSeconds(Time.deltaTime));
        }

        public void Signal(DispatcherPriority priority)
        {
            lock (this)
            {
                if (_signaled)
                    return;
                _signaled = true;
            }

            StartCoroutine(ToSignal());
        }

        public IEnumerator ToSignal()
        {
            yield return new WaitForFixedUpdate();
            lock (this)
                _signaled = false;
            Signaled?.Invoke(null);
        }

        public IDisposable StartTimer(DispatcherPriority priority, TimeSpan interval, Action tick)
        {
            var coroutine = StartCoroutine(TickCourotine(interval, tick));

            return Disposable.Create(() =>
            {
                StopCoroutine(coroutine);
            });
        }

        public IEnumerator TickCourotine(TimeSpan interval, Action tick)
        {
            while (true)
            {
                tick();
                yield return new WaitForSeconds((float)interval.TotalSeconds);
            }
        }
    }
}
