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

    public class TaskFilesData
    {
        public List<DataFile> files;
        public int? currentTime;
    }

    public class FileDbLoader
    {
        public static TaskFilesData Load(string zipFn)
        {
            using (ZipArchive zip = ZipFile.OpenRead(zipFn))
            {
                return new TaskFilesData {
                    files = zip.Entries
                        .Where(file => file.Name.StartsWith("users") || file.Name.StartsWith("locations") || file.Name.StartsWith("visits"))
                        .Select(file => JsonSerializers.DeserializeUtf8Stream<DataFile>(file.Open()))
                        .ToList(),
                    currentTime = GetTimestampFrom(zip.Entries.SingleOrDefault(file => file.Name == "options.txt"))
                    };
            }
        }

        private static int? GetTimestampFrom(ZipArchiveEntry file)
        {
            if (file == null) return null;
            using (var rdr = new StreamReader(file.Open(), Encoding.UTF8))
            {
                Console.WriteLine("zip/options found");
                return rdr.ReadLine().ParseInt();
            }
        }




    }
}
