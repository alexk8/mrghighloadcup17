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
            return (object)db.find<Visit>(id) ?? NotFound();
        }

        [HttpPost("{id:int}")]
        public object Update([FromRoute]uint id,[FromBody]JObject json)
        {
            foreach (var t in json)
                if (t.Value.Type == JTokenType.Null)
                    return BadRequest();
            var obj = db.find<Visit>(id);
            if (obj == null) return NotFound();
            //if (!ModelState.IsValid) return BadRequest();


            Location newLocation = null;
            if (json["location"] != null && json["location"].Value<uint>() != obj.location)
            {
                newLocation = db.find<Location>(json["location"].Value<uint>());
                if (newLocation == null) return BadRequest();
            }
            User newUser = null;
            if (json["user"] != null && json["user"].Value<uint>() != obj.user)
            {
                newUser = db.find<User>(json["user"].Value<uint>());
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
            if (db.insert(visit)) return emptyJSONObj;
            else return BadRequest();
        }

    }
}
