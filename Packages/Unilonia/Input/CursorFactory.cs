using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform;

namespace Unilonia.Input
{
    internal class CursorFactory : IStandardCursorFactory
    {
        public IPlatformHandle GetCursor(StandardCursorType cursorType)
            => new PlatformHandle(IntPtr.Zero, "ZeroCursor");
    }
}
