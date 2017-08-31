using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace shared.Services
{
    public static class Util
    {

        public static string Validate(this string s,string pattern)
        {
            if (Regex.IsMatch(s, pattern)) return s;else throw new Exception("pattern mismatch");
        }
        public static int ParseInt(this string s)
        {
            return int.Parse(s);
        }
        

        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            //Моментом начала отсчёта считается полночь (00:00:00 UTC) 1 января 1970 года
            return (int)((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static int GetAge(int current, int birth_date)
        {
            double sec = (long)current - birth_date;
            return (int)Math.Truncate(sec / 31557600);
        }
    }
}
