using System;
using System.Collections.Generic;

namespace EasyIoc
{
    public interface IIocContainer
    {
        bool IsRegistered<TInterface>()
            where TInterface : class;

        void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterFactory<TInterface>(Func<TInterface> createFunc)
            where TInterface : class;

        void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class;

        void Unregister<TInterface>()
            where TInterface : class;

        void UnregisterType<TInterface>()
            where TInterface : class;

        void UnregisterFactory<TInterface>()
            where TInterface : class;
        
        void UnregisterInstance<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>(IEnumerable<ParameterValue> parameters)
            where TInterface : class;

        void Reset();
    }
}
