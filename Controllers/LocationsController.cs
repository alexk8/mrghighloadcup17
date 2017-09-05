using Newtonsoft.Json.Linq;
using shared.Services;
using shared.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using websrv1.Models;

namespace websrv1.Controllers
{
    [Route("[controller]")]
    public class LocationsController : ControllerBase
    {
        public LocationsController(IDatabase db):base(db)
        {
        }

        [HttpGet("{id:int}")]
        public object Get(uint id)
        {
            return (object)db.Locations[id];// ?? NotFound();
        }

        [HttpGet("{id:int}/avg")]
        public object GetAvg(uint id,[FromQuery]SearchRequest q)
        {
            if (!q.IsValid) return BadRequest();

            var location = db.Locations[id];
            if (location == null) return null;// NotFound();

            IEnumerable<Visit> visits = location.GetVisits()
              .Where(v =>
                 (!q.fromDate.HasValue || v.visited_at > q.fromDate.Value)
                 && (!q.toDate.HasValue || v.visited_at < q.toDate.Value)
                 && (!q.fromAge.HasValue || Util.GetAge(currentTime, v.UserRef.birth_date) >= q.fromAge)
                 && (!q.toAge.HasValue || Util.GetAge(currentTime, v.UserRef.birth_date) < q.toAge)
                 && (string.IsNullOrEmpty(q.gender) || v.UserRef.gender == q.gender)
                 );
            
            return Json(new { avg = Math.Round(visits.Average(v => (int?)v.mark)??0, 5) });
        }

        //EXTRA!!!
        [HttpGet("{id:int}/visits")]
        public object GetVisits(uint id, [FromQuery]SearchRequest q)
        {
            if(!q.IsValid) return BadRequest();

            var location = db.Locations[id];
            if (location == null) return null;// NotFound();

            IEnumerable<Visit> visits = location.GetVisits()
                .Where(v=>true
                  && (!q.fromDate.HasValue|| v.visited_at > q.fromDate)
                  && (!q.toDate.HasValue||v.visited_at < q.toDate)
                  && (string.IsNullOrEmpty(q.country)||v.LocationRef?.country == q.country)
                  && (!q.toDistance.HasValue||v.LocationRef?.distance < q.toDistance)
                  && (!q.fromAge.HasValue||Util.GetAge(currentTime,v.UserRef.birth_date) >=  q.fromAge)
                  && (!q.toAge.HasValue||Util.GetAge(currentTime,v.UserRef.birth_date) <  q.toAge)
                  && (string.IsNullOrEmpty(q.gender)||v.UserRef.gender == q.gender)
                  );

            return Json(new { visits = visits.OrderBy(v => v.visited_at).ToList() });
        }


        [HttpPost("{id:int}")]
        public object Update([FromRoute]uint id,[FromBody]JObject json)
        {
            foreach (var t in json)
                if (t.Value.Type == JTokenType.Null)
                    return BadRequest();

            var obj = db.Locations[id];
            if (obj == null) return null; //NotFound();
            //if (!ModelState.IsValid) return BadRequest();
            obj.updateFrom(json);
            return emptyJSONObj;
        }
        [HttpPost("new")]
        public object Insert([FromBody]Location loc)
        {
            if (!loc.Valid) return BadRequest();
            db.Locations[loc.id] = loc;
            return emptyJSONObj;
        }


    }
}
