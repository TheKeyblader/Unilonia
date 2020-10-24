#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Raw;
using UnityEngine;
using UnityEngine.InputSystem;
using AvaKey = Avalonia.Input.Key;
using UniKey = UnityEngine.InputSystem.Key;
using Screen = UnityEngine.Screen;
using System.Linq;
using Avalonia.Threading;
using System.Globalization;
using System;

namespace Unilonia.Input
{
    public class UnityInputSystem : MonoBehaviour
    {
        internal TopLevelImpl TopLevel { get; set; }
        private Vector2 oldPosition;
        private IKeyboardDevice keyboard;

        public void Awake()
        {
            if (Keyboard.current != null)
            {
                Keyboard.current.onTextInput += CharEvent;
            }
            keyboard = AvaloniaLocator.Current.GetService<IKeyboardDevice>();
        }

        public void Update()
        {
            ulong timestamp = (ulong)(Environment.TickCount & int.MaxValue);
            var modifiers = GetRawInputModifiers();
            if (Mouse.current != null)
            {
                var toTransform = Mouse.current.position.ReadValue();
                var newPosition = new Vector2(toTransform.x, Screen.height - toTransform.y);
                if (oldPosition != newPosition)
                {
                    oldPosition = newPosition;
                    Dispatcher.UIThread.Post(() =>
                    {
                        TopLevel.Input?.Invoke(new RawPointerEventArgs(TopLevel.MouseDevice, timestamp,
                            TopLevel.InputRoot, RawPointerEventType.Move, oldPosition.ToAvalonia(), modifiers));
                    }, DispatcherPriority.Input);

                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        TopLevel.Input?.Invoke(new RawPointerEventArgs(TopLevel.MouseDevice, timestamp,
                            TopLevel.InputRoot, RawPointerEventType.LeftButtonDown, oldPosition.ToAvalonia(), modifiers));
                    }, DispatcherPriority.Input);
                }
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        TopLevel.Input?.Invoke(new RawPointerEventArgs(TopLevel.MouseDevice, timestamp,
                            TopLevel.InputRoot, RawPointerEventType.LeftButtonUp, oldPosition.ToAvalonia(), modifiers));
                    }, DispatcherPriority.Input);
                }
            }

            if (Keyboard.current != null)
            {
                foreach (var key in Keyboard.current.allKeys)
                {
                    if (key.wasPressedThisFrame || key.wasReleasedThisFrame)
                    {
                        AvaKey? keycode = null;
                        if (displayToKeyDict.ContainsKey(key.displayName))
                            keycode = displayToKeyDict[key.displayName];
                        if (uniKeyToAvaKeyDict.ContainsKey(key.keyCode))
                            keycode = uniKeyToAvaKeyDict[key.keyCode];
                        if (keycode == null) continue;

                        if (key.wasPressedThisFrame)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                TopLevel.Input?.Invoke(new RawKeyEventArgs(keyboard, timestamp, TopLevel.InputRoot, RawKeyEventType.KeyDown, keycode.Value, modifiers));
                            }, DispatcherPriority.Input);
                        }
                        if (key.wasReleasedThisFrame)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                TopLevel.Input?.Invoke(new RawKeyEventArgs(keyboard, timestamp, TopLevel.InputRoot, RawKeyEventType.KeyUp, keycode.Value, modifiers));
                            }, DispatcherPriority.Input);
                        }
                    }
                }

            }
        }

        private void CharEvent(char character)
        {
            if (excludeChars.Contains(character)) return;
            Dispatcher.UIThread.Post(() =>
            {
                ulong timestamp = (ulong)(Environment.TickCount & int.MaxValue);
                TopLevel.Input?.Invoke(new RawTextInputEventArgs(keyboard, timestamp, TopLevel.InputRoot, new string(character, 1)));
            }, DispatcherPriority.Input);
        }

        private RawInputModifiers GetRawInputModifiers()
        {
            var modifiers = RawInputModifiers.None;

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed) modifiers |= RawInputModifiers.LeftMouseButton;
                if (Mouse.current.rightButton.isPressed) modifiers |= RawInputModifiers.RightMouseButton;
                if (Mouse.current.middleButton.isPressed) modifiers |= RawInputModifiers.MiddleMouseButton;
                if (Mouse.current.backButton.isPressed) modifiers |= RawInputModifiers.XButton1MouseButton;
                if (Mouse.current.forwardButton.isPressed) modifiers |= RawInputModifiers.XButton2MouseButton;
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.altKey.isPressed) modifiers |= RawInputModifiers.Alt;
                if (Keyboard.current.ctrlKey.isPressed) modifiers |= RawInputModifiers.Control;
                if (Keyboard.current.shiftKey.isPressed) modifiers |= RawInputModifiers.Shift;
                if (Keyboard.current.leftMetaKey.isPressed) modifiers |= RawInputModifiers.Meta;
                if (Keyboard.current.rightMetaKey.isPressed) modifiers |= RawInputModifiers.Meta;
            }

            return modifiers;
        }

        private Dictionary<string, AvaKey> displayToKeyDict = new Dictionary<string, AvaKey>()
        {
            { "A",AvaKey.A },
            { "B",AvaKey.B },
            { "C",AvaKey.C },
            { "D",AvaKey.D },
            { "E",AvaKey.E },
            { "F",AvaKey.F },
            { "G",AvaKey.G },
            { "H",AvaKey.H },
            { "I",AvaKey.I },
            { "J",AvaKey.J },
            { "K",AvaKey.K },
            { "L",AvaKey.L },
            { "M",AvaKey.M },
            { "N",AvaKey.N },
            { "O",AvaKey.O },
            { "P",AvaKey.P },
            { "Q",AvaKey.Q },
            { "R",AvaKey.R },
            { "S",AvaKey.S },
            { "T",AvaKey.T },
            { "U",AvaKey.U },
            { "V",AvaKey.V },
            { "W",AvaKey.W },
            { "X",AvaKey.X },
            { "Y",AvaKey.Y },
            { "Z",AvaKey.Z },
        };

        private Dictionary<UniKey, AvaKey> uniKeyToAvaKeyDict = new Dictionary<UniKey, AvaKey>()
        {
            {UniKey.Escape,AvaKey.Escape },
            {UniKey.Space,AvaKey.Space },
            {UniKey.Backspace, AvaKey.Back },
            {UniKey.Enter,AvaKey.Enter },
            {UniKey.NumpadEnter,AvaKey.Enter },
            {UniKey.Tab,AvaKey.Tab },
            {UniKey.F1,AvaKey.F1 },
            {UniKey.F2,AvaKey.F2 },
            {UniKey.F3,AvaKey.F3 },
            {UniKey.F4,AvaKey.F4 },
            {UniKey.F5,AvaKey.F5 },
            {UniKey.F6,AvaKey.F6 },
            {UniKey.F7,AvaKey.F7 },
            {UniKey.F8,AvaKey.F8 },
            {UniKey.F9,AvaKey.F9 },
            {UniKey.F10,AvaKey.F10 },
            {UniKey.F11,AvaKey.F11 },
            {UniKey.F12,AvaKey.F12 },
            {UniKey.Numpad0,AvaKey.NumPad0 },
            {UniKey.Numpad1,AvaKey.NumPad1 },
            {UniKey.Numpad2,AvaKey.NumPad2 },
            {UniKey.Numpad3,AvaKey.NumPad3 },
            {UniKey.Numpad4,AvaKey.NumPad4 },
            {UniKey.Numpad5,AvaKey.NumPad5 },
            {UniKey.Numpad6,AvaKey.NumPad6 },
            {UniKey.Numpad7,AvaKey.NumPad7 },
            {UniKey.Numpad8,AvaKey.NumPad8 },
            {UniKey.Numpad9,AvaKey.NumPad9 },
            {UniKey.NumpadDivide,AvaKey.Divide },
            {UniKey.Digit0,AvaKey.D0 },
            {UniKey.Digit1,AvaKey.D1 },
            {UniKey.Digit2,AvaKey.D2 },
            {UniKey.Digit3,AvaKey.D3 },
            {UniKey.Digit4,AvaKey.D4 },
            {UniKey.Digit5,AvaKey.D5 },
            {UniKey.Digit6,AvaKey.D6 },
            {UniKey.Digit7,AvaKey.D7 },
            {UniKey.Digit8,AvaKey.D8 },
            {UniKey.Digit9,AvaKey.D9 },
            {UniKey.LeftArrow,AvaKey.FnLeftArrow },
            {UniKey.RightArrow,AvaKey.FnRightArrow },
            {UniKey.UpArrow,AvaKey.FnUpArrow },
            {UniKey.DownArrow,AvaKey.FnDownArrow },
            {UniKey.Delete,AvaKey.Delete },
            {UniKey.End,AvaKey.End },
            {UniKey.Insert,AvaKey.Insert },
            {UniKey.CapsLock,AvaKey.CapsLock },
            {UniKey.NumLock,AvaKey.NumLock },
            {UniKey.PrintScreen,AvaKey.PrintScreen },
            {UniKey.Pause,AvaKey.Pause },
            {UniKey.OEM1,AvaKey.Oem1 },
            {UniKey.OEM2,AvaKey.Oem2 },
            {UniKey.OEM3,AvaKey.Oem3 },
            {UniKey.OEM4,AvaKey.Oem4 },
            {UniKey.OEM5,AvaKey.Oem5 }
        };

        private char[] excludeChars = new[] { '\b', '\t' };
    }
}
#endif
