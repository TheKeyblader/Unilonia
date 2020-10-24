using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Packages.Unilonia.Selectors;
using System;
using TypeReferences;
using Unilonia;
using Unilonia.Input;
using Unilonia.Settings;
using UnityEngine;
using UnityEngine.UI;
using Canvas = UnityEngine.Canvas;

namespace Packages.Unilonia
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(RawImage))]
    public class AvaloniaView : MonoBehaviour
    {
        [EmptyConstructor(typeof(UserControl))]
        [InspectorName("View")]
        public TypeReference viewType;

        [InspectorName("Draw FPS")]
        public bool drawFps;

        private TopLevelImpl topLevel;
        private Vector2Int screenSize;
        private Texture texture;
        private RawImage rawImage;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            rawImage = GetComponent<RawImage>();
            rawImage.uvRect = new UnityEngine.Rect(0, 1, 1, -1);
            rawImage.color = Color.white;
        }

        void Start()
        {
            var settings = UniloniaSettings.Load();

            if (viewType.Type == null) throw new ArgumentNullException("View");
            AvaloniaApp.Start();

            screenSize = new Vector2Int(Screen.width, Screen.height);
            topLevel = new TopLevelImpl(new Size(Screen.width, Screen.height), settings.useDeferredRendering);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                topLevel.Init();
                topLevel.Content = (Control)Activator.CreateInstance(viewType.Type, new object[0]);
                topLevel.Paint(new Avalonia.Rect(0, 0, screenSize.x, screenSize.y));
            }).Wait();

#if ENABLE_INPUT_SYSTEM
            var input = gameObject.AddComponent<UnityInputSystem>();
            input.TopLevel = topLevel;
#endif
        }

        private void Update()
        {
            if (topLevel.DrawFPS != drawFps)
            {
                topLevel.DrawFPS = drawFps;
            }

            if ((texture == null || topLevel.TexturePtr != texture.GetNativeTexturePtr()) && topLevel.TexturePtr != IntPtr.Zero)
            {
                texture = Texture2D.CreateExternalTexture(screenSize.x, screenSize.y, TextureFormat.BGRA32, false, true, topLevel.TexturePtr);
                texture.hideFlags = HideFlags.DontSave;
                rawImage.texture = texture;
            }

            if (screenSize.x != Screen.width || screenSize.y != Screen.height)
            {
                screenSize = new Vector2Int(Screen.width, Screen.height);
                DestroyImmediate(texture);
                topLevel.Resize(new Size(Screen.width, Screen.height));
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(texture);
            topLevel.Close();
            topLevel.Dispose();
        }
    }
}