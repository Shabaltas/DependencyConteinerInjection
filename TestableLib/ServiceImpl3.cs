using DependencyInjectionContainer;

namespace TestableLib
{
    public class ServiceImpl3<T> : IService<T> 
        where T : IRepository
    {
        public T Repository;
        public ServiceImpl3([Provided(id:"my")] T repository)
        {
            Repository = repository;
        }

        public override string ToString()
        {
            return "Service IMPL 2 " + Repository;
        }
    }
}