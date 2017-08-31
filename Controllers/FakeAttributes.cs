using System;
using System.Collections.Generic;
using System.Text;

namespace websrv1.Controllers
{
    [AttributeUsage(AttributeTargets.All)]
    class RouteAttribute : Attribute
    {
        public RouteAttribute(string route)
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    class HttpGetAttribute : Attribute
    {
        public HttpGetAttribute(string route)
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    class HttpPostAttribute : Attribute
    {
        public HttpPostAttribute(string route)
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    class FromBodyAttribute : Attribute
    {
        public FromBodyAttribute()
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    class FromQueryAttribute : Attribute
    {
        public FromQueryAttribute()
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    class FromRouteAttribute : Attribute
    {
        public FromRouteAttribute()
        {

        }
    }

}
