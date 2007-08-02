using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Time
    {
         [InteropMethod("_load")]
        public static object _load(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cTime, "_load", p1);
        }

         [InteropMethod("at")]
        public static object at(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cTime, "at", args);
        }

         [InteropMethod("gm")]
        public static object gm(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cTime, "gm", args);
        }

         [InteropMethod("local")]
        public static object local(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cTime, "local", args);
        }

         [InteropMethod("mktime")]
        public static object mktime(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cTime, "mktime", args);
        }

         [InteropMethod("now")]
        public static object now(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cTime, "now", args);
        }

         [InteropMethod("times")]
        public static object times()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cTime, "times");
        }

         [InteropMethod("+")]
        public static object operator+(Time p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("-")]
        public static object operator-(Time p1, object p2)
        {
            return Eval.Call1(p1, "-", p2);
        }

         [InteropMethod("<")]
        public static object operator<(Time p1, object p2)
        {
            return Eval.Call1(p1, "<", p2);
        }

         [InteropMethod("<=")]
        public static object operator<=(Time p1, object p2)
        {
            return Eval.Call1(p1, "<=", p2);
        }

         [InteropMethod("<=>")]
        public object cmp(object p1)
        {
            return Eval.Call1(this, "<=>", p1);
        }

         [InteropMethod(">")]
        public static object operator>(Time p1, object p2)
        {
            return Eval.Call1(p1, ">", p2);
        }

         [InteropMethod(">=")]
        public static object operator>=(Time p1, object p2)
        {
            return Eval.Call1(p1, ">=", p2);
        }

         [InteropMethod("_dump")]
        public object _dump(params object[] args)
        {
            return Eval.Call(this, "_dump", args);
        }

         [InteropMethod("asctime")]
        public object asctime()
        {
            return Eval.Call0(this, "asctime");
        }

         [InteropMethod("between?")]
        public object is_between(object p1, object p2)
        {
            return Eval.Call2(this, "between?", p1, p2);
        }

         [InteropMethod("ctime")]
        public object ctime()
        {
            return Eval.Call0(this, "ctime");
        }

         [InteropMethod("day")]
        public object day()
        {
            return Eval.Call0(this, "day");
        }

         [InteropMethod("dst?")]
        public object is_dst()
        {
            return Eval.Call0(this, "dst?");
        }

         [InteropMethod("getgm")]
        public object getgm()
        {
            return Eval.Call0(this, "getgm");
        }

         [InteropMethod("getlocal")]
        public object getlocal()
        {
            return Eval.Call0(this, "getlocal");
        }

         [InteropMethod("getutc")]
        public object getutc()
        {
            return Eval.Call0(this, "getutc");
        }

         [InteropMethod("gmt?")]
        public object is_gmt()
        {
            return Eval.Call0(this, "gmt?");
        }

         [InteropMethod("gmt_offset")]
        public object gmt_offset()
        {
            return Eval.Call0(this, "gmt_offset");
        }

         [InteropMethod("gmtime")]
        public object gmtime()
        {
            return Eval.Call0(this, "gmtime");
        }

         [InteropMethod("gmtoff")]
        public object gmtoff()
        {
            return Eval.Call0(this, "gmtoff");
        }

         [InteropMethod("hour")]
        public object hour()
        {
            return Eval.Call0(this, "hour");
        }

         [InteropMethod("isdst")]
        public object isdst()
        {
            return Eval.Call0(this, "isdst");
        }

         [InteropMethod("localtime")]
        public object localtime()
        {
            return Eval.Call0(this, "localtime");
        }

         [InteropMethod("mday")]
        public object mday()
        {
            return Eval.Call0(this, "mday");
        }

         [InteropMethod("min")]
        public object min()
        {
            return Eval.Call0(this, "min");
        }

         [InteropMethod("mon")]
        public object mon()
        {
            return Eval.Call0(this, "mon");
        }

         [InteropMethod("month")]
        public object month()
        {
            return Eval.Call0(this, "month");
        }

         [InteropMethod("sec")]
        public object sec()
        {
            return Eval.Call0(this, "sec");
        }

         [InteropMethod("strftime")]
        public object strftime(object p1)
        {
            return Eval.Call1(this, "strftime", p1);
        }

         [InteropMethod("succ")]
        public object succ()
        {
            return Eval.Call0(this, "succ");
        }

         [InteropMethod("to_f")]
        public object to_f()
        {
            return Eval.Call0(this, "to_f");
        }

         [InteropMethod("to_i")]
        public object to_i()
        {
            return Eval.Call0(this, "to_i");
        }

         [InteropMethod("tv_sec")]
        public object tv_sec()
        {
            return Eval.Call0(this, "tv_sec");
        }

         [InteropMethod("tv_usec")]
        public object tv_usec()
        {
            return Eval.Call0(this, "tv_usec");
        }

         [InteropMethod("usec")]
        public object usec()
        {
            return Eval.Call0(this, "usec");
        }

         [InteropMethod("utc")]
        public object utc()
        {
            return Eval.Call0(this, "utc");
        }

         [InteropMethod("utc?")]
        public object is_utc()
        {
            return Eval.Call0(this, "utc?");
        }

         [InteropMethod("utc_offset")]
        public object utc_offset()
        {
            return Eval.Call0(this, "utc_offset");
        }

         [InteropMethod("wday")]
        public object wday()
        {
            return Eval.Call0(this, "wday");
        }

         [InteropMethod("yday")]
        public object yday()
        {
            return Eval.Call0(this, "yday");
        }

         [InteropMethod("year")]
        public object year()
        {
            return Eval.Call0(this, "year");
        }

         [InteropMethod("zone")]
        public object zone()
        {
            return Eval.Call0(this, "zone");
        }

    }
}

