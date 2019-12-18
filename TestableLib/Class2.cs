using DependencyInjectionContainer;

namespace TestableLib
{
    public class Class2
    {
        public Class2([Provided] Class1 class1) {}
    }
}