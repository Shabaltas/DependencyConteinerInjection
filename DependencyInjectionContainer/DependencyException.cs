using System;

namespace DependencyInjectionContainer
{
    public class DependencyException : Exception
    {
        public DependencyException(string message) : base(message)
        {
        }
    }
}