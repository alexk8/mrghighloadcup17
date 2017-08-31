using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using shared.Entities;
namespace shared.Services
{
    public class InmemoryDatabase:IDatabase
    {
        ConcurrentDictionary<uint, User> Users;
        ConcurrentDictionary<uint, Location> Locations;
        ConcurrentDictionary<uint, Visit> Visits;


        public void FillTable<T>(ref ConcurrentDictionary<uint,T> dest,IEnumerable<List<T>> batch) where T:IEntity
        {
            var count = batch.Sum(list => list.Count);
            dest = new ConcurrentDictionary<uint, T>(3, count + count >> 2);
            foreach (var x in batch.SelectMany(list => list)) dest[x.id] = x;
            foreach (var list in batch) list.Clear();//free some mem
        }


        public InmemoryDatabase(List<DataFile> filesContents)
        {
            try
            {
                long started = DateTime.Now.Ticks;

                FillTable<User>(ref Users, filesContents.Where(f => f.users != null).Select(f => f.users));
                FillTable<Location>(ref Locations, filesContents.Where(f => f.locations != null).Select(f => f.locations));
                FillTable<Visit>(ref Visits, filesContents.Where(f => f.visits != null).Select(f => f.visits));

                Console.WriteLine($"data size:  U={Users.Count}  L={Locations.Count}  V={Visits.Count} ");
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
            foreach (var x in Users.Values) x.jsonCached = JsonSerializers.Serialize(x);
            foreach (var x in Locations.Values) x.jsonCached = JsonSerializers.Serialize(x);
            //too much memory
            //foreach (var x in Visits.Values) x.jsonCached = JsonSerializers.Serialize(x); 
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

        public bool insert<T>(T value) where T : class
        {
            if (typeof(T) == typeof(User))
            {
                User val = value as User;
                Users[val.id] = val;
                return true;
            }
            else if (typeof(T) == typeof(Location))
            {
                Location val = value as Location;
                Locations[val.id] = val;
                return true;
            }
            else if (typeof(T) == typeof(Visit))
            {
                var visit = value as Visit;
                Visits[visit.id] = visit;

                if (!Users.TryGetValue(visit.user, out User user)) return false;
                if (!Locations.TryGetValue(visit.location, out Location location)) return false;
                visit.UserRef = user;
                visit.LocationRef = location;
                user.AddVisit(visit);
                location.AddVisit(visit);

                return true;
            }
            else return false;
        }

        public T find<T>(uint id) where T : class
        {
            if (typeof(T) == typeof(User))
            {
                if (Users.TryGetValue(id, out User val)) return val as T; else return null;
            }
            else if (typeof(T) == typeof(Location))
            {
                if (Locations.TryGetValue(id, out Location val)) return val as T; else return null;
            }
            else if (typeof(T) == typeof(Visit))
            {
                if (Visits.TryGetValue(id, out Visit val)) return val as T; else return null;
            }
            else throw new Exception("bad entity type");
        }


    }
}
