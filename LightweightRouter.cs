using System;
using System.Collections.Specialized;
using websrv1.Controllers;
using Newtonsoft.Json;
using websrv1.Models;
using Newtonsoft.Json.Linq;
using shared.Services;
using shared.Entities;
using shared;
using srvkestrel;
using Microsoft.AspNetCore.Http;

namespace websrv1
{
    class LightweightRouter
    {
        //private IDatabase db;

        public LightweightRouter(IDatabase db)
        {
            //this.db = db;
            users = new UsersController(db);
            locations = new LocationsController(db);
            visits = new VisitsController(db);
        }


        private Resp toJson(object val)
        {
            if (val == null) return new Resp { code = 404 };
            else if (val is IEntity)
            {
                byte[] json = (val as IEntity).jsonCached;
                if (json != null) return new Resp { body = json };
            }
            else if (val is string) return new Resp { bodyStr = (string)val };
            else if (val is HttpError) return new Resp { code = (val as HttpError).code };
            else if (val is byte[]) return new Resp { body = (byte[])val };
            return new Resp { bodyStr = JsonSerializers.SerializeUnknown(val) };
        }


        UsersController users;
        LocationsController locations;
        VisitsController visits;


        public Resp processGetRequest(string pathStr, IQueryCollection query)
        {
            string[] path = pathStr.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length < 2) return new Resp { code = 404 };
            uint id;
            if (!uint.TryParse(path[1], out id)) return new Resp { code = 404 };
            string tip = path[0];
            string sub = path.Length > 2 ? "/" + path[2] : "";

            //Match m = Regex.Match(path, "^/([a-z]+)/([0-9]+)(/[a-z]+)?");
            //if (!m.Success) throw new HttpError(404);
            //string tip = m.Groups[1].Captures[0].Value;
            //uint id = uint.Parse(m.Groups[2].Captures[0].Value);
            //string sub = m.Groups[3].Captures.FirstOrDefault()?.Value;
            switch (tip + sub)
            {
                case "users":
                    return toJson(users.Get(id));
                case "locations":
                    return toJson(locations.Get(id));
                case "visits":
                    return toJson(visits.Get(id));
                case "users/visits":
                    return toJson(users.GetVisits(id, new SearchRequest(query)));
                case "locations/avg":
                    return toJson(locations.GetAvg(id, new SearchRequest(query)));
                default:
                    return new Resp { code = 404 };
            }
        }



        public Resp processPostRequest(string pathStr, string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr)) return new Resp {code=400};

            //Match m = Regex.Match(path, "^/([a-z]+)/(new|\\d+)");
            //if (!m.Success) return Tuple.Create(404, "");
            //string tip = m.Groups[1].Captures[0].Value;
            //string id = m.Groups[2].Captures[0].Value;

            string[] path = (pathStr??"").Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length != 2) return new Resp { code = 404 };
            string tip = path[0];
            string id = path[1];
            uint id2;
            if (id!="new" && !uint.TryParse(id, out id2)) return new Resp { code = 404 };

            if (id == "new") tip = tip + "/" + id;
            try
            {
                switch (tip)
                {
                    case "visits/new":
                        return toJson(visits.Insert(JsonSerializers.Deserialize<Visit>(jsonStr)));
                    case "visits":
                        return toJson(visits.Update(uint.Parse(id), JObject.Parse(jsonStr)));
                    case "users/new":
                        return toJson(users.Insert(JsonSerializers.Deserialize<User>(jsonStr)));
                    case "users":
                        return toJson(users.Update(uint.Parse(id), JObject.Parse(jsonStr)));
                    case "locations/new":
                        return toJson(locations.Insert(JsonSerializers.Deserialize<Location>(jsonStr)));
                    case "locations":
                        return toJson(locations.Update(uint.Parse(id), JObject.Parse(jsonStr)));
                    default:
                        return new Resp { code = 404 };
                }
            }
            catch (JsonSerializationException)
            {
                return new Resp { code = 400 };
            }
            catch (JsonReaderException)
            {
                return new Resp { code = 400 };
            }
        }
    }
}
