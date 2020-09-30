using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.ReactiveUI;
using DynamicData.Annotations;
using TodoApp;
using Unilonia;
using UnityEngine;

public class TodoAppApp : AvaloniaApp<App>
{
    // Start is called before the first frame update
    void Start()
    {
        SetupWithTopLevel(builder => builder
            .UseUnity()
            .UseDirect2D1()
            .UseReactiveUI()
            .LogToUnityDebug());
    }
}
