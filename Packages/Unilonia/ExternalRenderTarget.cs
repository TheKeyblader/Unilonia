using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Direct2D1;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using UnityEngine;
using UnityEngine.UI;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Factory = SharpDX.Direct2D1.Factory;
using RenderTarget = SharpDX.Direct2D1.RenderTarget;
using Resource = SharpDX.DXGI.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using UnityTexture = UnityEngine.Texture2D;

namespace Unilonia
{
    public class ExternalRenderTarget : IExternalDirect2DRenderTargetSurface, IDisposable
    {
        public RawImage Image { get; set; }

        private Texture2D texture;
        private DeviceContext renderTarget;
        private SharpDX.Direct2D1.Device Direct2D1Device;
        private SharpDX.Direct3D11.Device Direct3D11Device;
        private SharpDX.DXGI.Device1 DxgiDevice;

        public ExternalRenderTarget(RawImage image)
        {
            Image = image;

            var empty = new UnityTexture(1, 1);
            var emptyText = new Texture2D(empty.GetNativeTexturePtr());
            Direct3D11Device = emptyText.Device;
            DxgiDevice = Direct3D11Device.QueryInterface<SharpDX.DXGI.Device1>();
            Direct2D1Device = new SharpDX.Direct2D1.Device(Direct2D1Platform.Direct2D1Factory, DxgiDevice);
        }

        public void AfterDrawing()
        {

        }

        public void BeforeDrawing()
        {

        }

        public void DestroyRenderTarget()
        {
            UnityEngine.Object.Destroy(Image.texture);
            Image.texture = null;
            texture.Dispose();
            texture = null;
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
            texture = new Texture2D(Direct3D11Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = Screen.height,
                Width = Screen.width,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 1,
                Usage = ResourceUsage.Default
            });


            var tex = UnityTexture.CreateExternalTexture(Screen.width, Screen.height, TextureFormat.BGRA32, false, true, texture.NativePointer);
            Image.texture = tex;
            Image.uvRect = new Rect(0, 1, 1, -1);
            Image.color = Color.white;
            Image.texture.filterMode = FilterMode.Trilinear;

            var surface = texture.QueryInterface<Surface>();

            renderTarget = new DeviceContext(Direct2D1Device, DeviceContextOptions.EnableMultithreadedOptimizations)
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
            Direct2D1Device.Dispose();
            Direct2D1Device = null;
        }
    }
}
