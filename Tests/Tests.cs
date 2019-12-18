using System.Collections.Generic;
using DependencyInjectionContainer;
using NUnit.Framework;
using TestableLib;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void GenericResolveSimpleTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IService<IRepository>, ServiceImpl1<IRepository>>();
            DependencyContainer container = new DependencyContainer(configuration);
            IRepository repository = container.Resolve<IRepository>();
            IService<IRepository> service = container.Resolve<IService<IRepository>>();
            Assert.IsInstanceOf<MyRepository>(repository);
            Assert.IsInstanceOf<ServiceImpl1<IRepository>>(service);
        }
        
        [Test]
        public void ScopesTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), Lifetime.Singleton, "1");
            configuration.Register(typeof(IService<>), typeof(ServiceImpl2<>), Lifetime.Prototype, "2");
            DependencyContainer container = new DependencyContainer(configuration);
            IService<IRepository> serviceImpl1 = container.Resolve<IService<IRepository>>("1");
            ServiceImpl2<IRepository> serviceImpl2 = (ServiceImpl2<IRepository>)container.Resolve<IService<IRepository>>("2");
            ServiceImpl1<IRepository> serviceImpl11 = (ServiceImpl1<IRepository>)container.Resolve<IService<IRepository>>("1");
            ServiceImpl2<IRepository> serviceImpl22 = (ServiceImpl2<IRepository>)container.Resolve<IService<IRepository>>("2");
            Assert.AreSame(serviceImpl1, serviceImpl11);
            Assert.That(serviceImpl2 != serviceImpl22);
        }
        
        [Test]
        public void NotDirectInterfaceTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IBaseService), typeof(ServiceImpl1<IRepository>));
            DependencyContainer container = new DependencyContainer(configuration);
            IBaseService service = container.Resolve<IBaseService>();
            Assert.IsInstanceOf<ServiceImpl1<IRepository>>(service);
        }
        
        [Test]
        public void ListOfDependenciesWithOpenGenericsTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository), "1");
            configuration.Register(typeof(IRepository), typeof(SomeRepository), "2");
            DependencyContainer container = new DependencyContainer(configuration);
            var services = container.Resolve<IList<IRepository>>();
            Assert.That(services.Count == 2);
            Assert.IsInstanceOf<MyRepository>(services[0]);
            Assert.IsInstanceOf<SomeRepository>(services[1]);
        }
        
        [Test]
        public void ListOfDependenciesTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository> ("1");
            configuration.Register<IRepository, SomeRepository>("2");
            DependencyContainer container = new DependencyContainer(configuration);
            var services = container.Resolve<IList<IRepository>>();
            Assert.That(services.Count == 2);
            Assert.IsInstanceOf<MyRepository>(services[0]);
            Assert.IsInstanceOf<SomeRepository>(services[1]);
        }
        
        [Test]
        public void RecursionTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<SelfDependent, SelfDependent> ();
            DependencyContainer container = new DependencyContainer(configuration);
            Assert.Throws<DependencyException>(() => container.Resolve<SelfDependent>());
        }
        
        [Test]
        public void CyclicDependencyTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<Class1, Class1> ();
            configuration.Register<Class2, Class2> ();
            DependencyContainer container = new DependencyContainer(configuration);
            Assert.Throws<DependencyException>(() => container.Resolve<Class1>());
            Assert.Throws<DependencyException>(() => container.Resolve<Class2>());
        }
        
        [Test]
        public void WrongIdTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository> ("my");
            DependencyContainer container = new DependencyContainer(configuration);
            Assert.Throws<DependencyException>(() => container.Resolve<IRepository>("wrong"));
        }
        
        [Test]
        public void ProvideByIdTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository> ("my");
            DependencyContainer container = new DependencyContainer(configuration);
            IRepository repository = container.Resolve<IRepository>("my");
            Assert.IsInstanceOf<MyRepository>(repository);
        }
        
        [Test]
        public void ProvidedAttributeTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>> ();
            DependencyContainer container = new DependencyContainer(configuration);
            IService<IRepository> service = container.Resolve<IService<IRepository>>();
            Assert.IsInstanceOf<ServiceImpl3<IRepository>>(service);
            Assert.IsInstanceOf<MyRepository>(((ServiceImpl3<IRepository>)service).Repository);
        }
        
        [Test]
        public void DependencyNotRegisteredTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>> ();
            DependencyContainer container = new DependencyContainer(configuration);
            Assert.Throws<DependencyException>(() => container.Resolve<ServiceImpl3<IRepository>>());
        }
        
        [Test]
        public void CollectionDependencyTest()
        {
            DependenciesConfiguration configuration = new DependenciesConfiguration();
            configuration.Register<IEnumerable<IRepository>, List<MyRepository>> ();
            DependencyContainer container = new DependencyContainer(configuration);
            IEnumerable<IRepository> repositories = container.Resolve<IEnumerable<IRepository>>();
            Assert.IsInstanceOf<List<MyRepository>>(repositories);
        }
        
    }
}