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
        public RawImage Image { get; set; }

        private Texture2D renderedTexture;
        private Texture2D visibleTexture;
        private DeviceContext renderTarget;
        private static SharpDX.Direct3D11.Device Direct3D11Device;
        public Size ScreenSize { get; set; }

        static ExternalRenderTarget()
        {
            var empty = new UnityTexture(1, 1);
            var emptyText = new Texture2D(empty.GetNativeTexturePtr());
            Direct3D11Device = emptyText.Device;
        }

        public ExternalRenderTarget(RawImage image)
        {
            Image = image;
        }

        public void AfterDrawing()
        {
            Direct2D1Platform.Direct3D11Device.ImmediateContext.CopyResource(renderedTexture, visibleTexture);
        }

        public void BeforeDrawing()
        {

        }

        public void DestroyRenderTarget()
        {
            UnityDispatcher.UnityThread.Post(() =>
            {
                UnityEngine.Object.Destroy(Image.texture);
                Image.texture = null;
            });
            renderedTexture.Dispose();
            renderedTexture = null;
            visibleTexture.Dispose();
            visibleTexture = null;
            renderTarget.Dispose();
            renderTarget = null;
        }

        public RenderTarget GetOrCreateRenderTarget()
        {
            if (renderTarget == null) renderTarget = Create();
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
                Height = (int)ScreenSize.Height,
                Width = (int)ScreenSize.Width,
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
                Height = (int)ScreenSize.Height,
                Width = (int)ScreenSize.Width,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                Usage = ResourceUsage.Default
            });

            UnityDispatcher.UnityThread.Post(() =>
            {
                var unityRes = Direct3D11Device.OpenSharedResource<Resource>(visibleTexture.QueryInterface<Resource>().SharedHandle);
                var resourceShader = new ShaderResourceView(Direct3D11Device, unityRes.QueryInterface<Resource3D>());

                var tex = UnityTexture.CreateExternalTexture(Screen.width, Screen.height, TextureFormat.BGRA32, false, true, resourceShader.NativePointer);
                tex.hideFlags = HideFlags.DontSave;
                Image.texture = tex;
                Image.uvRect = new UnityEngine.Rect(0, 1, 1, -1);
                Image.color = Color.white;
                Image.texture.filterMode = FilterMode.Trilinear;
            });

            var surface = renderedTexture.QueryInterface<Surface>();

            renderTarget = new DeviceContext(Direct2D1Platform.Direct2D1Device, DeviceContextOptions.EnableMultithreadedOptimizations)
            {
                DotsPerInch = new SharpDX.Size2F(96, 96)
            };

            var bitmap = new Bitmap1(renderTarget, surface, new BitmapProperties1
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

            return renderTarget;
        }

        public void Dispose()
        {
            DestroyRenderTarget();
        }
    }
}
