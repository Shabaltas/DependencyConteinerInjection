using DependencyInjectionContainer;

namespace TestableLib
{
    public class SelfDependent
    {
        public SelfDependent([Provided] SelfDependent self) {}
    }
}