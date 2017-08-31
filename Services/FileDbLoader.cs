using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using shared.Entities;
using System.Threading.Tasks;

namespace shared.Services
{
    public class FileDbLoader
    {
        class UsersFile
        {
            public List<User> users=null;
        }
        class LocationsFile
        {
            public List<Location> locations=null;
        }
        class VisitsFile
        {
            public List<Visit> visits=null;
        }



        public static IDatabase Load(string zipFn)
        {
            using (ZipArchive zip = ZipFile.OpenRead(zipFn))
            {

                List<List<User>> Users = new List<List<User>>();
                List<List<Location>> Locations = new List<List<Location>>();
                List<List<Visit>> Visits = new List<List<Visit>>();
                long started = DateTime.Now.Ticks;
                foreach (var file in zip.Entries)
                {
                    if (file.Name.StartsWith("users"))
                    {
                        using (var fs = new StreamReader(file.Open(), Encoding.UTF8))
                            Users.Add(JsonSerializers.DeserializeStream<UsersFile>(fs).users);
                    }
                    else if (file.Name.StartsWith("locations"))
                    {
                        using (var fs = new StreamReader(file.Open(), Encoding.UTF8))
                            Locations.Add(JsonSerializers.DeserializeStream<LocationsFile>(fs).locations);
                    }
                    else if (file.Name.StartsWith("visits"))
                    {
                        using (var fs = new StreamReader(file.Open(), Encoding.UTF8))
                            Visits.Add(JsonSerializers.DeserializeStream<VisitsFile>(fs).visits);
                    }
                }
                Console.WriteLine($"data loaded in {(DateTime.Now.Ticks - started) / 10000} ms");
                return new InmemoryDatabase(Users, Locations, Visits);
            }
        }
    }
}
