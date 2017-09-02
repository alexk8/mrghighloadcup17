using Newtonsoft.Json.Linq;
using shared.Services;
using shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using websrv1.Models;

namespace websrv1.Controllers
{
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        public UsersController(IDatabase db) : base(db)
        {
        }

        [HttpGet("{id:int}")]
        public object Get(uint id)
        {
            return (object)db.find<User>(id) ?? NotFound();
        }

        [HttpGet("{id:int}/visits")]
        public object GetVisits(uint id, [FromQuery]SearchRequest q)
        {
            if (!q.IsValid) return BadRequest();

            var user = db.find<User>(id);
            if (user == null) return NotFound();

            IEnumerable<Visit> visits = user.GetVisits()
                .Where(v => true
                  && (!q.fromDate.HasValue || v.visited_at > q.fromDate)
                  && (!q.toDate.HasValue || v.visited_at < q.toDate)
                  && (string.IsNullOrEmpty(q.country) || v.LocationRef?.country == q.country)
                  && (!q.toDistance.HasValue || v.LocationRef?.distance < q.toDistance)
                  //&& (!q.fromAge.HasValue || currentTime - v.UserRef.birth_date > q.fromAge * 31557600)
                  //&& (!q.toAge.HasValue || currentTime - v.UserRef.birth_date < q.toAge * 31557600)
                  //&& (string.IsNullOrEmpty(q.gender) || v.UserRef.gender == q.gender)
                  );

            var v_join = visits.Select(v=>new {mark=v.mark, visited_at = v.visited_at, place=v.LocationRef?.place});

            return Json(new { visits = v_join.OrderBy(v => v.visited_at).ToList() });

        }


        [HttpPost("{id:int}")]
        public object Update([FromRoute]uint id, [FromBody]JObject json)
        {
            foreach (var t in json)
                if (t.Value.Type == JTokenType.Null)
                    return BadRequest();

            var obj = db.find<User>(id);
            if (obj == null) return NotFound();
            //if (!ModelState.IsValid) return BadRequest();
            obj.updateFrom(json);
            return emptyJSONObj;
        }

        [HttpPost("new")]
        public object Insert([FromBody]User user)
        {
            if (!user.Valid) return BadRequest();
            if (db.insert(user)) return emptyJSONObj;
            else return BadRequest();
        }

    }
}
