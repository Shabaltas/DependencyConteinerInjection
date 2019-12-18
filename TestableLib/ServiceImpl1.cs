using DependencyInjectionContainer;

namespace TestableLib
{
    public class ServiceImpl1<T> : IService<T> 
        where T : IRepository
    {
        private T Repository;
        public ServiceImpl1([Provided] T repository)
        {
            Repository = repository;
        }

        public override string ToString()
        {
            return "Service IMPL 1 " + Repository;
        }
    }
}