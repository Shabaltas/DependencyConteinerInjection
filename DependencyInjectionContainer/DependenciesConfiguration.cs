using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependenciesConfiguration
    {
        private Dictionary<Type, List<Dependency>> _dependencies = new Dictionary<Type, List<Dependency>>();
        
        public void Register<I, Impl>() 
            where I : class
            where Impl : class, I
        {
            register(typeof(I), typeof(Impl), Lifetime.Singleton, typeof(Impl).Name);
        }

        public void Register<I, Impl>(Lifetime scope) 
            where I : class
            where Impl : class, I
        {
            register(typeof(I), typeof(Impl), scope, typeof(Impl).Name);
        }
        
        public void Register<I, Impl>(string id) 
            where I : class
            where Impl : class, I
        {
            register(typeof(I), typeof(Impl), Lifetime.Singleton, id);
        }

        public void Register<I, Impl>(Lifetime scope, string id) 
            where I : class
            where Impl : class, I
        {
            register(typeof(I), typeof(Impl), scope, id);
        }
        
        public void Register(Type interfaceType, Type implType, Lifetime scope, string id)
        {
            if (!DoesInheritType(implType, interfaceType)) 
                throw new DependencyException("Dependency implementation class must inherit/implement dependency interface");
            register(interfaceType, implType, scope, id);
        }
        
        public void Register(Type interfaceType, Type implType, Lifetime scope)
        {
            Register(interfaceType, implType, scope, implType.Name);
        }
        
        public void Register(Type interfaceType, Type implType, string id)
        {
            Register(interfaceType, implType, Lifetime.Singleton, id);
        }
        
        public void Register(Type interfaceType, Type implType)
        {
            Register(interfaceType, implType, Lifetime.Singleton, implType.Name);
        }

        private void register(Type interfaceType, Type implType, Lifetime scope, string id)
        {
            if (implType.IsAbstract || implType.IsInterface)
                throw new DependencyException("Dependency implementation can't be abstract");
            Dependency newDependency = new Dependency(implType, scope, id);
            if (_dependencies.ContainsKey(interfaceType))
            {
                List<Dependency> dependencies = _dependencies[interfaceType];
                if (dependencies.Any(dependency => dependency.ImplType == implType))
                    throw new DependencyException($"Bean with such type {implType.Name} has been already registered for {interfaceType.Name}");
                dependencies.Add(newDependency);
            }
            else
                _dependencies.Add(interfaceType, new List<Dependency>(){newDependency});
        }

        internal List<Dependency> GetDependencies(Type interfaceType)
        {
            return _dependencies[interfaceType];
        }

        internal void CheckDependencyForType(Type interfaceType)
        {
            if (!_dependencies.Keys.Contains(interfaceType))
                throw new DependencyException($"No dependency for the {interfaceType.Name}");
        }
        
        internal bool ContainsDependencyForType(Type interfaceType)
        {
            return _dependencies.Keys.Contains(interfaceType);
        }

        private bool DoesInheritType(Type implType, Type interfaceType)
        {
            if (interfaceType.IsGenericType)
                return IsAssignableFromGeneric(implType, interfaceType);
            return GetBaseTypes(implType).Contains(interfaceType);
        }
        private IList<Type> GetBaseTypes(Type type)
        {
            List<Type> types = new List<Type>();
            for (Type baseType = type; baseType != null; baseType = baseType.BaseType)
                types.Add(baseType);
            types.AddRange(type.GetInterfaces());
            return types;
        }
        private bool IsAssignableFromGeneric(Type implType, Type interfaceType)
        {
            IList<Type> baseTypes = GetBaseTypes(GetTypeDefinition(implType));
            return baseTypes
                .Select(GetTypeDefinition)
                .Contains(interfaceType.GetGenericTypeDefinition());
        }
        private Type GetTypeDefinition(Type type) =>
            type.IsGenericType ? type.GetGenericTypeDefinition() : type;

         
    }
}