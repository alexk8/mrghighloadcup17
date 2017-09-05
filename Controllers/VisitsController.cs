using Newtonsoft.Json.Linq;
using shared.Services;
using shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace websrv1.Controllers
{
    [Route("[controller]")]
    public class VisitsController : ControllerBase
    {
        public VisitsController(IDatabase db):base(db)
        {
        }

        [HttpGet("{id:int}")]
        public object Get(uint id)
        {
            return (object)db.Visits[id];//?? NotFound();
        }

        [HttpPost("{id:int}")]
        public object Update([FromRoute]uint id,[FromBody]JObject json)
        {
            foreach (var t in json)
                if (t.Value.Type == JTokenType.Null)
                    return BadRequest();
            var obj = db.Visits[id];
            if (obj == null) return null; // NotFound();
            //if (!ModelState.IsValid) return BadRequest();


            Location newLocation = null;
            if (json["location"] != null && json["location"].Value<uint>() != obj.location)
            {
                newLocation = db.Locations[json["location"].Value<uint>()];
                if (newLocation == null) return BadRequest();
            }
            User newUser = null;
            if (json["user"] != null && json["user"].Value<uint>() != obj.user)
            {
                newUser = db.Users[json["user"].Value<uint>()];
                if (newUser == null) return BadRequest();
            }
            obj.updateFrom(json,
                newLocation,
                newUser
                );
            return emptyJSONObj;
        }
        [HttpPost("new")]
        public object Insert([FromBody]Visit visit)
        {
            if (!visit.Valid) return BadRequest();

            User user = db.Users[visit.user];
            Location location = db.Locations[visit.location];
            if (user == null || location == null) return BadRequest();

            db.Visits[visit.id] = visit;

            visit.UserRef = user;
            visit.LocationRef = location;
            user.AddVisit(visit);
            location.AddVisit(visit);

            return emptyJSONObj;
            
        }

    }
}
