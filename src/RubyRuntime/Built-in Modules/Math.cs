/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


namespace Ruby
{

    public partial class Math
    {
        private const double M_PI = System.Math.PI;
        private const double M_E = System.Math.E;


        internal static double erf(double z)
        {

            double t = 1.0 / (1.0 + 0.5 * System.Math.Abs(z));
            double ans = 1 - t * System.Math.Exp(-z * z - 1.26551223 +
                                                 t * (1.00002368 +
                                                 t * (0.37409196 +
                                                 t * (0.09678418 +
                                                 t * (-0.18628806 +
                                                 t * (0.27886807 +
                                                 t * (-1.13520398 +
                                                 t * (1.48851587 +
                                                 t * (-0.82215223 +
                                                 t * (0.17087277))))))))));
            if (z >= 0) return ans;
            else return -ans;
        }
    }
}
