using Ruby.Runtime;

namespace Ruby
{
    public class Env
    {
        internal const string PATH_ENV = "PATH";

        internal static int path_tainted = -1;

        internal static String env_str_new(Frame caller, string str)
        {
            String s = new String(str);
            s.Tainted = true;
            Object.rb_obj_freeze(caller, s);

            return s;
        }

        internal static String env_str_new2(Frame caller, string str)
        {
            if (str == null)
                return null;
            else
                return env_str_new(caller, str);
        }

        internal static bool rb_env_path_tainted()
        {
            return path_tainted < 0;
        }

        internal static void env_each_i(Frame caller, Proc block, bool values)
        {
            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                String key = Env.env_str_new(caller, (string)pair.Key);
                String value = Env.env_str_new2(caller, (string)pair.Value);

                if (values)
                    Proc.rb_yield(block, caller, key, value);
                else
                    Proc.rb_yield(block, caller, new Array(key, value));
            }
        }

        internal static String env_delete(Frame caller, object name)
        {
            Eval.rb_secure(4, caller);

            string nam = String.StringValue(name, caller);
            Eval.rb_check_safe_obj(caller, name);

            string val = System.Environment.GetEnvironmentVariable(nam);
            if (val != null)
            {
                String value = env_str_new2(caller, val);
                System.Environment.SetEnvironmentVariable(nam, null);

                if (PATH_ENV.Equals(nam))
                    path_tainted = 0;

                return value;
            }

            return null;
        }
    }
}
