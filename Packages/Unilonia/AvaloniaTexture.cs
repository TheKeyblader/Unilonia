using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Unilonia.Selectors;
using TypeReferences;
using UnityEngine;
using AvaloniaApplication = Avalonia.Application;
using Unilonia.Settings;
using Avalonia.Threading;
using Avalonia;
using Unilonia.Input;

namespace Unilonia
{
    public class AvaloniaTexture : MonoBehaviour
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

        public Vector2Int size;
        private Texture texture;
        public Texture Texture => texture;

        void Start()
        {
            var settings = UniloniaSettings.Load();

            if (size == default) throw new ArgumentNullException(nameof(size));
            if (viewType.Type == null) throw new ArgumentNullException(nameof(viewType));
            AvaloniaApp.Start(overrideApplicationType.Type);

            topLevel = new TopLevelImpl(new Size(size.x, size.y), settings.useDeferredRendering);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                topLevel.Init();
                topLevel.Content = (Control)Activator.CreateInstance(viewType.Type, new object[0]);
                topLevel.Paint(new Avalonia.Rect(0, 0, size.x, size.y));
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
            }

            if (topLevel.ClientSize.Width != size.x || topLevel.ClientSize.Height != size.y)
            {
                topLevel.Resize(new Size(size.x, size.y));
            }
        }

        private void OnDestroy()
        {
            topLevel.Close();
            topLevel.Dispose();
        }
    }
}
