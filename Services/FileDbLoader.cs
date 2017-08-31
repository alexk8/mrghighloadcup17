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
    public class DataFile
    {
        public List<User> users = null;
        public List<Location> locations = null;
        public List<Visit> visits = null;
    }

    public class FileDbLoader
    {
        public static List<DataFile> Load(string zipFn)
        {
            using (ZipArchive zip = ZipFile.OpenRead(zipFn))
            {
                return zip.Entries
                    .Where(file => file.Name.StartsWith("users") || file.Name.StartsWith("locations") || file.Name.StartsWith("visits"))
                    .Select(file => JsonSerializers.DeserializeUtf8Stream<DataFile>(file.Open()))
                    .ToList();
            }
        }
    }
}
