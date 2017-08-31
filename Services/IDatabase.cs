namespace shared.Services
{
    public interface IDatabase
    {
        bool insert<T>(T value) where T : class;
        T find<T>(uint id) where T : class;
    }
}
