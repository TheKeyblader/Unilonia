﻿using Avalonia.Input;

namespace Unilonia.Input
{
    public class UnityMouseDevice : MouseDevice, IMouseDevice
    {
        public UnityMouseDevice() : base(new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true))
        {

        }
    }
}
