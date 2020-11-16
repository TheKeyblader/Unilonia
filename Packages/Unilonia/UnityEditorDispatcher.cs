using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Avalonia.Threading;
using UnityEditor;
using UnityEngine;

namespace Unilonia
{
    [InitializeOnLoad]
    public class UnityEditorDispatcher : IDispatcher
    {
        [ThreadStatic]
        private static bool isUnityThread;

        static UnityEditorDispatcher()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                new UnityEditorDispatcher();
            }
        }

        private readonly ConcurrentQueue<Task> _taskqueue = new ConcurrentQueue<Task>();

        public UnityEditorDispatcher()
        {
            if (UnityDispatcher.UnityThread != null) throw new InvalidOperationException("One unity dispatcher allowed !");
            UnityDispatcher.UnityThread = this;
            isUnityThread = true;

            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                    Attach();
                else if (state == PlayModeStateChange.ExitingEditMode)
                    Detach();
            };
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

        public void Detach()
        {
            UnityDispatcher.UnityThread = null;
            Update();
        }

        public void Attach()
        {
            UnityDispatcher.UnityThread = this;
        }
    }
}
