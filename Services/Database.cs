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

        public Dataset<User> Users { get; private set; }
        public Dataset<Location> Locations { get; private set; }
        public Dataset<Visit> Visits { get; private set; }

        public int? CurrentTime { get; set; }

        public InmemoryDatabase(TaskFilesData allData)
        {
            try
            {
                long started = DateTime.Now.Ticks;

                CurrentTime = allData.currentTime;
                Users = new Dataset<User>(allData.files.Where(f => f.users != null).Select(f => f.users));
                Locations= new Dataset<Location>(allData.files.Where(f => f.locations != null).Select(f => f.locations));
                Visits= new Dataset<Visit>(allData.files.Where(f => f.visits != null).Select(f => f.visits));

                Console.WriteLine($"DB loaded in {(DateTime.Now.Ticks - started) / 10000} ms");

                InitCaches();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: "+e.Message);
            }
        }

        private void InitCaches()
        {
            GC.Collect();// free some mem

            long started = DateTime.Now.Ticks;

            foreach (Visit visit in Visits)
            {
                var user = Users[visit.user];
                var location = Locations[visit.location];
                visit.UserRef = user;
                visit.LocationRef = location;
                user.AddVisit(visit);
                location.AddVisit(visit);
            }

            foreach (var x in Users) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x));
            foreach (var x in Locations) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x));
            //too much memory (remove line)
            //foreach (var x in Visits.Values) x.jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(x)); 

            Console.WriteLine($"cash done in {(DateTime.Now.Ticks - started) / 10000} ms");

            GC.Collect();//prepare to fight
        }

    }
}
