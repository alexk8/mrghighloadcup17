//using NetJSON;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace shared.Services
{
    public class JsonSerializers
    {
        public static T DeserializeUtf8Stream<T>(Stream strm)
        {
            using (var rdr = new StreamReader(strm, Encoding.UTF8))
                return DeserializeStream<T>(rdr);
        }
        public static T DeserializeStream<T>(TextReader reader)
        {
            //return NetJSON.NetJSON.Deserialize<T>(reader); //50% faster
            return new JsonSerializer().Deserialize<T>(new JsonTextReader(reader));
        }

        public static T Deserialize<T>(string s)
        {
            //return NetJSON.NetJSON.Deserialize<T>(s);
            return JsonConvert.DeserializeObject<T>(s);
        }
        public static string Serialize<T>(T o)
        {
            //return NetJSON.NetJSON.Serialize<T>(o);
            return JsonConvert.SerializeObject(o);
        }
        public static string SerializeUnknown(object o)
        {
            //NetJSON.ne
            //return NetJSON.NetJSON.Serialize(o);
            return JsonConvert.SerializeObject(o);

        }
    }
}
