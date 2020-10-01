using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;
using UnityEngine;

namespace Unilonia.Input
{
    public class ClipboardImpl : IClipboard
    {
        public Task ClearAsync()
        {
            GUIUtility.systemCopyBuffer = string.Empty;
            return Task.CompletedTask;
        }

        public Task<object> GetDataAsync(string format) => throw new PlatformNotSupportedException();

        public Task<string[]> GetFormatsAsync() => throw new PlatformNotSupportedException();

        public Task<string> GetTextAsync()
        {
            return Task.FromResult(GUIUtility.systemCopyBuffer);
        }

        public Task SetDataObjectAsync(IDataObject data) => throw new PlatformNotSupportedException();

        public Task SetTextAsync(string text)
        {
            GUIUtility.systemCopyBuffer = text;
            return Task.CompletedTask;
        }
    }
}
