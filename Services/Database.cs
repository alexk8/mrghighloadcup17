using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using shared.Entities;
using System.Text;
using System.Collections;
using srvkestrel;

namespace shared.Services
{
    public class InmemoryDatabase:IDatabase
    {

        Dataset<User> Users;
        Dataset<Location> Locations;
        Dataset<Visit> Visits;

        public InmemoryDatabase(List<DataFile> filesContents)
        {
            try
            {
                long started = DateTime.Now.Ticks;

                Users= new Dataset<User>(filesContents.Where(f => f.users != null).Select(f => f.users));
                Locations= new Dataset<Location>(filesContents.Where(f => f.locations != null).Select(f => f.locations));
                Visits= new Dataset<Visit>(filesContents.Where(f => f.visits != null).Select(f => f.visits));

                Console.WriteLine($"DB loaded in {(DateTime.Now.Ticks - started) / 10000} ms");


                Console.Write("GC:");
                GC.Collect();
                Console.WriteLine("OK");

                started = DateTime.Now.Ticks;
                InitCaches();
                Console.WriteLine("generating json cache");
                InitJsonCaches();
                Console.WriteLine($"cash done in {(DateTime.Now.Ticks - started) / 10000} ms");

                Console.Write("GC:");
                GC.Collect();
                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: "+e.Message);
            }
        }

        private void InitJsonCaches()
        {
            foreach (var x in Users.Values) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x));
            foreach (var x in Locations.Values) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x));
            
            //too much memory (remove line)
            //foreach (var x in Visits.Values) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x)); 
        }

        private void InitCaches()
        {
            foreach (Visit visit in Visits.Values)
            {
                var user = Users[visit.user];
                var location = Locations[visit.location];
                visit.UserRef = user;
                visit.LocationRef = location;
                user.AddVisit(visit);
                location.AddVisit(visit);
            }
        }


        public Dataset<T> GetDataset<T>() where T:class,IEntity
        {
            if (typeof(T) == typeof(User)) return Users as Dataset<T>;
            else if (typeof(T) == typeof(Location)) return Locations as Dataset<T>;
            else if (typeof(T) == typeof(Visit)) return Visits as Dataset<T>;
            else throw new Exception("bad entity type");
        }

        public bool insert<T>(T value) where T : class,IEntity
        {
            GetDataset<T>()[value.id] = value;
            if (typeof(T) == typeof(Visit))
            {
                Visit visit = value as Visit;
                User user = Users[visit.user];
                Location location = Locations[visit.location];
                if (user == null || location == null) return false;
                visit.UserRef = user;
                visit.LocationRef = location;
                user.AddVisit(visit);
                location.AddVisit(visit);
            }
            return true;
        }

        public T find<T>(uint id) where T : class,IEntity
        {
            return GetDataset<T>()[id];
        }


    }
}
