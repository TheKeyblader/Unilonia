using Avalonia.Controls;
using Avalonia.Dialogs;
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

        void Start()
        {
            if (viewType.Type == null) throw new ArgumentNullException("View");
            AvaloniaApp.Start();
            var topLevel = gameObject.AddComponent<TopLevelImpl>();
            topLevel.Setup();
            topLevel.Content = (Control)Activator.CreateInstance(viewType.Type, new object[0]);
        }
    }
}