using shared;
using shared.Entities;
using shared.Services;
using System;
using System.Text;

namespace websrv1.Controllers
{
    //replacement
    public class ControllerBase
    {
        protected readonly IDatabase db;
        protected readonly int currentTime;

        public ControllerBase(IDatabase db)
        {
            this.db = db;
            currentTime = db.CurrentTime??DateTime.Now.ToUnixTimestamp();
        }


        public object NotFound()
        {
            return new HttpError(404);
        }
        public object BadRequest()
        {
            return new HttpError(400);
        }
        public object Json(object o)
        {
            return o;
            /*
            if(o is IEntity)
            {
                string json = (o as IEntity).jsonCached;
                if (json != null) return json;
            }
            return JsonSerializers.SerializeUnknown(o);
            */
        }

        protected static readonly byte[] emptyJSONObj = Encoding.ASCII.GetBytes("{}");
    }
}
