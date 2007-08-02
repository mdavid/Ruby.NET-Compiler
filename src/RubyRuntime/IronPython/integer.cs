/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace IronMath {
    /// <summary>
    /// arbitrary precision integers
    /// </summary>
    
    public struct integer : IComparable {
        private const int BitsPerDigit = 32;
        private const ulong Base = 0x100000000;

        private readonly short sign;
        private readonly uint[] data;

        internal short Sign
        {
            get
            {
                return sign;
            }
        }

        internal uint[] Data
        {
            get
            {
                return data;
            }
        }

        public static readonly integer ZERO = new integer(0, new uint[0]);
        public static readonly integer ONE = new integer(+1, new uint[] { 1 });


        public static integer make(ulong v) {
            return new integer(+1, (uint)v, (uint)(v >> BitsPerDigit));
        }

        public static integer make(uint v) {
            if (v == 0) return ZERO;
            else if (v == 1) return ONE;
            else return new integer(+1, v);
        }

        public static integer make(long v) {
            ulong x;
            int s = +1;
            if (v < 0) {
                x = (ulong)-v; s = -1;
            } else {
                x = (ulong)v;
            }

            return new integer(s, (uint)x, (uint)(x >> BitsPerDigit));
        }

        public static integer make(int v) {
            if (v == 0) return ZERO;
            else if (v == 1) return ONE;
            else if (v < 0) return new integer(-1, (uint)-v);
            else return new integer(+1, (uint)v);
        }

        public static integer make(double v) {
            // !!! very simple for now
            return integer.make((long)v);
        }

        public static implicit operator integer(uint i) {
            return make(i);
        }
        public static implicit operator integer(int i)
        {
            return make(i);
        }
        public static implicit operator integer(ulong i)
        {
            return make(i);
        }
        public static implicit operator integer(long i)
        {
            return make(i);
        }

        public static implicit operator double(integer i)
        {
            return i.ToFloat64();
        }

        internal integer(int sign, params uint[] data) {
            this.sign = (short)sign;
            this.data = data;
            //this.data = resize(data, GetLength(data));

            //!!! consider caching length here
            if (length == 0) this.sign = 0;
        }

        internal bool AsInt64(out long ret) {
            ret = 0;
            if (sign == 0) return true;
            if (length > 2) return false;
            if (data.Length == 1) {
                ret = sign * (long)data[0];
                return true;
            }
            ret = (long)(((ulong)data[1]) << 32 | (ulong)data[0]);
            if (ret < 0) return false;
            ret *= sign;
            return true;
        }

        internal bool AsUInt32(out uint ret) {
            ret = 0;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (length > 1) return false;
            ret = data[0];
            return true;
        }

        internal bool AsUInt64(out ulong ret) {
            ret = 0;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (length > 2) return false;
            ret = (ulong)data[0];
            if (data.Length > 1) {
                ret |= ((ulong)data[1]) << 32;
            }
            return true;
        }

        internal bool AsInt32(out int ret) {
            ret = 0;
            if (sign == 0) return true;
            if (length > 1) return false;
            long result = data[0] * sign;
            ret = (int)result;
            return ret == result;
        }


        internal uint ToUInt32() {
            uint ret;
            if (AsUInt32(out ret)) return ret;
            throw new OverflowException("integer won't fit in uint");
        }

        internal int ToInt32() {
            int ret;
            if (AsInt32(out ret)) return ret;
            throw new OverflowException("integer won't fit in int");
        }

        internal long ToInt64() {
            long ret;
            if (AsInt64(out ret)) return ret;
            throw new OverflowException("integer won't fit in long");
        }

        internal ulong ToUInt64()
        {
            ulong ret;
            if (AsUInt64(out ret)) return ret;
            throw new OverflowException("integer won't fit in ulong");
        }

        internal double ToFloat64() {
            return double.Parse(ToString(10));

            //!!! code above is horribly inefficient; however, code below is inaccurate which is clearly worse
            //            double ret = (double)sign;
            //            double place = 1.0;
            //            double radix = (double)uint.MaxValue;
            //            for (int i=0, len=length; i<len; i++) {
            //                ret += place*(double)data[i];
            //                place *= radix;
            //            }
            //            return ret;
        }

        internal int length {
            get {
                //return data.Length; 
                return GetLength(data);
            }
        }

        private static int GetLength(uint[] data) {
            int ret = data.Length - 1;
            while (ret >= 0 && data[ret] == 0) ret--;
            return ret + 1;
        }


        private static uint[] copy(uint[] v) {
            uint[] ret = new uint[v.Length];
            for (int i = 0; i < v.Length; i++) {
                ret[i] = v[i];
            }
            return ret;
        }

        private static uint[] resize(uint[] v, int len) {
            if (v.Length == len) return v;
            uint[] ret = new uint[len];
            int n = Math.Min(v.Length, len);
            for (int i = 0; i < n; i++) {
                ret[i] = v[i];
            }
            return ret;
        }

        private static uint[] add(uint[] x, int xl, uint[] y, int yl) {
            Debug.Assert(xl >= yl);
            uint[] z = new uint[xl];

            int i;
            ulong sum = 0;
            for (i = 0; i < yl; i++) {
                sum = sum + x[i] + y[i];
                z[i] = (uint)sum;
                sum >>= BitsPerDigit;
            }

            for (; i < xl && sum != 0; i++) {
                sum = sum + x[i];
                z[i] = (uint)sum;
                sum >>= BitsPerDigit;
            }
            if (sum != 0) {
                z = resize(z, xl + 1);
                z[i] = (uint)sum;
            } else {
                for (; i < xl; i++) {
                    z[i] = x[i];
                }
            }
            return z;
        }

        private static uint[] sub(uint[] x, int xl, uint[] y, int yl) {
            Debug.Assert(xl >= yl);
            uint[] z = new uint[xl];

            int i;
            bool borrow = false;
            for (i = 0; i < yl; i++) {
                uint xi = x[i];
                uint yi = y[i];
                if (borrow) {
                    if (xi == 0) {
                        xi = 0xffffffff;
                        borrow = true;
                    } else {
                        xi -= 1;
                        borrow = false;
                    }
                }
                if (yi > xi) borrow = true;
                z[i] = xi - yi;
            }

            if (borrow) {
                for (; i < xl; i++) {
                    uint xi = x[i];
                    z[i] = xi - 1;
                    if (xi != 0) { i++; break; }
                }
            }
            for (; i < xl; i++) {
                z[i] = x[i];
            }
            return z;  // may have leading zeros
        }

        private static uint[] add0(uint[] x, int xl, uint[] y, int yl) {
            if (xl >= yl) return add(x, xl, y, yl);
            else return add(y, yl, x, xl);
        }

        public static int Compare(integer x, integer y) {
            if (x.sign == y.sign) {
                int xl = x.length;
                int yl = y.length;
                if (xl == yl) {
                    for (int i = xl - 1; i >= 0; i--) {
                        if (x.data[i] == y.data[i]) continue;
                        return x.data[i] > y.data[i] ? x.sign : -x.sign;
                    }
                    return 0;
                } else {
                    return xl > yl ? +x.sign : -x.sign;
                }
            } else {
                return x.sign > y.sign ? +1 : -1;
            }
        }

        public static bool operator ==(integer x, int y) {
            return x == (integer)y;
        }

        public static bool operator !=(integer x, int y)
        {
            return !(x == y);
        }

        public static bool operator ==(integer x, double y)
        {
            double d = x.ToFloat64();
            return d == y; ;
        }

        public static bool operator ==(double x, integer y)
        {
            return y.ToFloat64() == x;
        }

        public static bool operator !=(integer x, double y)
        {
            return !(x == y);
        }

        public static bool operator !=(double x, integer y)
        {
            return !(x == y);
        }


        public static bool operator ==(integer x, integer y)
        {
            return Compare(x, y) == 0;
        }

        public static bool operator !=(integer x, integer y)
        {
            return Compare(x, y) != 0;
        }
        public static bool operator <(integer x, integer y)
        {
            return Compare(x, y) < 0;
        }
        public static bool operator <=(integer x, integer y)
        {
            return Compare(x, y) <= 0;
        }
        public static bool operator >(integer x, integer y)
        {
            return Compare(x, y) > 0;
        }
        public static bool operator >=(integer x, integer y)
        {
            return Compare(x, y) >= 0;
        }

        public static integer add(integer x, integer y) { return x + y; }

        public static integer operator +(integer x, integer y)
        {
            if (x.sign == y.sign) {
                return new integer(x.sign, add0(x.data, x.length, y.data, y.length));
            } else {
                return x - new integer(-y.sign, y.data);  //??? performance issue
            }
        }

        public static integer subtract(integer x, integer y) { return x - y; }

        public static integer operator -(integer x, integer y)
        {
            int c = Compare(x, y);
            if (c == 0) return ZERO;

            if (x.sign == y.sign) {
                uint[] z;
                switch (c * x.sign) {
                    case +1:
                        z = sub(x.data, x.length, y.data, y.length);
                        break;
                    case -1:
                        z = sub(y.data, y.length, x.data, x.length);
                        break;
                    default:
                        return ZERO;
                }
                return new integer(c, z);
            } else {
                uint[] z = add0(x.data, x.length, y.data, y.length);
                return new integer(c, z);
            }
        }

        public static integer multiply(integer x, integer y) { return x * y; }

        public static integer operator *(integer x, integer y)
        {
            int xl = x.length;
            int yl = y.length;
            int zl = xl + yl;
            uint[] xd = x.data, yd = y.data, zd = new uint[zl];

            for (int xi = 0; xi < xl; xi++) {
                uint xv = xd[xi];
                int zi = xi;
                ulong carry = 0;
                for (int yi = 0; yi < yl; yi++) {
                    carry = carry + ((ulong)xv) * yd[yi] + zd[zi];
                    zd[zi++] = (uint)carry;
                    carry >>= BitsPerDigit;
                }
                while (carry != 0) {
                    carry += zd[zi];
                    zd[zi++] = (uint)carry;
                    carry >>= BitsPerDigit;
                }
            }

            return new integer(x.sign * y.sign, zd);
        }


        public static integer divide(integer x, integer y) { return x / y; }

        public static integer operator /(integer x, integer y)
        {
            integer dummy;
            return divmod(x, y, out dummy);
        }

        public static integer modulo(integer x, integer y) { return x % y; }

        public static integer operator %(integer x, integer y)
        {
            integer ret;
            divmod(x, y, out ret);
            return ret;
        }

        public static int GetNormalizeShift(uint value) {
            int shift = 0;

            if ((value & 0xFFFF0000) == 0) { value <<= 16; shift += 16; }
            if ((value & 0xFF000000) == 0) { value <<= 8; shift += 8; }
            if ((value & 0xF0000000) == 0) { value <<= 4; shift += 4; }
            if ((value & 0xC0000000) == 0) { value <<= 2; shift += 2; }
            if ((value & 0x80000000) == 0) { value <<= 1; shift += 1; }

            return shift;
        }


        private static void Normalize(uint[] u, int l, uint[] un, int shift) {
            Debug.Assert(un.Length == l || un.Length == l + 1);
            Debug.Assert(un.Length == l + 1 || ((u[l-1] << shift) >> shift) == u[l-1]);
            Debug.Assert(0 <= shift && shift < 32);

            uint carry = 0;
            int i;
            if (shift > 0) {
                int rshift = BitsPerDigit - shift;
                for (i = 0; i < l; i++) {
                    uint ui = u[i];
                    un[i] = (ui << shift) | carry;
                    carry = ui >> rshift;
                }
            } else {
                for (i = 0; i < l; i++) {
                    un[i] = u[i];
                }
            }

            while (i < un.Length) {
                un[i++] = 0;
            }

            if (carry != 0) {
                Debug.Assert(l == un.Length - 1);
                un[l] = carry;
            }
        }

        private static void Unnormalize(uint[] un, out uint[] r, int shift) {
            Debug.Assert(0 <= shift && shift < 32);

            int length = GetLength(un);
            r = new uint[length];

            if (shift > 0) {
                int lshift = 32 - shift;
                uint carry = 0;
                for (int i = length - 1; i >= 0; i--) {
                    uint uni = un[i];
                    r[i] = (uni >> shift) | carry;
                    carry = (uni << lshift);
                }
            } else {
                for (int i = 0; i < length; i++) {
                    r[i] = un[i];
                }
            }
        }

        private static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r) {
            int m = GetLength(u);
            int n = GetLength(v);

            if (n <= 1) {
                if (n == 0) {
                    throw new DivideByZeroException();
                }

                //  Divide by single digit
                //
                ulong rem = 0;
                uint v0 = v[0];
                q = new uint[m];
                r = new uint[1];

                for (int j = m - 1; j >= 0; j--) {
                    rem *= Base;
                    rem += u[j];

                    ulong div = rem / v0;
                    rem -= div * v0;
                    q[j] = (uint)div;
                }
                r[0] = (uint)rem;
            } else if (m >= n) {
                int shift = GetNormalizeShift(v[n - 1]);

                uint[] un = new uint[m + 1];
                uint[] vn = new uint[n];

                Normalize(u, m, un, shift);
                Normalize(v, n, vn, shift);

                q = new uint[m - n + 1];
                r = null;

                //  Main division loop
                //
                for (int j = m - n; j >= 0; j--) {
                    ulong rr, qq;
                    int i;

                    rr = Base * un[j + n] + un[j + n - 1];
                    qq = rr / vn[n - 1];
                    rr -= qq * vn[n - 1];

                    Debug.Assert((Base * un[j + n] + un[j + n - 1]) == qq * vn[n - 1] + rr);

                    for (; ; ) {
                        // Estimate too big ?
                        //
                        if ((qq >= Base) || (qq * vn[n - 2] > (rr * Base + un[j + n - 2]))) {
                            qq--;
                            rr += (ulong)vn[n - 1];
                            if (rr < Base) continue;
                        }
                        break;
                    }

                    Debug.Assert((Base * un[j + n] + un[j + n - 1]) == qq * vn[n - 1] + rr);

                    //  Multiply and subtract
                    //
                    long b = 0;
                    long t = 0;
                    for (i = 0; i < n; i++) {
                        ulong p = vn[i] * qq;
                        t = (long)un[i + j] - (long)(uint)p - b;
                        un[i + j] = (uint)t;
                        p >>= 32;
                        t >>= 32;
                        Debug.Assert(t == 0 || t == -1 || t == -2);
                        b = (long)p - t;
                    }
                    t = (long)un[j + n] - b;
                    un[j + n] = (uint)t;

                    //  Store the calculated value
                    //
                    q[j] = (uint)qq;

                    //  Add back vn[0..n] to un[j..j+n]
                    //
                    if (t < 0) {
                        q[j]--;
                        ulong c = 0;
                        for (i = 0; i < n; i++) {
                            c = (ulong)vn[i] + un[j + i] + c;
                            un[j + i] = (uint)c;
                            c >>= 32;
                        }
                        c += (ulong)un[j + n];
                        un[j + n] = (uint)c;
                    }
                }

                Unnormalize(un, out r, shift);
            } else {
                q = new uint[] { 0 };
                r = u;
            }
        }

        public static integer divmod(integer x, integer y, out integer mod)
        {
            integer div = divrem(x, y, out mod);

            if (x.sign != y.sign && mod != ZERO)
            {
                div = div - 1;
                mod = mod + y;
            }

            return div;
        }

        public static integer divrem(integer x, integer y, out integer mod)
        {
            integer div;
            uint[] q;
            uint[] r;

            DivModUnsigned(x.data, y.data, out q, out r);

            div = new integer(x.sign * y.sign, q);
            mod = new integer(x.sign, r);

            return div;

        }


        private static uint div(uint[] n, ref int nl, uint d) {
            ulong rem = 0;
            int i = nl;
            bool seenNonZero = false;
            while (--i >= 0) {
                rem <<= BitsPerDigit;
                rem |= n[i];
                uint v = (uint)(rem / d);
                n[i] = v;
                if (v == 0) {
                    if (!seenNonZero) nl--;
                } else {
                    seenNonZero = true;
                }
                rem %= d;
            }
            return (uint)rem;
        }

        //        private uint getTwosComplement(int index) {
        //            if (index >= data.Length) {
        //                return getSignExtension();
        //            } else {
        //                uint ret = data[index];
        //                if (sign == -1) {
        //                    -ret vs. ~ret
        //                } else {
        //                    return ret;
        //                }
        //            }
        //        }

        private static uint extend(uint v, ref bool seenNonZero) {
            if (seenNonZero) {
                return ~v;
            } else {
                if (v == 0) {
                    return 0;
                } else {
                    seenNonZero = true;
                    return ~v + 1;
                }
            }
        }

        private static uint getOne(bool isNeg, uint[] data, int i, ref bool seenNonZero) {
            if (i < data.Length) {
                uint ret = data[i];
                return isNeg ? extend(ret, ref seenNonZero) : ret;
            } else {
                return isNeg ? uint.MaxValue : 0;
            }
        }

        private static uint[] makeTwosComplement(uint[] d) {
            // first do complement and +1 as long as carry is needed
            int i = 0;
            uint v = 0;
            for (; i < d.Length; i++) {
                v = ~d[i] + 1;
                d[i] = v;
                if (v != 0) { i++; break; }
            }

            if (v != 0) {
                // now ones complement is sufficient
                for (; i < d.Length; i++) {
                    d[i] = ~d[i];
                }
            } else {
                //??? this is weird
                d = resize(d, d.Length + 1);
                d[d.Length - 1] = 1;
            }
            return d;
        }

        public static integer and(integer x, integer y) { return x & y; }

        public static integer operator &(integer x, integer y)
        {
            int xl = x.length, yl = y.length;
            uint[] xd = x.data, yd = y.data;

            int zl = Math.Max(xl, yl);  //!!! can optimize for some &/| cases
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu & yu;
            }

            if (negx && negy) {

                return new integer(-1, makeTwosComplement(zd));
            } else if (negx || negy) {
                return new integer(+1, zd);
            } else {
                return new integer(+1, zd);
            }
        }


        public static integer or(integer x, integer y) { return x | y; }

        public static integer operator |(integer x, integer y)
        {
            int xl = x.length, yl = y.length;
            uint[] xd = x.data, yd = y.data;

            int zl = Math.Max(xl, yl);  //!!! can optimize for some &/| cases
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu | yu;
            }

            if (negx && negy) {
                return new integer(-1, makeTwosComplement(zd));
            } else if (negx || negy) {
                return new integer(-1, makeTwosComplement(zd));
            } else {
                return new integer(+1, zd);
            }
        }

        public static integer xor(integer x, integer y) { return x ^ y; }

        public static integer operator ^(integer x, integer y)
        {
            int xl = x.length, yl = y.length;
            uint[] xd = x.data, yd = y.data;

            int zl = Math.Max(xl, yl);  //!!! can optimize for some &/| cases
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu ^ yu;
            }

            if (negx && negy) {
                return new integer(+1, zd);
            } else if (negx || negy) {
                return new integer(-1, makeTwosComplement(zd));
            } else {
                return new integer(+1, zd);
            }
        }

        public static integer lshift(integer x, int shift) {
            return x << shift;
        }

        public static integer operator <<(integer x, int shift) {
            if (shift == 0) return x;
            else if (shift < 0) return x >> -shift;

            int digitShift = shift / BitsPerDigit;
            int smallShift = shift - (digitShift * BitsPerDigit);

            int xl = x.length;
            uint[] xd = x.data;
            int zl = xl + digitShift + 1;
            uint[] zd = new uint[zl];

            if (smallShift == 0) {
                for (int i = 0; i < xl; i++) {
                    zd[i + digitShift] = xd[i];
                }
            } else {
                int carryShift = BitsPerDigit - smallShift;
                uint carry = 0;
                int i;
                for (i = 0; i < xl; i++) {
                    uint rot = xd[i];
                    zd[i + digitShift] = rot << smallShift | carry;
                    carry = rot >> carryShift;
                }
                zd[i + digitShift] = carry;
            }
            return new integer(x.sign, zd);
        }

        public static integer rshift(integer x, int shift) {
            return x >> shift;
        }

        public static integer operator >>(integer x, int shift)
        {
            if (shift == 0) return x;
            else if (shift < 0) return x << -shift;

            int digitShift = shift / BitsPerDigit;
            int smallShift = shift - (digitShift * BitsPerDigit);

            int xl = x.length;
            uint[] xd = x.data;
            int zl = xl - digitShift;
            uint[] zd = new uint[zl];

            if (smallShift == 0) {
                for (int i = xl - 1; i >= digitShift; i--) {
                    zd[i - digitShift] = xd[i];
                }
            } else {
                int carryShift = BitsPerDigit - smallShift;
                uint carry = 0;
                for (int i = xl - 1; i >= digitShift; i--) {
                    uint rot = xd[i];
                    zd[i - digitShift] = rot >> smallShift | carry;
                    carry = rot << carryShift;
                }
            }

           integer result = new integer(x.sign, zd);

           if (x.sign < 0 && (x.data[0] & 1) != 0)
               result = result - 1;

           return result;
        }

        public static integer operator -(integer x) {
            return new integer(-x.sign, x.data);
        }

        public static integer operator ~(integer x) {
            return -(x + ONE);
        }

        internal integer abs() {
            if (this.sign == -1) return -this;
            else return this;
        }

        internal integer pow(int power) {
            if (power == 0) return ONE;
            if (power < 0) throw new ArgumentOutOfRangeException("power", power, "power must be >= 0");
            integer factor = this;
            integer result = ONE; //!!! want a mutable here for efficiency
            while (power != 0) {
                if ((power & 1) != 0) result = result * factor;
                factor = factor.square();
                power >>= 1;
            }
            return result;
        }

        internal integer modpow(int power, integer mod) {
            if (power == 0) return ONE;
            if (power < 0) throw new ArgumentOutOfRangeException("power", power, "power must be >= 0");
            integer factor = this;
            integer result = ONE; //!!! want a mutable here for efficiency
            while (power != 0) {
                if ((power & 1) != 0) {
                    result = result * factor;
                    result = result % mod; //!!! should do all in one step
                }
                factor = factor.square();
                power >>= 1;
            }
            return result;
        }

        internal integer square() {
            return this * this; //!!! can do much better than O(N**2)
        }

        public override string ToString()
        {
            return ToString(10);
        }

        // generated by scripts/radix_generator.py
        private static uint[] maxCharsPerDigit = { 0, 0, 31, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
        private static uint[] groupRadixValues = { 0, 0, 2147483648, 3486784401, 1073741824, 1220703125, 2176782336, 1977326743, 1073741824, 3486784401, 1000000000, 2357947691, 429981696, 815730721, 1475789056, 2562890625, 268435456, 410338673, 612220032, 893871739, 1280000000, 1801088541, 2494357888, 3404825447, 191102976, 244140625, 308915776, 387420489, 481890304, 594823321, 729000000, 887503681, 1073741824, 1291467969, 1544804416, 1838265625, 2176782336 };

        internal string ToString(uint radix) {
            if (radix < 2) throw new ArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            if (radix > 36) throw new ArgumentOutOfRangeException("radix", radix, "radix must be <= 36");

            int len = length;
            if (len == 0) return "0";

            ArrayList digitGroups = new ArrayList();

            uint[] d = copy(data);
            int dl = length;

            uint groupRadix = groupRadixValues[radix];
            while (dl > 0) {
                uint rem = div(d, ref dl, groupRadix);
                digitGroups.Add(rem);  //!!! generics will improve efficiency
            }

            StringBuilder ret = new StringBuilder();
            if (sign == -1) ret.Append("-");
            int digitIndex = digitGroups.Count - 1;

            char[] tmpDigits = new char[maxCharsPerDigit[radix]];

            appendRadix((uint)digitGroups[digitIndex--], radix, tmpDigits, ret, false);
            while (digitIndex >= 0) {
                appendRadix((uint)digitGroups[digitIndex--], radix, tmpDigits, ret, true);
            }
            return ret.ToString();
        }

        private static void appendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros) {
            const string symbols = "0123456789abcdefghijklmnopqrstuvwxyz";

            int digits = tmp.Length;
            int i = digits;
            while (i > 0 && (leadingZeros || rem != 0)) {
                uint digit = rem % radix;
                rem /= radix;
                tmp[--i] = symbols[(int)digit];
            }
            if (leadingZeros) buf.Append(tmp);
            else buf.Append(tmp, i, digits - i);
        }

        public override int GetHashCode()
        {
            if (data.Length == 0) return 0;
            //!!! weak (must be same as int for values in the range of a single int)
            return (int)data[0];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is integer)) return false;
            integer o = (integer)obj;
            return this == o;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (!(obj is integer)) throw new ArgumentException("expected integer");
            integer o = (integer)obj;
            return Compare(this, o);
        }

        #endregion
    }
}