using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace shared.Services
{
    static class valid
    {
        public static bool MaxLenghth(string s,int len)
        {
            return s.Length <= len;
        }
        public static bool Expr(string s,string pattern)
        {
            return Regex.IsMatch(s, pattern);

        }
        public static bool Range(long val,long min,long max)
        {
            return val >= min && val <= max;
        }
        public static bool EMail(string s)
        {
            return Expr(s, ".+@.+\\..+");
        }
    }
}
