/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System;

namespace Ruby
{
    using Methods;
    using System.Globalization;


    public partial class Time : Basic
    {
        internal System.DateTime value;
        internal bool tm_got;

        internal bool gmt
        {
            get { return this.value.Kind == DateTimeKind.Utc; }
        }

        //-----------------------------------------------------------------


        internal Time()
            : this(DateTime.Now) //status: done
        {
        }

        public Time(Class klass)
            : base(klass) //status: done
        {
        }


        internal Time(System.DateTime value)
            : base(Ruby.Runtime.Init.rb_cTime) //status: done
        {
            this.value = value;
            this.tm_got = true;
        }

        //-----------------------------------------------------------------

        private static readonly System.DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly System.DateTime LocalEpoch = Epoch.ToLocalTime();
        private static readonly System.DateTime EndTime = new DateTime(2038, 1, 19, 3, 14, 7, DateTimeKind.Utc);
        internal static string[] months = {"jan", "feb", "mar", "apr", "may", "jun",
                "jul", "aug", "sep", "oct", "nov", "dec"};

        //-----------------------------------------------------------------

        internal override object  Inner()
        {
             return value;
        }


        //-----------------------------------------------------------------


        // the value of epoch is different depending on time zone
        internal static System.DateTime GetEpoch(bool utc)
        {
            if (utc)
            {
                return Time.Epoch;
            }
            else
            {
                return Time.Epoch.ToLocalTime();
            }
        }

        // the value of epoch is different depending on time zone
        internal static System.DateTime GetEpoch(Time time)
        {
            if (time.gmt)
            {
                return Time.Epoch;
            }
            else
            {
                return Time.Epoch.ToLocalTime();
            }
        }

        internal static void GetParts(Time time, out long sec, out long usec)
        {
            long ticks = time.value.Ticks - Time.GetEpoch(time).Ticks;
            Time.GetParts(ticks, out sec, out usec);
        }

        internal static void GetParts(long ticks, out long sec, out long usec)
        {
            sec = ticks / (long)1e7;
            usec = (ticks % (long)1e7) / 10;
        }

        internal static long GetTicks(long sec, long usec)
        {
            return sec * (long)1e7 + usec * 10;
        }


        internal static string strftime(string format, System.DateTime dt)
        {
            bool printFormat = false;
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            foreach (char c in format)
            {
                if (!printFormat && c == '%')
                {
                    printFormat = true;
                    continue;
                }

                if (printFormat)
                {
                    switch (c)
                    {
                        case 'a':
                            result.Append(dt.ToString("ddd", CultureInfo.InvariantCulture));
                            break;
                        case 'A':
                            result.Append(dt.ToString("dddd", CultureInfo.InvariantCulture));
                            break;
                        case 'b':
                            result.Append(dt.ToString("MMM", CultureInfo.InvariantCulture));
                            break;
                        case 'B':
                            result.Append(dt.ToString("MMMM", CultureInfo.InvariantCulture));
                            break;
                        case 'c':
                            result.Append(dt.ToString(CultureInfo.InvariantCulture));
                            break;
                        case 'd':
                            result.Append(dt.ToString("dd", CultureInfo.InvariantCulture));
                            break;
                        case 'H':
                            result.Append(dt.ToString("HH", CultureInfo.InvariantCulture));
                            break;
                        case 'I':
                            result.Append(dt.ToString("hh", CultureInfo.InvariantCulture));
                            break;
                        case 'j':
                            string day = dt.DayOfYear.ToString(CultureInfo.InvariantCulture);
                            if (day.Length < 3)
                            {
                                if (day.Length == 1)
                                {
                                    day = "00" + day;
                                }
                                else
                                {
                                    day = "0" + day;
                                }
                            }
                            result.Append(day);
                            break;
                        case 'm':
                            result.Append(dt.ToString("MM", CultureInfo.InvariantCulture));
                            break;
                        case 'M':
                            result.Append(dt.ToString("mm", CultureInfo.InvariantCulture));
                            break;
                        case 'p':
                            result.Append(dt.ToString("tt", CultureInfo.InvariantCulture));
                            break;
                        case 'S':
                            result.Append(dt.ToString("ss", CultureInfo.InvariantCulture));
                            break;
                        case 'U':
                            {
                                System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar();
                                int weekOfYear = cal.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                                if (dt.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    weekOfYear--;
                                }
                                result.Append(weekOfYear.ToString("00", CultureInfo.InvariantCulture));
                            }
                            break;
                        case 'W':
                            {
                                System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar();
                                int weekOfYear = cal.GetWeekOfYear(dt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                if (dt.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    weekOfYear--;
                                }
                                result.Append(weekOfYear.ToString("00", CultureInfo.InvariantCulture));
                            }
                            break;
                        case 'w':
                            result.Append(((int)dt.DayOfWeek).ToString(CultureInfo.InvariantCulture));
                            break;
                        case 'x':
                            result.Append(dt.ToString("d", CultureInfo.InvariantCulture));
                            break;
                        case 'X':
                            result.Append(dt.ToString("t", CultureInfo.InvariantCulture));
                            break;
                        case 'y':
                            result.Append(dt.ToString("yy", CultureInfo.InvariantCulture));
                            break;
                        case 'Y':
                            result.Append(dt.ToString("yyyy", CultureInfo.InvariantCulture));
                            break;
                        case 'Z':
                            {
                                TimeZone localZone = TimeZone.CurrentTimeZone;
                                result.Append(localZone.DaylightName);
                            }
                            break;
                        case '%':
                            result.Append('%');
                            break;
                        default:
                            //invalid format - do nothing
                            //C specification leaves this behaviour undefined 
                            break;
                    }
                    printFormat = false;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        internal static void time_modify(Frame caller, Time time)
        {
            TypeError.rb_check_frozen(caller, time);
            if (!time.Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't modify Time").raise(caller);
        }

        internal static Time time_get_tm(Time time, bool gmt, Frame caller)
        {
            if (gmt) return (Time)Ruby.Methods.time_gmtime.singleton.Call0(null, time, caller, null);
            return (Time)Ruby.Methods.time_localtime.singleton.Call0(null, time, caller, null);
        }

        internal static Time time_dup(Time time, Frame caller, Proc block)
        {
            Time dup = (Time)time_s_alloc.singleton.Call0(null, Ruby.Runtime.Init.rb_cTime, caller, null);
            time_init_copy.singleton.Call1(null, dup, caller, block, time);
            return dup;
        }

        internal static Time time_new_internal(long secs, long usec, bool utc_p, Frame caller)
        {           
            Time.time_overflow_p(ref secs, ref usec, caller);

            long ticks = 0;
            ticks = secs * 10000000;
            ticks += usec * 10;

            DateTime datetime;
            if (utc_p)
                datetime = Time.Epoch.AddTicks(ticks);
            else
                datetime = Time.LocalEpoch.AddTicks(ticks);
            return new Time(datetime);
        }

        internal static System.TimeSpan rb_time_interval(object time, Frame caller)
        {
            long secs, usec;

            time_timeval(time, out secs, out usec, true, caller);

            return new TimeSpan(Time.GetTicks(secs, usec));
        }

        internal static void rb_time_timeval(object time, out long secs, out long usec, Frame caller)
        {

            if (time is Time)
            {
                Time.GetParts((Time)time, out secs, out usec);
                return;
            }

            time_timeval(time, out secs, out usec, false, caller);
        }

        internal static void time_timeval(object time, out long secs, out long usec, bool interval, Frame caller)
        {
            string tstr = interval ? "time interval" : "time";

            interval = true;

            if (time is int)
            {
                secs = (int)time;
                if (interval && secs < 0)
                {
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "{0} must be positive", tstr)).raise(caller);
                }
                usec = 0;

            }
            else if (time is Float)
            {
                if (interval && ((Float)time).value < 0.0)
                {
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "{0} must be positive", tstr)).raise(caller);
                }
                else
                {
                    double f, d;
                    d = Marshal.modf(((Float)time).value, out f);
                    secs = (long)f;
                    if (f != secs)
                    {
                        throw new RangeError(string.Format(CultureInfo.InvariantCulture, "{0} out of Time range", ((Float)time).value)).raise(caller);
                    }
                    usec = (long)(d * 1e6);
                }
            }
            else if (time is Bignum)
            {
                secs = Numeric.rb_num2long(time, caller);
                if (interval && secs < 0)
                {
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "{0} must be positive", tstr)).raise(caller);
                }
                usec = 0;
            }
            else
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "can't convert {0} into {1}", Marshal.rb_obj_classname(time, caller), tstr)).raise(caller);
            }
        }

        internal static String time_mdump(Frame caller, Time timeRecv) // author: war, cjs, status: done
        {
            long p, s;

            DateTime time = timeRecv.value.ToUniversalTime();

            int year = time.Year - 1900; //time is stored internally in ruby as years from 1900
            if ((year & 0xffff) != year) //year must be able to be stored in 16-bits for marshalling purposes
            {
                throw new ArgumentError("year too big to marshal").raise(caller);
            }

            p = 0x1 << 31 | /*  1 */
                year << 14 | /* 16 */
                time.Month - 1 << 10 | /*  4 */ //month is 0-11 in ruby, 1-12 in System.DateTime
                time.Day << 5 | /*  5 */
                time.Hour;           /*  5 */

            DateTime usecTime = new DateTime(time.Ticks);
            TimeSpan ts = usecTime.Subtract(new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second));
            int usec = (int)ts.Ticks / 10;

            s = time.Minute << 26 | /*  6 */
                time.Second << 20 | /*  6 */
                usec;               /* 20 */

            char[] buf = new char[8];

            for (int i = 0; i < 4; i++)
            {
                buf[i] = (char)(p & 0xff);
                p = p >> 8;
            }
            for (int i = 4; i < 8; i++)
            {
                buf[i] = (char)(s & 0xff);
                s = s >> 8;
            }

            return new String(new System.String(buf));
        }

        internal static Time time_mload(Frame caller, Time time, String str) // author: cjs/war, status: done
        {
            int p, s;
            int sec, usec;
            string buf;
            int i;
            System.DateTime tm;

            time_modify(caller, time);
            buf = str.value;
            if (str.value.Length != 8)
            {
                throw new TypeError("marshaled time format differ").raise(caller);
            }

            p = s = 0;
            for (i = 0; i < 4; i++)
            {
                p |= buf[i] << (8 * i);
            }
            for (i = 4; i < 8; i++)
            {
                s |= buf[i] << (8 * (i - 4));
            }

            if ((p & (1 << 31)) == 0)
            {
                sec = p;
                usec = s;
                tm = new DateTime(sec * 100000000L + usec * 100L);
            }
            else
            {
                p &= ~(1 << 31);

                tm = new DateTime(
                    ((p >> 14) & 0xffff) + 1900, //year
                    ((p >> 10) & 0xf) + 1,    //month //month is 0-11 in ruby, 1-12 in System.DateTime
                    (p >> 5) & 0x1f,    //day
                    p & 0x1f,           //hour
                    (s >> 26) & 0x3f,   //min
                    (s >> 20) & 0x3f);   //sec
                tm = tm.AddTicks((s & 0xfffff) * 10);//usec - DateTime constructor only supports up to msec
            }

            time.value = tm.ToLocalTime();

            return time;
        }

        internal static int obj2long(object obj, Frame caller)
        {
            if (obj is String)
            {
                obj = String.rb_str_to_inum(obj, caller, 10, false);
            }

            return (int)obj;
        }

        internal static Time time_utc_or_local(Array rest, bool utc_p, Frame caller)
        {
            return time_arg(rest, utc_p, caller);
        }

        struct StructTM
        {
            internal int tm_year;   /* Year less 1900 */
            internal int tm_mon;    /* month (0 - 11 : 0 = January) */
            internal int tm_mday;   /* day of month (1 - 31) */
            internal int tm_hour;   /* hour (0 - 23) */
            internal int tm_min;    /* minutes (0 - 59) */
            internal int tm_sec;    /* seconds (0 - 59) */
            internal int usec;      //not a member of the time.h struct tm.
            internal int tm_isdst;  /* daylight saving time enabled/disabled */
        }

        internal static object time_add(Time tobj, object offset, int sign, Frame caller)
        {
            double v = Numeric.rb_num2dbl(offset, caller);
            double f, d;
            uint sec_off;  //    unsigned_time_t sec_off;
            long usec_off, sec, usec; //time_t
            object result;

            if (v < 0)
            {
                v = -v;
                sign = -sign;
            }
            d = Marshal.modf(v, out f);
            sec_off = (uint)f;
            if (f != (double)sec_off)
            {
                throw new RangeError(string.Format(CultureInfo.InvariantCulture, "time {0} {1} out of Time range", sign < 0 ? "-" : "+", v)).raise(caller);
            }
            usec_off = (long)(d * 1e6);

            Time.GetParts(tobj, out sec, out usec);
            if (sign < 0)
            {
                sec -= sec_off;
                usec -= usec_off;
                //wartag: C Ruby uses overflows here to test if the subtraction will fail
                //unfortunately it tests for overflow on a signed 32-bit integer so it won't fail
                //if time is subtracted less than -2^31 before epoch. Rather it will go on to throw
                //a different type of error later on from a different location.
                if (sec < -2147483648)
                    throw new RangeError(string.Format(CultureInfo.InvariantCulture, "time - {0} out of Time range", v)).raise(caller);
            }
            else {
                //long tt = new Time(Time.EndTime)._tv_sec();
                sec += sec_off;
                usec += usec_off;
                //wartag: simple rangeerror once we get over (2^31-1)
                if (sec > 2147483647)
                    throw new RangeError(string.Format(CultureInfo.InvariantCulture, "time - {0} out of Time range", v)).raise(caller);
            }
            //Time.CheckAddition(tobj.value, Time.GetTicks(sec, usec), caller);
            Time.time_overflow_p(ref sec, ref usec, caller);
            result = new Time(new System.DateTime(Time.GetEpoch(tobj).Ticks + Time.GetTicks(sec, usec), tobj.value.Kind));
            return result;
        }

        internal static long NDIV(long x, long y){            
            return (-(-((x)+1)/(y))-1);
        }        

        internal static long NMOD(long x, long y){
            return ((y)-(-((x)+1)%(y))-1);
        }

        internal static void time_overflow_p(ref long secp, ref long usecp, Frame caller)
        {

            long tmp, sec = secp, usec = usecp;
            
            if (usec >= 1000000) { /* usec positive overflow */
                tmp = sec + usec / 1000000;
                usec %= 1000000;
                if (sec > 0 && tmp < 0) {
                    throw new RangeError("out of Time range").raise(caller);
                }
                sec = tmp;
            }
            if (usec < 0) {        /* usec negative overflow */
                tmp = sec + NDIV(usec,1000000); /* negative div */
                usec = NMOD(usec,1000000);      /* negative mod */
                if (sec < 0 && tmp > 0) {
                    throw new RangeError("out of Time range").raise(caller);
                }
                sec = tmp;
            }

            if (sec < 0 || (sec == 0 && usec < 0))
            {
                throw new ArgumentError("time must be positive").raise(caller);
            }
            secp = sec;
            usecp = usec;
        }

        internal static Time time_arg(Array argv, bool utc, Frame caller/*, out long secs, out long usec*/)
        {

            StructTM tm = new StructTM();
            System.DateTime dt;
            object[] v = new object[8];
            int year;

            if (argv.Count == 10)
            {
                v[0] = argv[5];
                v[1] = argv[4];
                v[2] = argv[3];
                v[3] = argv[2];
                v[4] = argv[1];
                v[5] = argv[0];
                v[6] = null;
                tm.tm_isdst = Marshal.RTEST(argv[8]) ? 1 : 0;
            }
            else
            {
                int numArgs = Class.rb_scan_args(caller, argv, 1, 7, false);
                v[0] = argv[0];
                for (int i = 1; i < numArgs; i++)
                {
                    v[i] = argv[i];
                }
                //        /* v[6] may be usec or zone (parsedate) */
                //        /* v[7] is wday (parsedate; ignored) */               
                tm.tm_isdst = -1;
            }


            year = obj2long(v[0], caller);

            if (0 <= year && year < 39)
            {
                year += 2000;
                Errors.rb_warning("2 digits year is used");
            }
            else if (69 <= year && year < 139)
            {
                year += 1900;
                Errors.rb_warning("2 or 3 digits year is used");
            }

            tm.tm_year = year;
            if (v[1] == null)
            {
                tm.tm_mon = 0;
            }
            else
            {
                String s = String.rb_check_string_type(v[1], caller);
                if (s != null)
                {
                    tm.tm_mon = -1;
                    for (int i = 0; i < 12; i++)
                    {
                        if (s.value.Length == 3 && string.Compare(s.value, Time.months[i], StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            tm.tm_mon = i;
                            break;
                        }
                    }
                    if (tm.tm_mon == -1)
                    {
                        char c = s.value[0];

                        if ('0' <= c && c <= '9')
                        {
                            tm.tm_mon = obj2long(s, caller) - 1;
                        }
                    }
                }
                else
                {
                    tm.tm_mon = obj2long(v[1], caller) - 1;
                }
            }

            if (v[2] == null)
            {
                tm.tm_mday = 1;
            }
            else
            {
                tm.tm_mday = obj2long(v[2], caller);
            }
            tm.tm_hour = (v[3] == null) ? 0 : obj2long(v[3], caller);
            tm.tm_min = (v[4] == null) ? 0 : obj2long(v[4], caller);
            tm.tm_sec = (v[5] == null) ? 0 : obj2long(v[5], caller);
            if (!(v[6] == null))
            {
                if (argv.Count == 8)
                {
                    /* v[6] is timezone, but ignored */
                }
                else if (argv.Count == 7)
                {
                    tm.usec = obj2long(v[6], caller);
                }
            }
            /* value validation */
            if (tm.tm_mon < 0 || tm.tm_mon > 11
                || tm.tm_mday < 1 || tm.tm_mday > 31
                || tm.tm_hour < 0 || tm.tm_hour > 23
                || tm.tm_min < 0 || tm.tm_min > 59
                || tm.tm_sec < 0 || tm.tm_sec > 60)
                throw new ArgumentError("argument out of range").raise(caller);

            if (utc)
            {
                dt = Time.CheckRange(tm.tm_year, tm.tm_mon + 1, tm.tm_mday, tm.tm_hour, tm.tm_min, tm.tm_sec, tm.usec, DateTimeKind.Utc, caller);
            }
            else
            {
                dt = Time.CheckRange(tm.tm_year, tm.tm_mon + 1, tm.tm_mday, tm.tm_hour, tm.tm_min, tm.tm_sec, tm.usec, DateTimeKind.Local, caller);
            }

            return new Time(dt);
        }


        internal static System.DateTime CheckRange(int year, int mon, int mday, int hour, int min, int sec, int usec, System.DateTimeKind kind, Frame caller)
        {
            System.DateTime dt;

            try
            {
                dt = new System.DateTime(year, mon, mday, hour, min, sec, usec, kind);

                if (dt.ToUniversalTime() > Time.EndTime || dt.ToUniversalTime() < Time.Epoch)
                {
                    throw new ArgumentError("time out of range").raise(caller);
                }
            }
            catch
            {
                throw new ArgumentError("time out of range").raise(caller);
            }

            return dt;
        }

        internal static void CheckAddition(System.DateTime date1, long ticks, Frame caller)
        {
            try
            {
                if (date1.ToUniversalTime().AddTicks(ticks) > Time.EndTime)
                {
                    throw new ArgumentError("time out of range").raise(caller);
                }
            }
            catch
            {
                throw new ArgumentError("time out of range").raise(caller);
            }
        }

        internal static void CheckSubtraction(System.DateTime date1, long ticks, Frame caller)
        {
            try
            {
                if (date1.ToUniversalTime().AddTicks(-ticks) < Time.Epoch)
                {
                    throw new ArgumentError("time out of range").raise(caller);
                }
            }
            catch
            {
                throw new ArgumentError("time out of range").raise(caller);
            }
        }
    }
}
