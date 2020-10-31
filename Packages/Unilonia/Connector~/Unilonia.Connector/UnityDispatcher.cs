using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace Unilonia
{
    public class UnityDispatcher : MonoBehaviour, IDispatcher
    {
        public static UnityDispatcher UnityThread { get; private set; }

        [ThreadStatic]
        private static bool isUnityThread;

        private ConcurrentQueue<Task> _taskqueue = new ConcurrentQueue<Task>();

        public UnityDispatcher()
        {
            if (UnityThread != null) throw new InvalidOperationException("One unity dispatcher allowed !");
            UnityThread = this;
            isUnityThread = true;
        }

        public bool CheckAccess() => isUnityThread;

        public void VerifyAccess()
        {
            if (!CheckAccess())
                throw new InvalidOperationException("Call from invalid thread");
        }

        public void Post(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _taskqueue.Enqueue(new Task(action));
        }

        public Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var task = new Task(action);
            _taskqueue.Enqueue(task);
            return task;
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var task = new Task<TResult>(function);
            _taskqueue.Enqueue(task);
            return task;
        }

        public Task InvokeAsync(Func<Task> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var task = new Task<Task>(function);
            _taskqueue.Enqueue(task);
            return task.Unwrap();
        }

        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> function, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var task = new Task<Task<TResult>>(function);
            _taskqueue.Enqueue(task);
            return task.Unwrap();
        }

        public void Update()
        {
            while (_taskqueue.TryDequeue(out var task))
            {
                try
                {
                    task.RunSynchronously();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}
