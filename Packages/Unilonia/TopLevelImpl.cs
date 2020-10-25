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
using Avalonia.Threading;
using Unilonia.Input;
using UnityEngine;
using Rect = Avalonia.Rect;

namespace Unilonia
{

    public class TopLevelImpl : ITopLevelImpl
    {
        public Size ClientSize { get; private set; }

        public double RenderScaling => 1;

        public IEnumerable<object> Surfaces { get; private set; }

        public Action<RawInputEventArgs> Input { get; set; }
        public Action<Rect> Paint { get; set; }
        public Action<Size> Resized { get; set; }
        public Action<double> ScalingChanged { get; set; }
        public Action<WindowTransparencyLevel> TransparencyLevelChanged { get; set; }
        public Action Closed { get; set; }
        public Action LostFocus { get; set; }

        public IMouseDevice MouseDevice { get; }
        public IInputRoot InputRoot { get; private set; }

        public WindowTransparencyLevel TransparencyLevel => WindowTransparencyLevel.Transparent;

        public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => new AcrylicPlatformCompensationLevels();

        private readonly bool useDeferredRenderer;
        private readonly ExternalRenderTarget target;
        private EmbeddableControlRoot _root;

        public Control Content
        {
            get => (Control)_root.Content;
            set => _root.Content = value;
        }
        public bool DrawFPS
        {
            get => _root.Renderer.DrawFps;
            set => _root.Renderer.DrawFps = value;
        }
        public Texture Texture => target.Texture;

        public TopLevelImpl(Size clientSize, bool useDeferredRenderer)
        {
            MouseDevice = new UnityMouseDevice();
            ClientSize = clientSize;
            this.useDeferredRenderer = useDeferredRenderer;

            target = new ExternalRenderTarget()
            {
                ClientSize = ClientSize
            };
            Surfaces = new object[] { target };
        }

        public void Init()
        {
            _root = new EmbeddableControlRoot(this)
            {
                TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                Background = new SolidColorBrush(Colors.Transparent)
            };
            _root.Prepare();
            _root.Renderer.Start();
        }

        public IPopupImpl CreatePopup()
        {
            return null;
        }

        public IRenderer CreateRenderer(IRenderRoot root)
        {
            if (useDeferredRenderer)
            {
                return new DeferredRenderer(root, AvaloniaLocator.Current.GetService<IRenderLoop>());
            }
            else
            {
                return new ImmediateRenderer(root);
            }
        }

        public void Invalidate(Rect rect)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Paint?.Invoke(rect);
            }, DispatcherPriority.Render);
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

        internal void Resize(Size size)
        {
            target.DestroyRenderTarget();
            target.ClientSize = size;
            Dispatcher.UIThread.Post(() =>
            {
                Resized?.Invoke(size);
            });
        }

        internal void Close()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Closed?.Invoke();
            }).Wait();
        }

        public void Dispose()
        {
            target.Dispose();
        }
    }
}
