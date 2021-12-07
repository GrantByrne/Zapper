namespace Zapper.WebOs.Factory
{
    public interface IFactory<T>
    {
        T Create();
    }
}
