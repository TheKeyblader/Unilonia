using Avalonia;
using Unilonia;

namespace HelloWorld
{
    public class HelloWorldApp : AvaloniaApp<App>
    {
        public void Start()
        {
            SetupWithTopLevel(builder => builder
                .UseUnity()
                .UseDirect2D1()
                .LogToUnityDebug());
        }
    }
}
