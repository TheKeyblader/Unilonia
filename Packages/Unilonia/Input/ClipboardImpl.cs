using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using UnityEngine;

namespace Unilonia.Input
{
    internal class ClipboardImpl : IClipboard
    {
        public Task ClearAsync()
        {
            return UnityDispatcher.UnityThread.InvokeAsync(() =>
            {
                GUIUtility.systemCopyBuffer = string.Empty;
            });
        }

        public Task<object> GetDataAsync(string format) => throw new PlatformNotSupportedException();

        public Task<string[]> GetFormatsAsync() => throw new PlatformNotSupportedException();

        public Task<string> GetTextAsync()
        {
            return UnityDispatcher.UnityThread.InvokeAsync(() => GUIUtility.systemCopyBuffer);
        }

        public Task SetDataObjectAsync(IDataObject data) => throw new PlatformNotSupportedException();

        public Task SetTextAsync(string text)
        {
            return UnityDispatcher.UnityThread.InvokeAsync(() =>
            {
                GUIUtility.systemCopyBuffer = text;
            });
        }
    }
}
