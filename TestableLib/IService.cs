namespace TestableLib
{
    public interface IService<in T> : IBaseService where T: IRepository
    {

    }
}