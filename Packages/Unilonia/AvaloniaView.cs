using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Threading;
using Packages.Unilonia.Selectors;
using System;
using TypeReferences;
using Unilonia;
using UnityEngine;

namespace Packages.Unilonia
{
    public class AvaloniaView : MonoBehaviour
    {
        [EmptyConstructor(typeof(UserControl))]
        [InspectorName("View")]
        public TypeReference viewType;

        [InspectorName("Draw FPS")]
        public bool drawFps;

        private TopLevelImpl topLevel;

        void Start()
        {
            if (viewType.Type == null) throw new ArgumentNullException("View");
            AvaloniaApp.Start();
            topLevel = gameObject.AddComponent<TopLevelImpl>();
            topLevel.Setup();
            Dispatcher.UIThread.Post(() =>
            {
                topLevel.Content = (Control)Activator.CreateInstance(viewType.Type, new object[0]);
            });
        }

        private void Update()
        {
            if (topLevel.DrawFPS != drawFps)
            {
                topLevel.DrawFPS = drawFps;
            }
        }
    }
}