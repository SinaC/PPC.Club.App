using PPC.Domain;

namespace PPC.Module.Common
{
    public interface IReloadModule : IModule
    {
        void Reload(Session session);
    }
}
