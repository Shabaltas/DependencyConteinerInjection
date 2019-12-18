using System;

namespace DependencyInjectionContainer
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Provided : System.Attribute
    {
        public string Id { get; }

        public Provided(string id)
        {
            Id = id;
        }
        
        public Provided() {}
    }
}