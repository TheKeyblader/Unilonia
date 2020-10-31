using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using TypeReferences;
using Unilonia.Input;
using Unilonia.Selectors;
using Unilonia.Settings;
using UnityEngine;
using UnityEngine.UI;
using AvaloniaApplication = Avalonia.Application;
using Canvas = UnityEngine.Canvas;

namespace Unilonia
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

        [CustomInherits(typeof(AvaloniaApplication), ExcludeNone = false)]
        [InspectorName("For sample only")]
        public TypeReference overrideApplicationType;

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
            rawImage.color = Color.white;
        }

        private void Start()
        {
            var settings = UniloniaSettings.Load();

            if (viewType.Type == null) throw new ArgumentNullException("View");
            AvaloniaApp.Start(overrideApplicationType.Type);

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

            if (texture != topLevel.Texture)
            {
                texture = topLevel.Texture;
                rawImage.texture = texture;
            }

            if (screenSize.x != Screen.width || screenSize.y != Screen.height)
            {
                screenSize = new Vector2Int(Screen.width, Screen.height);
                topLevel.Resize(new Size(Screen.width, Screen.height));
            }
        }

        private void OnDestroy()
        {
            topLevel.Close();
            topLevel.Dispose();
        }
    }
}