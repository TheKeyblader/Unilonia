using Avalonia.Controls;

namespace Unilonia
{
    public sealed class UnityAppBuilder : AppBuilderBase<UnityAppBuilder>
    {
        public UnityAppBuilder() : base(new UnityRuntimePlatform(), builder => UnityRuntimePlatformServices.Register(builder.Instance?.GetType()?.Assembly))
        {

        }
    }
}
