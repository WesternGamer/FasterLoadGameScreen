using Shared.Logging;

namespace Shared.Plugin
{
    public interface ICommonPlugin
    {
        IPluginLogger Log { get; }
    }
}