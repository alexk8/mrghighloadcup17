using shared.Entities;
using srvkestrel;

namespace shared.Services
{
    public interface IDatabase
    {
        //T find<T>(uint id) where T : class, IEntity;
        int? CurrentTime { get; }

        Dataset<User> Users { get; }
        Dataset<Location> Locations { get; }
        Dataset<Visit> Visits { get; }

    }
}
