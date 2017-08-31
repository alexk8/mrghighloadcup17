using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using shared.Services;
using shared;

namespace websrv1.Models
{
    public class SearchRequest
    {
        private NameValueCollection query;

        public SearchRequest(NameValueCollection query)
        {
            this.query = query;
            try
            {
                fromDate = query["fromDate"]?.ParseInt();
                toDate = query["toDate"]?.ParseInt();
                fromAge  = query["fromAge"]?.ParseInt();
                toAge  = query["toAge"]?.ParseInt();
                toDistance = query["toDistance"]?.ParseInt();

                gender = query["gender"]?.Validate("^[fm]$");
                country = query["country"];
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }

            // optimization hack
            if (!IsValid) throw new HttpError(400);
        }

        public int? fromDate { get; set; }
        public int? toDate { get; set; }
        public int? fromAge { get; set; }
        public int? toAge { get; set; }
        //[RegularExpression("^[fm]$")]
        public string gender { get; set; }

        public string country { get; set; }
        public int? toDistance { get; set; }
        public bool IsValid { get; internal set; }
    }
}
