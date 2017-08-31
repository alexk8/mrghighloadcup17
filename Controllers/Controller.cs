using shared;
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
            return JsonSerializers.SerializeUnknown(o);
        }
    }
}
