using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using shared.Services;
using shared;
using Microsoft.AspNetCore.Http;

namespace websrv1.Models
{
    public class SearchRequest
    {
        public SearchRequest(IQueryCollection query)
        {
            try
            {
                
                fromDate = query["fromDate"].FirstOrDefault()?.ParseInt();
                toDate = query["toDate"].FirstOrDefault()?.ParseInt();
                fromAge  = query["fromAge"].FirstOrDefault()?.ParseInt();
                toAge  = query["toAge"].FirstOrDefault()?.ParseInt();
                toDistance = query["toDistance"].FirstOrDefault()?.ParseInt();

                gender = query["gender"].FirstOrDefault()?.Validate("^[fm]$");
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
