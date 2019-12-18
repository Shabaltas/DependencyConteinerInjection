﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependencyContainer
    {
        private Stack<Type> dependenciesStack = new Stack<Type>();
        public DependenciesConfiguration DependenciesConfiguration { get; set; }
        
        private Dictionary<string, Object> _singletonBeans = new Dictionary<string, object>();

        public DependencyContainer(DependenciesConfiguration configuration)
        {
            DependenciesConfiguration = configuration;
        }
        public I Resolve<I>(string id)
        {
            return (I)Resolve(typeof(I), id);
        }
        
        public I Resolve<I>()
        {
            return (I)Resolve(typeof(I));
        }

        private ConstructorInfo getConstructorWithMaxParams(ConstructorInfo[] constructors)
        {
            int maxCount = -1;
            int currentCount;
            int resultIndex = 0;
            for (int i = 0; i < constructors.Length; i++)
            {
                if ((currentCount = constructors[i].GetParameters().Length) > maxCount)
                {
                    maxCount = currentCount;
                    resultIndex = i;
                }
            }

            return constructors[resultIndex];
        }

        private Object CreateBean(ConstructorInfo constructor)
        {
            ParameterInfo[] parametersInfo = constructor.GetParameters();
            object[] parameters = new object[parametersInfo.Length];
            for (int i = 0; i < parametersInfo.Length; i++)
            {
                Attribute customAttribute = parametersInfo[i].GetCustomAttribute(typeof(Provided));
                if (customAttribute == null)
                    parameters[i] = default;
                else
                {
                    string id = ((Provided) customAttribute).Id;
                    parameters[i] = id == null
                        ? Resolve(parametersInfo[i].ParameterType)
                        : Resolve(parametersInfo[i].ParameterType, id);
                }
            }
            return constructor.Invoke(parameters);
        }

        private object Resolve(Type interfaceType, string id)
        {
            if (!IsInterfaceValid(ref interfaceType, out var args))
                throw new DependencyException($"No dependency for the {interfaceType.Name}");
            List<Dependency> dependencies = DependenciesConfiguration.GetDependencies(interfaceType);
            if (dependenciesStack.Contains(interfaceType))
                throw new DependencyException("Beans have cyclic dependency");
            dependenciesStack.Push(interfaceType);
            Dependency foundDependency = null;
            foreach (var dependency in dependencies)
            {
                if (dependency.Id.Equals(id))
                {
                    foundDependency = dependency;
                    break;
                }
            }
            if (foundDependency == null)
                throw new DependencyException($"Bean {id} is not registered");
            object bean = ResolveDependency(args == null 
                ? foundDependency 
                : new Dependency(foundDependency.ImplType.MakeGenericType(args), foundDependency.Scope, foundDependency.Id));
            dependenciesStack.Pop();
            return bean;
        }

        private Object Resolve(Type interfaceType)
        {
            if (!IsInterfaceValid(ref interfaceType, out var args))
                if (interfaceType.GetInterface("IEnumerable") != null) 
                {
                    Type genericInterfaceType = interfaceType.GetGenericArguments()[0]; 
                    DependenciesConfiguration.CheckDependencyForType(genericInterfaceType);
                    return  GetType().GetMethod("ResolveList", BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(genericInterfaceType)
                        .Invoke(this, new object[]{genericInterfaceType});
                } else 
                    throw new DependencyException($"No dependency for the {interfaceType.Name}");

            List<Dependency> dependencies = DependenciesConfiguration.GetDependencies(interfaceType);
            if (dependencies.Count > 1)
                throw new DependencyException($"Can't define implementation for {interfaceType.Name}");
            if (dependenciesStack.Contains(interfaceType))
                throw new DependencyException("Beans have cyclic dependency");
            dependenciesStack.Push(interfaceType);
            Dependency foundDependency = dependencies[0];
            object bean = ResolveDependency(args == null 
                ? foundDependency
                : new Dependency(foundDependency.ImplType.MakeGenericType(args), foundDependency.Scope, foundDependency.Id));
            dependenciesStack.Pop();
            return bean;
        }

        private bool ValidateGenericInterfaceAndGetArgs(ref Type interfaceType, out Type[] args)
        {
            args = null;
            if (DependenciesConfiguration.ContainsDependencyForType(interfaceType.GetGenericTypeDefinition()))
            {
                args = interfaceType.GetGenericArguments();
                interfaceType = interfaceType.GetGenericTypeDefinition();
                return true;
            }
            return false;
        }

        private bool IsInterfaceValid(ref Type interfaceType, out Type[] args)
        {
            args = null;
            return (DependenciesConfiguration.ContainsDependencyForType(interfaceType)
                    || (interfaceType.IsGenericType &&
                        ValidateGenericInterfaceAndGetArgs(ref interfaceType, out args)));
        }
        
        private List<T> ResolveList<T>(Type interfaceType)
        {
            List<Dependency> dependencies = DependenciesConfiguration.GetDependencies(interfaceType);
            List<T> result = new List<T>();
            if (dependenciesStack.Contains(interfaceType))
                throw new DependencyException("Beans have cyclic dependency");
            dependenciesStack.Push(interfaceType);
            dependencies.ForEach(dependency =>
            {
                result.Add((T)ResolveDependency(dependency));
            });
            dependenciesStack.Pop();
            return result;
        }

        private Object ResolveDependency(Dependency dependency)
        {
            string id = dependency.Id;
            if (_singletonBeans.ContainsKey(id))
                return _singletonBeans[id];
            ConstructorInfo[] constructors = dependency.ImplType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (constructors.Length == 0)
                throw new DependencyException($"No public constructor for bean \"{id}\"");
            ConstructorInfo constructor = getConstructorWithMaxParams(constructors);
            object bean = CreateBean(constructor);
            if (dependency.Scope == Lifetime.Singleton)
                _singletonBeans.Add(id, bean);
            return bean;
        }
    }
    
    
}