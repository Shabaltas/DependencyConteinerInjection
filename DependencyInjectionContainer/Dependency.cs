using System;

namespace DependencyInjectionContainer
{
    public class Dependency
    {
        public readonly Lifetime Scope;
        public readonly string Id;
        public Type ImplType { get; internal set; }

        internal Dependency(Type type, Lifetime scope, string id)
        {
            Scope = scope;
            Id = id;
            ImplType = type;
        }
    }
}