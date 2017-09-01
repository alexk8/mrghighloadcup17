using shared.Entities;

namespace shared.Services
{
    public interface IDatabase
    {
        bool insert<T>(T value) where T : class,IEntity;
        T find<T>(uint id) where T : class, IEntity;
    }
}
