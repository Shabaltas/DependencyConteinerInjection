using DependencyInjectionContainer;

namespace TestableLib
{
    public class ServiceImpl2<T> : IService<T> 
        where T : IRepository
    {
        private T Repository;
        public ServiceImpl2([Provided] T repository)
        {
            Repository = repository;
        }

        public override string ToString()
        {
            return "Service IMPL 2 " + Repository;
        }
    }
}