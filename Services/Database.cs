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

        public InmemoryDatabase(IEnumerable<List<User>> users, IEnumerable<List<Location>> loactions, IEnumerable<List<Visit>> visits)
        {
            try
            {
                long started = DateTime.Now.Ticks;
                Users = new ConcurrentDictionary<uint, User>(users.SelectMany(list=>list).ToDictionary(o => o.id, o => o));
                foreach (var list in users) list.Clear();
                Locations = new ConcurrentDictionary<uint, Location>(loactions.SelectMany(list => list).ToDictionary(o => o.id, o => o));
                foreach (var list in loactions) list.Clear();
                Visits = new ConcurrentDictionary<uint, Visit>(visits.SelectMany(list => list).ToDictionary(o => o.id, o => o));
                foreach (var list in visits) list.Clear();

                Console.WriteLine($"data size:  U={Users.Count}  L={Locations.Count}  V={Visits.Count} ");
                Console.WriteLine($"DB loaded in {(DateTime.Now.Ticks - started) / 10000} ms");


                Console.Write("GC:");
                GC.Collect();
                Console.WriteLine("OK");

                started = DateTime.Now.Ticks;
                InitCaches();
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
