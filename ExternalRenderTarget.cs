using System;
using System.Threading;
using Avalonia;
using Avalonia.Direct2D1;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using UnityEngine;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using RenderTarget = SharpDX.Direct2D1.RenderTarget;
using Resource = SharpDX.DXGI.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using UnityTexture = UnityEngine.Texture2D;

namespace Unilonia
{
    internal class ExternalRenderTarget : IExternalDirect2DRenderTargetSurface, IDisposable
    {
        private Texture2D renderedTexture;
        private Texture2D visibleTexture;
        private UnityTexture unityVisibleTexture;
        private DeviceContext renderTarget;
        private Bitmap1 bitmap;
        private static readonly SharpDX.Direct3D11.Device Direct3D11Device;

        private bool hasRendererTarget;
        private static readonly Material blitMaterial;

        public Size ClientSize { get; set; }
        public RenderTexture Texture { get; private set; }

        private readonly Mutex mutex = new Mutex();

        static ExternalRenderTarget()
        {
            var empty = new UnityTexture(1, 1);
            var emptyText = new Texture2D(empty.GetNativeTexturePtr());
            Direct3D11Device = emptyText.Device;
            blitMaterial = new Material(Shader.Find("Hidden/Spout/Blit"));
            blitMaterial.hideFlags = HideFlags.DontSave;

        }

        public void AfterDrawing()
        {
            Direct2D1Platform.Direct3D11Device.ImmediateContext.CopyResource(renderedTexture, visibleTexture);
            Direct2D1Platform.Direct3D11Device.ImmediateContext.Flush();
            mutex.ReleaseMutex();

            UnityDispatcher.UnityThread.InvokeAsync(() =>
            {
                mutex.WaitOne();
                if (unityVisibleTexture != null && Texture != null)
                    Graphics.Blit(unityVisibleTexture, Texture, blitMaterial, 1);
                mutex.ReleaseMutex();
            }).Wait();
        }

        public void BeforeDrawing()
        {
            mutex.WaitOne();
        }

        public void DestroyRenderTarget()
        {
            if (hasRendererTarget)
            {
                mutex.WaitOne();
                UnityEngine.Object.DestroyImmediate(unityVisibleTexture);
                UnityEngine.Object.DestroyImmediate(Texture);
                renderedTexture?.Dispose();
                visibleTexture?.Dispose();
                renderTarget?.Dispose();
                bitmap?.Dispose();
                hasRendererTarget = false;
                mutex.ReleaseMutex();
            }
        }

        public RenderTarget GetOrCreateRenderTarget()
        {
            if (!hasRendererTarget)
            {
                UnityDispatcher.UnityThread.InvokeAsync(() =>
                {
                    mutex.WaitOne();
                    renderTarget = Create();
                    mutex.ReleaseMutex();
                }).Wait();
            }
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

            unityVisibleTexture = UnityTexture.CreateExternalTexture((int)ClientSize.Width, (int)ClientSize.Height, TextureFormat.BGRA32, false, true, unityTex.NativePointer);
            unityVisibleTexture.hideFlags = HideFlags.DontSave;
            Texture = new RenderTexture((int)ClientSize.Width, (int)ClientSize.Height, 1, RenderTextureFormat.BGRA32, 1);

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
