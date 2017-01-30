using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Unless a specific instance has been registered via RegisterInstance, every Resolve will create a new instance

namespace EasyIoc
{
    public sealed class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, Type> _implementations = new Dictionary<Type, Type>(); // registrered with RegisterType
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>(); // registered with RegisterFactory
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>(); // registered with RegisterInstance
        private readonly Dictionary<Type, ResolveNodeBase> _resolveTrees = new Dictionary<Type, ResolveNodeBase>();

        private readonly object _lockObject = new object();

        private static readonly Lazy<IocContainer> LazyDefault = new Lazy<IocContainer>(() => new IocContainer());
        public static IIocContainer Default
        {
            get { return LazyDefault.Value; }
        }

        #region IIocContainer

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            bool found;
            lock (_lockObject)
            {
                found = _implementations.ContainsKey(interfaceType) || _factories.ContainsKey(interfaceType) || _instances.ContainsKey(interfaceType);
            }
            return found;
        }

        public void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Register: Only an interface can be registered");

            Type implementationType = typeof (TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot Register: No interface or abstract class is valid as implementation");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException(String.Format("{0} is not assignable from {1}", interfaceType.FullName, implementationType.FullName));

            lock (_lockObject)
            {
                if (_factories.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot Register: A factory has already been registered");

                if (_instances.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot Register: An instance has already been registered");

                if (_implementations.ContainsKey(interfaceType))
                {
                    if (_implementations[interfaceType] != implementationType)
                        throw new ArgumentException(String.Format("Cannot Register: An implementation has already been registered for interface {0}", interfaceType.FullName));
                }
                else
                    _implementations.Add(interfaceType, implementationType);
                // ResolveTree will be built on first call to Resolve
            }
        }

        public void RegisterFactory<TInterface>(Func<TInterface> createFunc)
            where TInterface : class
        {
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");

            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterFactory: Only an interface can be registered");

            lock (_lockObject)
            {
                if (_implementations.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot RegisterFactory: An implementation has already been registered");

                if (_instances.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot RegisterFactory: An instance has already been registered");

                if (_factories.ContainsKey(interfaceType))
                {
                    if (_factories[interfaceType] != createFunc)
                        throw new ArgumentException(String.Format("Cannot RegisterFactory: A factory has already been registered for interface {0}", interfaceType.FullName));
                }
                else
                    _factories[interfaceType] = createFunc;
            }
        }

        public void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterInstance: Only an interface can be registered");

            lock (_lockObject)
            {
                if (_implementations.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot RegisterFactory: An implementation has already been registered");

                if (_factories.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot RegisterInstance: A factory has already been registered");

                if (_instances.ContainsKey(interfaceType))
                {
                    if (_instances[interfaceType] != instance)
                        throw new ArgumentException(String.Format("Cannot RegisterInstance: An instance has already been registered for interface {0}", interfaceType.FullName));
                }
                else
                    _instances.Add(interfaceType, instance);
            }
        }

        public void Unregister<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            lock (_lockObject)
            {
                _implementations.Remove(interfaceType);
                _factories.Remove(interfaceType);
                _instances.Remove(interfaceType);
                _resolveTrees.Remove(interfaceType);
            }
        }

        public void UnregisterType<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            lock (_lockObject)
            {
                _implementations.Remove(interfaceType);
                _resolveTrees.Remove(interfaceType); // Force ResolveTree rebuild
            }
        }

        public void UnregisterFactory<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            lock (_lockObject)
            {
                _factories.Remove(interfaceType);
                _resolveTrees.Remove(interfaceType); // Force ResolveTree rebuild
            }
        }

        public void UnregisterInstance<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            lock (_lockObject)
            {
                _instances.Remove(interfaceType);
                _resolveTrees.Remove(interfaceType); // Force ResolveTree rebuild
            }
        }

        public TInterface Resolve<TInterface>() where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Search in resolve tree cache
                if (!_resolveTrees.TryGetValue(interfaceType, out resolveTree))
                {
                    // Create resolve tree if not found
                    resolveTree = BuildResolveTree(interfaceType);
                    if (!(resolveTree is ErrorNode))
                        _resolveTrees.Add(interfaceType, resolveTree); // save resolve tree only if not in error
                }
            }
            // Check errors
            if (resolveTree is ErrorNode)
            {
                if (resolveTree == ErrorNode.TypeNotRegistered)
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.NoPublicConstructorOrNoConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.NoResolvableConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.CyclicDependencyConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: Cyclic dependency detected for type {0}", interfaceType.FullName));
            }
            // Create instance
            try
            {
                resolved = resolveTree.Resolve();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
            }
            return (TInterface) resolved;
        }

        public TInterface Resolve<TInterface>(IEnumerable<ParameterValue> parameters) where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Don't search in resolve tree cache: ResolveTree may be different with or without parameters
                // Create resolve tree
                List<ParameterValue> parametersList = parameters == null ? null : parameters.ToList();
                resolveTree = BuildResolveTree(interfaceType, parametersList);
            }
            // Check errors
            if (resolveTree is ErrorNode)
            {
                if (resolveTree == ErrorNode.TypeNotRegistered)
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.NoPublicConstructorOrNoConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.NoResolvableConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
                if (resolveTree == ErrorNode.CyclicDependencyConstructor)
                    throw new ArgumentException(String.Format("Cannot Resolve: Cyclic dependency detected for type {0}", interfaceType.FullName));
            }
            // Create instance
            try
            {
                resolved = resolveTree.Resolve();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
            }
            return (TInterface) resolved;
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _implementations.Clear();
                _factories.Clear();
                _instances.Clear();
                _resolveTrees.Clear();
            }
        }

        #endregion

        // Following code is NOT responsible for locking collections

        #region Resolve Tree

        private abstract class ResolveNodeBase
        {
            public Type InterfaceType { protected get; set; }

            public abstract object Resolve();
        }

        private sealed class ErrorNode : ResolveNodeBase
        {
            public static readonly ResolveNodeBase TypeNotRegistered = new ErrorNode();
            public static readonly ResolveNodeBase NoPublicConstructorOrNoConstructor = new ErrorNode();
            public static readonly ResolveNodeBase NoResolvableConstructor = new ErrorNode();
            public static readonly ResolveNodeBase CyclicDependencyConstructor = new ErrorNode();

            public override object Resolve()
            {
                throw new InvalidOperationException("Cannot resolve an ErrorNode");
            }
        }

        private sealed class ValueNode : ResolveNodeBase
        {
            public IParameterValue ParameterValue { private get; set; }

            public override object Resolve()
            {
                if (ParameterValue == null)
                    throw new InvalidOperationException("No parameter value");
                return ParameterValue.Value;
            }
        }

        private sealed class FactoryNode : ResolveNodeBase
        {
            public Func<object> Factory { private get; set; }

            public override object Resolve()
            {
                if (Factory == null)
                    throw new InvalidOperationException(String.Format("No factory for type {0}", InterfaceType.FullName));

                object instance = Factory.DynamicInvoke(null);
                return instance;
            }
        }

        private sealed class InstanceNode : ResolveNodeBase
        {
            public object Instance { private get; set; }

            public override object Resolve()
            {
                return Instance;
            }
        }

        private sealed class BuildableNode : ResolveNodeBase
        {
            public ConstructorInfo ConstructorInfo { get; set; }
            public List<ResolveNodeBase> Parameters { get; set; }

            public override object Resolve()
            {
                if (ConstructorInfo == null)
                    throw new InvalidOperationException(String.Format("No constructor info for type {0}", InterfaceType.FullName));

                // If parameterless, create instance
                if (Parameters == null)
                    return ConstructorInfo.Invoke(null);

                // If parameters, recursively create parameters instance
                object[] parameters = new object[Parameters.Count];
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ResolveNodeBase unspecializedParameter = Parameters[i];

                    object parameterValue = unspecializedParameter.Resolve();
                    parameters[i] = parameterValue;
                }
                // and create instance using parameters
                return ConstructorInfo.Invoke(parameters);
            }
        }

        private ResolveNodeBase BuildResolveTree(Type interfaceType, List<ParameterValue> userDefinedParameters = null)
        {
            List<Type> discoveredTypes = new List<Type>
            {
                interfaceType
            };
            return InnerBuildResolveTree(interfaceType, discoveredTypes, userDefinedParameters);
        }

        private ResolveNodeBase InnerBuildResolveTree(Type interfaceType, ICollection<Type> discoveredTypes, List<ParameterValue> userDefinedParameters = null)
        {
            // Factory ?
            Func<object> factory;
            if (_factories.TryGetValue(interfaceType, out factory))
                return new FactoryNode
                {
                    InterfaceType = interfaceType,
                    Factory = factory
                };
            // Instance ?
            object instance;
            if (_instances.TryGetValue(interfaceType, out instance))
                return new InstanceNode
                {
                    InterfaceType = interfaceType,
                    Instance = instance
                };

            // Implementation ?
            Type implementationType;
            if (!_implementations.TryGetValue(interfaceType, out implementationType))
                return ErrorNode.TypeNotRegistered;

            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            // Valid constructor ?
            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                return ErrorNode.NoPublicConstructorOrNoConstructor;

            // Get parameters for each ctor
            var constructorAndParameters = constructorInfos.Select(x => new
            {
                Constructor = x,
                Parameters = x.GetParameters()
            }).ToList();

            // Get first parameterless if any
            var parameterless = constructorAndParameters.FirstOrDefault(x => x.Parameters.Length == 0);
            if (parameterless != null)
                return new BuildableNode
                {
                    InterfaceType = interfaceType,
                    ConstructorInfo = parameterless.Constructor
                };

            // Check if every ctor's parameter is registered in container or resolvable, returns first resolvable
            foreach (var c in constructorAndParameters)
            {
                List<ResolveNodeBase> parametersResolvable = new List<ResolveNodeBase>(c.Parameters.Length);

                // Try to resolved every parameters
                bool ok = true;
                foreach (ParameterInfo parameterInfo in c.Parameters)
                {
                    ParameterValue parameterValue = userDefinedParameters != null ? userDefinedParameters.FirstOrDefault(x => x.Name == parameterInfo.Name) : null;
                    if (parameterValue != null)
                    {
                        ValueNode parameter = new ValueNode
                        {
                            InterfaceType = null,
                            ParameterValue = parameterValue
                        };
                        parametersResolvable.Add(parameter);
                    }
                    else
                    {
                        if (discoveredTypes.Any(x => x == parameterInfo.ParameterType)) // check cyclic dependency
                        {
                            ok = false;
                            break;
                        }

                        discoveredTypes.Add(parameterInfo.ParameterType); // add parameter type to discovered type
                        ResolveNodeBase parameter = InnerBuildResolveTree(parameterInfo.ParameterType, discoveredTypes, userDefinedParameters);
                        discoveredTypes.Remove(parameterInfo.ParameterType); // remove parameter type from discovered type

                        if (parameter is ErrorNode) // once an invalid ctor parameter has been found, try next ctor
                        {
                            ok = false;
                            break;
                        }
                        parametersResolvable.Add(parameter);
                    }
                }

                if (ok)
                    return new BuildableNode
                    {
                        InterfaceType = interfaceType,
                        ConstructorInfo = c.Constructor,
                        Parameters = parametersResolvable
                    };
            }
            return ErrorNode.NoResolvableConstructor;
        }

        #endregion
    }
}
