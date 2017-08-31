using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class HttpError:Exception
    {
        public int code { get; private set; }
        public HttpError(int code):base("http error "+code)
        {
            this.code = code;
        }
    }
}
