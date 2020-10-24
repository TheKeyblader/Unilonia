using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Direct2D1;
using Packages.Unilonia;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using UnityEngine;
using UnityEngine.UI;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Factory = SharpDX.Direct2D1.Factory;
using RenderTarget = SharpDX.Direct2D1.RenderTarget;
using Resource = SharpDX.DXGI.Resource;
using Resource3D = SharpDX.Direct3D11.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using UnityTexture = UnityEngine.Texture2D;

namespace Unilonia
{
    public class ExternalRenderTarget : IExternalDirect2DRenderTargetSurface, IDisposable
    {
        private Texture2D renderedTexture;
        private Texture2D visibleTexture;
        private DeviceContext renderTarget;
        private Bitmap1 bitmap;
        private static SharpDX.Direct3D11.Device Direct3D11Device;

        private bool hasRendererTarget;

        public Size ClientSize { get; set; }
        public IntPtr TexturePtr { get; private set; }

        static ExternalRenderTarget()
        {
            var empty = new UnityTexture(1, 1);
            var emptyText = new Texture2D(empty.GetNativeTexturePtr());
            Direct3D11Device = emptyText.Device;
        }

        public void AfterDrawing()
        {
            Direct2D1Platform.Direct3D11Device.ImmediateContext.CopyResource(renderedTexture, visibleTexture);
            Direct2D1Platform.Direct3D11Device.ImmediateContext.Flush();
        }

        public void BeforeDrawing()
        {

        }

        public void DestroyRenderTarget()
        {
            renderedTexture?.Dispose();
            visibleTexture?.Dispose();
            TexturePtr = IntPtr.Zero;
            renderTarget?.Dispose();
            bitmap?.Dispose();
            hasRendererTarget = false;
        }

        public RenderTarget GetOrCreateRenderTarget()
        {
            if (!hasRendererTarget) renderTarget = Create();
            return renderTarget;
        }

        private DeviceContext Create()
        {
            renderedTexture = new Texture2D(Direct2D1Platform.Direct3D11Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = (int)ClientSize.Height,
                Width = (int)ClientSize.Width,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                Usage = ResourceUsage.Default
            });

            visibleTexture = new Texture2D(Direct2D1Platform.Direct3D11Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = (int)ClientSize.Height,
                Width = (int)ClientSize.Width,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                Usage = ResourceUsage.Default
            });

            var sharedHandle = visibleTexture.QueryInterface<Resource>().SharedHandle;
            var unityTex = Direct3D11Device.OpenSharedResource<Texture2D>(sharedHandle);
            TexturePtr = unityTex.NativePointer;

            var surface = renderedTexture.QueryInterface<Surface>();

            renderTarget = new DeviceContext(Direct2D1Platform.Direct2D1Device, DeviceContextOptions.EnableMultithreadedOptimizations)
            {
                DotsPerInch = new SharpDX.Size2F(96, 96)
            };

            bitmap = new Bitmap1(renderTarget, surface, new BitmapProperties1
            {
                BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.Target,
                DpiX = Screen.width,
                DpiY = Screen.height,
                PixelFormat = new PixelFormat
                {
                    AlphaMode = SharpDX.Direct2D1.AlphaMode.Premultiplied,
                    Format = Format.B8G8R8A8_UNorm
                }
            });

            renderTarget.Target = bitmap;
            hasRendererTarget = true;

            return renderTarget;
        }

        public void Dispose()
        {
            DestroyRenderTarget();
        }
    }
}
