/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;

namespace Ruby.Methods
{  
    internal class time_s_times : MethodBody0 //author: war, status: done
    {
        internal static time_s_times singleton = new time_s_times();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {          
            Errors.rb_warn("obsolete method Time::times; use Process::times");
            return rb_proc_times.singleton.Call0(last_class, recv, caller, block);
        }
    }

    
    internal class time_s_mktime : VarArgMethodBody0 //author: war, status: done
    {
        internal static time_s_mktime singleton = new time_s_mktime();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {            
            return Time.time_utc_or_local(rest, false, caller);
        }
    }



    
    internal class time_s_mkutc : VarArgMethodBody0 //author: war, status: done
    {
        internal static time_s_mkutc singleton = new time_s_mkutc();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return Time.time_utc_or_local(rest, true, caller);
        }
    }



    
    internal class time_s_at : VarArgMethodBody0 //author: war, status: done
    {
        internal static time_s_at singleton = new time_s_at();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object time, t;            
            long seconds = 0;
            long uSeconds = 0;

            if (Class.rb_scan_args(caller, rest, 1, 1, false) == 2)
            {
                time = rest[0];
                t = rest[1];
                seconds = Numeric.rb_num2long(rest[0], caller);
                uSeconds = Numeric.rb_num2long(rest[1], caller);
            }
            else 
            {
                time = rest[0];
                Time.rb_time_timeval(rest[0], out seconds, out uSeconds, caller);
            }

            t = Time.time_new_internal(seconds, uSeconds, caller);
            if (time is Time)
            {
                ((Time)t).gmt = ((Time)time).gmt;
            }
            else
            {
                ((Time)t).gmt = false;
                ((Time)t).value = ((Time)t).value.ToLocalTime();
            }

            return t;
        }
    }


    
    internal class time_s_alloc : MethodBody0 //author: war, status: done
    {
        internal static time_s_alloc singleton = new time_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Time tobj = new Time((Class)recv);
            tobj.tm_got = false;

            return tobj;                     
        }
    }

    

    internal class time_strftime : MethodBody1 //author: war, status: done
    {
        internal static time_strftime singleton = new time_strftime();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object format)
        {

            //static VALUE
//time_strftime(time, format)
//    VALUE time, format;
//{

//    struct time_object *tobj;
//    char buffer[SMALLBUF];
//    char *fmt, *buf = buffer;
//    long len;
//    VALUE str;
//
//    GetTimeval(time, tobj);
//    if (tobj->tm_got == 0) {
//        time_get_tm(time, tobj->gmt);
//    }
//    StringValue(format);
//    fmt = RSTRING(format)->ptr;
//    len = RSTRING(format)->len;
//    if (len == 0) {
//        rb_warning("strftime called with empty format string");
//    }
//    else if (strlen(fmt) < len) {
//        /* Ruby string may contain \0's. */
//        char *p = fmt, *pe = fmt + len;
//
//        str = rb_str_new(0, 0);
//        while (p < pe) {
//            len = rb_strftime(&buf, p, &tobj->tm);
//            rb_str_cat(str, buf, len);
//            p += strlen(p) + 1;
//            if (p <= pe)
//                rb_str_cat(str, "\0", 1);
//            if (buf != buffer) {
//                free(buf);
//                buf = buffer;
//            }
//        }
//        return str;
//    }
//    else {
//        len = rb_strftime(&buf, RSTRING(format)->ptr, &tobj->tm);
//    }
//    str = rb_str_new(buf, len);
//    if (buf != buffer) free(buf);
//    return str;
//}
            object str;
            string fmt;
            string buf = "";
            
            if (!((Time)recv).tm_got)
            {
                Time.time_get_tm(((Time)recv), ((Time)recv).gmt, caller);
            }
            fmt = String.StringValue(format, caller);
            if (fmt.Length == 0)
            {
                Errors.rb_warning("strftime called with empty format string");
            }
            else
            {
                buf =  Time.strftime(fmt, ((Time)recv).value);             }         
  
            str =  new String(buf);
            return str;
        }
    }

    
    internal class time_usec : MethodBody0 //author: war, status: done 
    {
        internal static time_usec singleton = new time_usec();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            System.DateTime time = ((Time)recv).value;
            long ticks = time.Ticks;
            //Remove the seconds component of the ticks
            ticks = ticks % 10000000; 
            //Convert ticks (1x10^-7 of a second precision) to usec (1x10^-6 of a second precision)
            long usecs = ticks / 10;

            try
            {
                return checked((int)usecs);
            }
            catch (System.OverflowException)
            {
                return new Bignum(usecs);
            }
        }
    }

    
    internal class time_utc_p : MethodBody0 //author: war, status: done
    {
        internal static time_utc_p singleton = new time_utc_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {

            if (((Time)recv).gmt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    
    internal class time_utc_offset : MethodBody0 //author: war, status: done
    {
        internal static time_utc_offset singleton = new time_utc_offset();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!((Time)recv).tm_got)
            {
                Time.time_get_tm(((Time)recv), ((Time)recv).gmt, caller);
            }
            if (((Time)recv).gmt)
            {
                return 0;
            }
            else
            {
                int off;

                System.DateTime l = ((Time)recv).value;
                System.DateTime u = ((Time)recv).value.ToUniversalTime();                               
                if (l.Year != u.Year)
                    off = l.Year < u.Year ? -1 : 1;
                else if (l.Month != u.Month)
                    off = l.Month < u.Month ? -1 : 1;
                else if (l.Day != u.Day)
                    off = l.Day < u.Day ? -1 : 1;
                else
                    off = 0;
                off = off * 24 + l.Hour - u.Hour;
                off = off * 60 + l.Minute - u.Minute;
                off = off * 60 + l.Second - u.Second;
                return off;
            }
        }
    }

    
    internal class time_zone : MethodBody0 //author: war, status: done
    {
        internal static time_zone singleton = new time_zone();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!((Time)recv).tm_got)
            {
                Time.time_get_tm(((Time)recv), ((Time)recv).gmt, caller);
            }

            if (((Time)recv).gmt)
            {
                return new String("UTC");
            }
            return new String(Time.strftime("%Z", ((Time)recv).value));
        }
    }


    
    internal class time_to_a : MethodBody0 //author: war, status: done
    {
        internal static time_to_a singleton = new time_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!((Time)recv).tm_got)
            {
                Time.time_get_tm(((Time)recv), ((Time)recv).gmt, caller);
            }
            return new Array(
                ((Time)recv).value.Second,
                ((Time)recv).value.Minute,
                ((Time)recv).value.Hour,
                ((Time)recv).value.Day,
                ((Time)recv).value.Month,
                ((Time)recv).value.Year,
                (int)((Time)recv).value.DayOfWeek,
                ((Time)recv).value.DayOfYear,
                ((Time)recv).value.IsDaylightSavingTime(),
                time_zone.singleton.Call0(last_class, recv, caller, null));
        }
    }

    
    internal class time_to_s : MethodBody0 //author: war, status: done
    {
        internal static time_to_s singleton = new time_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string buf;

            if (!((Time)recv).tm_got)
            {
                Time.time_get_tm((Time)recv, ((Time)recv).gmt, caller);
            }
            if (((Time)recv).gmt == true)
            {
                buf = Time.strftime("%a %b %d %H:%M:%S UTC %Y", ((Time)recv).value);
            }
            else
            {
                buf = Time.strftime("%a %b %d %H:%M:%S %Z %Y", ((Time)recv).value);
            }
            return new String(buf);
        }
    }


    
    internal class time_asctime : MethodBody0 //author: war, status: done 
    {
        internal static time_asctime singleton = new time_asctime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Time tobj = (Time)recv;
            if (!tobj.tm_got)
            {
                Time.time_get_tm(tobj, tobj.gmt, caller);
            }

            System.DateTime time = ((Time)recv).value;
            string ascTime;
            ascTime = time.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture);
            return new String(ascTime);
        }
    }


    
    internal class time_getgmtime : MethodBody0 //author: war, status: done 
    {
        internal static time_getgmtime singleton = new time_getgmtime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return time_gmtime.singleton.Call0(last_class, Time.time_dup((Time)recv, caller, null), caller, null);
        }
    }


    
    internal class time_getlocaltime : MethodBody0 //author: war, status: done 
    {
        internal static time_getlocaltime singleton = new time_getlocaltime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return time_localtime.singleton.Call0(last_class, Time.time_dup((Time)recv, caller, null), caller, null);
        }
    }

    
    internal class time_gmtime : MethodBody0 //author: war, status: done 
    {
        internal static time_gmtime singleton = new time_gmtime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Time tobj = (Time)recv;
            if (tobj.gmt)
            {
                if (tobj.tm_got)
                {
                    return tobj;
                }
            }
            else
            {
                Time.time_modify(caller, tobj);
            }

            System.DateTime time = ((Time)recv).value;
            time = time.ToUniversalTime();
            ((Time)recv).value = time;
            ((Time)recv).gmt = true;
            ((Time)recv).tm_got = true;
            return recv;
        }
    }


    
    internal class time_localtime : MethodBody0 //author: war, status: done 
    {
        internal static time_localtime singleton = new time_localtime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {            
            Time tobj = (Time)recv;
            if (!tobj.gmt)
            {
                if (tobj.tm_got)
                {
                    return tobj;
                }
            }
            else
            {
                Time.time_modify(caller, tobj);
            }

            System.DateTime time = ((Time)recv).value;
            time = time.ToLocalTime();
            ((Time)recv).value = time;
            ((Time)recv).gmt = false;
            ((Time)recv).tm_got = true;
            return recv;
        }
    }


    
    internal class time_init_copy : MethodBody1 //author: war, status: done
    {
        internal static time_init_copy singleton = new time_init_copy();

        public override object Call1(Class last_class, object copy, Frame caller, Proc block, object time)
        {
            if (copy == time) return copy;
            Time.time_modify(caller, (Time)copy);
            if(!(time is Time))
            {
                throw new TypeError("wrong argument type").raise(caller);
            }
            ((Time)copy).value = ((Time)time).value;
            ((Time)copy).tm_got = ((Time)time).tm_got;
            ((Time)copy).gmt = ((Time)time).gmt;
            return copy;
        }
    }

    
    internal class time_init : MethodBody0 //author: war, status: done
    {
        internal static time_init singleton = new time_init();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Time.time_modify(caller, (Time)recv);            
            Time time = (Time)recv;
            time.tm_got = false;
            time.value = System.DateTime.Now;
            return time;
        }
    }

    
    internal class time_cmp : MethodBody1 //author: war, status: done
    {
        internal static time_cmp singleton = new time_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            System.DateTime time1, time2;
            long time1Ticks, time2Ticks;
            long time1MicroSeconds, time2MicroSeconds;

            if (param0 is Time)
            {
                //Ruby's Time class precision is microseconds (1x10^-6).
                //The DateTime class precision is 100 nanoseconds (100x10^-9 = 1x10^-7)
                time1 = ((Time)recv).value;
                time2 = ((Time)param0).value;
                time1Ticks = time1.Ticks;
                time2Ticks = time2.Ticks;
                //Losing precision on purpose for comparison
                time1MicroSeconds = time1Ticks / 10;
                time2MicroSeconds = time2Ticks / 10;

                if (time1MicroSeconds == time2MicroSeconds)
                {
                    return 0;
                }
                if (time1MicroSeconds > time2MicroSeconds)
                {
                    return 1;
                }
                return -1;
            }

            //Ruby 1.8.2 implementation returns a null every time a Time object
            //is compared with a Numeric value. 
            return null;
        }

    }

    
    internal class time_eql : MethodBody1 //author: war, status: done
    {
        internal static time_eql singleton = new time_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            System.DateTime time1, time2;
            long time1Ticks, time2Ticks;
            long time1MicroSeconds, time2MicroSeconds;

            if (param0 is Time)
            {
                //Ruby's Time class precision is microseconds (1x10^-6).
                //The DateTime class precision is 100 nanoseconds (100x10^-9 = 1x10^-7)
                time1 = ((Time)recv).value;
                time2 = ((Time)param0).value;
                time1Ticks = time1.Ticks;
                time2Ticks = time2.Ticks;
                //Losing precision on purpose for comparison
                time1MicroSeconds = time1Ticks / 10;
                time2MicroSeconds = time2Ticks / 10;

                if (time1MicroSeconds == time2MicroSeconds)
                {
                    return true;
                }
            }

            return false;
        }
    }

    
    internal class time_plus : MethodBody1 //status: done
    {
        internal static time_plus singleton = new time_plus();

        public override object Call1(Class last_class, object time1, Frame caller, Proc block, object time2)
        {
            Time tobj = (Time) time1;
            if (time2 is Time)
            {
                throw new TypeError("time + time?").raise(caller);
            }

            return Time.time_add(tobj, time2, 1, caller);
        }
    }

    
    internal class time_minus : MethodBody1 //status: done
    {
        internal static time_minus singleton = new time_minus();

        public override object Call1(Class last_class, object time1, Frame caller, Proc block, object time2)
        {
            Time tobj = (Time)time1;

            if (time2 is Time)
            {
                long sec, usec;
                Time.GetParts(tobj, out sec, out usec);

                long sec2, usec2;
                Time.GetParts((Time)time2, out sec2, out usec2);

                double f; // result, in floating-point seconds
                f = sec - sec2;
                f += (usec - usec2) / 1e6;
                
                return new Float(f);
            }

            return Time.time_add(tobj, time2, -1, caller);
        }
    }

    
    internal class time_succ : MethodBody0 //status: done
    {
        internal static time_succ singleton = new time_succ();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Time(((Time)recv).value.AddSeconds(1));
        }
    }

    
    internal class time_sec : MethodBody0 //status: done
    {
        internal static time_sec singleton = new time_sec();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Second;
        }
    }

    
    internal class time_min : MethodBody0 //status: done
    {
        internal static time_min singleton = new time_min();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Minute;
        }
    }

    
    internal class time_hour : MethodBody0 //status: done
    {
        internal static time_hour singleton = new time_hour();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Hour;
        }
    }

    
    internal class time_mon : MethodBody0 //status: done
    {
        internal static time_mon singleton = new time_mon();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Month;
        }
    }

    
    internal class time_year : MethodBody0 //status: done
    {
        internal static time_year singleton = new time_year();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Year;
        }
    }

    
    internal class time_mday : MethodBody0 //status: done
    {
        internal static time_mday singleton = new time_mday();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.Day;
        }
    }

    
    internal class time_wday : MethodBody0 //status: done
    {
        internal static time_wday singleton = new time_wday();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return (int)((Time)recv).value.DayOfWeek;
        }
    }

    
    internal class time_yday : MethodBody0 //status: done
    {
        internal static time_yday singleton = new time_yday();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.DayOfYear;
        }
    }

    
    internal class time_isdst : MethodBody0 //status: done
    {
        internal static time_isdst singleton = new time_isdst();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Time)recv).value.IsDaylightSavingTime();
        }
    }

    
    internal class time_hash : MethodBody0 //status: done
    {
        internal static time_hash singleton = new time_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int hash;
            
            int secs = (int)time_to_i.singleton.Call0(last_class, recv, caller, block);
            int milli = (int)((Time)recv).value.Millisecond;

            hash = secs ^ milli;

            return hash;
        }
    }

    
    internal class time_to_i : MethodBody0 //status: done
    {
        internal static time_to_i singleton = new time_to_i();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return (int)((Time)recv).value.Subtract(Time.GetEpoch((Time)recv)).TotalSeconds;
        }
    }

    
    internal class time_to_f : MethodBody0 //status: done
    {
        internal static time_to_f singleton = new time_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float(((Time)recv).value.Subtract(Time.GetEpoch((Time)recv)).TotalSeconds);
        }
    }

    
    internal class time_dump : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static time_dump singleton = new time_dump();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            Time time = (Time)recv;
            String str;

            Class.rb_scan_args(caller, argv, 0, 1, false);
            str = Time.time_mdump(caller, time);

            if (Object.exivar_p(time))
            {
                Object.rb_copy_generic_ivar(str, time);
                //    FL_SET(str, FL_EXIVAR);
            }

            return str;
        }
    }


    
    internal class time_load : MethodBody1 // author: cjs, status: done
    {
        internal static time_load singleton = new time_load();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Time time = (Time)time_s_alloc.singleton.Call0(last_class, recv, caller, null);
            String str = (String)p1;

            if (Object.exivar_p(str))
            {
                Object.rb_copy_generic_ivar(time, str);
                //    FL_SET(time, FL_EXIVAR);
            }
            
            Time.time_mload(caller, time, str);

            return time;
        }
    }
}
