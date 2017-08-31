using shared;
using shared.Entities;
using shared.Services;

namespace websrv1.Controllers
{
    //replacement
    public class Controller
    {
        public object NotFound()
        {
            return new HttpError(404);
        }
        public object BadRequest()
        {
            return new HttpError(400);
        }
        public string Json(object o)
        {
            if(o is IEntity)
            {
                string json = (o as IEntity).jsonCached;
                if (json != null) return json;
            }
            return JsonSerializers.SerializeUnknown(o);
        }
    }
}
