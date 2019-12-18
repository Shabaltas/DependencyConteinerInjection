using DependencyInjectionContainer;

namespace TestableLib
{
    public class Class1
    {
        public Class1([Provided] Class2 class2) {}
    }
}