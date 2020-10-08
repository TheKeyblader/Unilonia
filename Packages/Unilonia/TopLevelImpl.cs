using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Unilonia.Input;
using UnityEngine;
using UnityEngine.UI;
using Canvas = UnityEngine.Canvas;
using Screen = UnityEngine.Screen;

namespace Unilonia
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(RawImage))]
    public class TopLevelImpl : MonoBehaviour, ITopLevelImpl
    {
        public Size ClientSize { get; private set; }

        public double RenderScaling => 1;

        public IEnumerable<object> Surfaces { get; private set; }

        public Action<RawInputEventArgs> Input { get; set; }
        public Action<Avalonia.Rect> Paint { get; set; }
        public Action<Size> Resized { get; set; }
        public Action<double> ScalingChanged { get; set; }
        public Action<WindowTransparencyLevel> TransparencyLevelChanged { get; set; }
        public Action Closed { get; set; }
        public Action LostFocus { get; set; }

        public IMouseDevice MouseDevice { get; }
        public IInputRoot InputRoot { get; private set; }

        public WindowTransparencyLevel TransparencyLevel => WindowTransparencyLevel.Transparent;

        public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => new AcrylicPlatformCompensationLevels();

        private EmbeddableControlRoot _root;
        public Control Content
        {
            get => (Control)_root.Content;
            set => _root.Content = value;
        }

        public TopLevelImpl()
        {
            MouseDevice = new UnityMouseDevice();
            ClientSize = new Size(Screen.width, Screen.height);
        }

        public ExternalRenderTarget Target;

        public void Setup()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            var image = gameObject.GetComponent<RawImage>();
            Target = new ExternalRenderTarget(image);
            Surfaces = new object[] { Target };

#if ENABLE_INPUT_SYSTEM
            var input = gameObject.AddComponent<UnityInputSystem>();
            input.TopLevel = this;
#endif

            _root = new EmbeddableControlRoot(this);
            _root.TransparencyLevelHint = WindowTransparencyLevel.Transparent;
            _root.Background = new SolidColorBrush(Colors.Transparent);
            _root.Prepare();
        }

        public void LateUpdate()
        {
            if (ClientSize.Width != Screen.width || ClientSize.Height != Screen.height)
            {
                ClientSize = new Size(Screen.width, Screen.height);
                Target.DestroyRenderTarget();
                Resized?.Invoke(ClientSize);
            }
            Paint?.Invoke(new Avalonia.Rect(0, 0, Screen.width, Screen.height));
        }

        public void OnDestroy()
        {
            this.Dispose();
        }

        public IPopupImpl CreatePopup()
        {
            return null;
        }

        public IRenderer CreateRenderer(IRenderRoot root) => new DeferredRenderer(root,
                AvaloniaLocator.Current.GetService<IRenderLoop>());

        public void Dispose()
        {
            Target?.Dispose();
            Target = null;
        }

        public void Invalidate(Avalonia.Rect rect)
        {
            //NO-OP
        }

        public Point PointToClient(PixelPoint point)
        {
            return point.ToPoint(1);
        }

        public PixelPoint PointToScreen(Point point)
        {
            return new PixelPoint((int)point.X, (int)point.Y);
        }

        public void SetCursor(IPlatformHandle cursor)
        {
            //NO-OP
        }

        public void SetInputRoot(IInputRoot inputRoot)
        {
            InputRoot = inputRoot;
        }

        public void SetTransparencyLevelHint(WindowTransparencyLevel transparencyLevel)
        {
            //NO-OP
        }
    }
}
