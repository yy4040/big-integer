using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Norify
{
    [Serializable]
    public struct NRNumber : IFormattable, IComparable, IEquatable<NRNumber>, IComparable<NRNumber>
    {
        public static NRNumber Zero = new NRNumber() { _mantissa = 0, _exponent = _normalizedExponentBase };
        public static NRNumber One = new NRNumber(1, 0);
        public static NRNumber PositiveInfinity = new NRNumber(1, 0x7F800000);
        public static NRNumber NegativeInfinity = new NRNumber(1, 0x7F800000);
        
        public static Func<NRNumber, string, IFormatProvider?, string?>? CustomToString;
        
        private int _mantissa;
        private int _exponent;

        public int Mantissa => _mantissa;
        public int Exponent => _exponent;

        public NRNumber(int mantissa, int exponent)
        {
            this = Normalize(mantissa, exponent);
        }

        public NRNumber(NRNumber other)
        {
            _mantissa = other._mantissa;
            _exponent = other._exponent;
        }

        public bool IsZero => _mantissa == 0;

        public bool IsNotZero => _mantissa != 0;

        public int CompareTo(NRNumber other)
        {
            if (_mantissa == 0 || other._mantissa == 0 || _exponent == other._exponent)
                return _mantissa.CompareTo(other._mantissa);

            if (_mantissa > 0 && other._mantissa < 0)
                return 1;

            if (_mantissa < 0 && other._mantissa > 0)
                return -1;

            var expComp = _exponent.CompareTo(other._exponent);
            return _mantissa > 0 ? expComp : -expComp;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
                return 1;
            
            if (obj is int i)
                return CompareTo(FromInt(i));
            
            if (obj is float f)
                return CompareTo(FromFloat(f));
            
            if (obj is NRNumber nr)
                return CompareTo(nr);
            
            throw new NotSupportedException();
        }
        
        public bool Equals(NRNumber other)
            => _mantissa == other._mantissa && _exponent == other._exponent;

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is int i)
                return Equals(i);

            if (obj is float f)
                return Equals(f);

            if (obj is NRNumber nr)
                return Equals(nr);

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_mantissa.GetHashCode() * 397) ^ _exponent.GetHashCode();
            }
        }

        public string ToString(string format) => ToString(format, null);

        public string ToString(string format, IFormatProvider? formatProvider)
        {
            if (CustomToString != null)
            {
                var ret = CustomToString.Invoke(this, format, formatProvider);
                if (ret != null)
                    return ret;
            }
            return ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Normalize() => this = Normalize(_mantissa, _exponent);

        public override string ToString()
        {
            var builder = InternalStatic.Sb;
            
            builder.Clear();

            if (_exponent >= 0)
            {
                builder.Append(_mantissa);
                for (var i = 0; i < _exponent; ++i)
                    builder.Append('0');
                return builder.ToString();
            }
            else
            {
                var t = Math.Abs(_mantissa);
                var sign = t != _mantissa ? -1 : 1;
                var first = true;
                
                for (var i = _exponent; i < 0; ++i)
                {
                    var r = t % 10;
                    if (!first || r != 0)
                    {
                        first = false;
                        builder.Insert(0, r);
                    }
                    t /= 10;
                }
                if (!first)
                    builder.Insert(0, '.');

                if (t > 0)
                {
                    do
                    {
                        builder.Insert(0, t % 10);
                        t /= 10;
                    } while (t > 0);
                }
                else
                {
                    builder.Insert(0, '0');
                }

                if (sign < 0)
                    builder.Insert(0, '-');
                
                return builder.ToString();
            }
        }

        public string ToStringE()
        {
            var builder = InternalStatic.Sb;
            
            builder.Clear();

            var first = true;
            var t = Math.Abs(_mantissa);
            var sign = t != _mantissa ? -1 : 1;
            var additionalExp = 0;
            
            while (t >= 10)
            {
                var r = t % 10;
                if (!first || r != 0)
                {
                    first = false;
                    builder.Insert(0, r);
                }
                t /= 10;
                ++additionalExp;
            }
            
            if (!first)
                builder.Insert(0, '.');
            
            builder.Insert(0, t * sign);

            if (_exponent + additionalExp != 0)
                builder.Append('e').Append(_exponent + additionalExp);

            return builder.ToString();
        }

        /// <summary>
        /// Is is unsafe.
        /// </summary>
        /// <returns>
        /// Returns float.Max if this is greater than or equal to 1e38.
        /// Returns float.Min if this is less than or equal to -1e38.
        /// Others return float value
        /// </returns>
        public float ToFloat()
        {
            if (_mantissa == 0)
                return 0f;
            
            var additionalExp = (int)Math.Floor(Math.Log10(Math.Abs(_mantissa)));
            if (_exponent + additionalExp >= 38)
                return _mantissa > 0 ? float.MaxValue : float.MinValue;

            return (float)(_mantissa * PowersOf10.LookupD(_exponent));
        }

        public static NRNumber FromFloat(float value)
        {
            if (!float.IsFinite(value))
            {
                var bits = BitConverter.SingleToInt32Bits(value);
                return new NRNumber(bits > 0 ? 1 : -1, bits & 0x7FFFFFFF);
            }

            if (Math.Abs(value) < float.Epsilon)
            {
                return Zero;
            }

            var exponent = (int)Math.Floor(Math.Log10(Math.Abs(value))) - 6;
            var mantissa = (int)Math.Round(value * PowersOf10.LookupD(-exponent));
            
            return Normalize(mantissa, exponent);
        }
        
        public static NRNumber FromDouble(double value)
        {
            if (!double.IsFinite(value))
            {
                var bits = BitConverter.DoubleToInt64Bits(value);
                return new NRNumber(bits > 0 ? 1 : -1, (int)((bits >> 32) & 0x7FFFFFFF));
            }

            if (Math.Abs(value) < float.Epsilon)
            {
                return Zero;
            }

            var exponent = (int)Math.Floor(Math.Log10(Math.Abs(value))) - 8;
            var mantissa = (int)Math.Round(value * PowersOf10.LookupD(-exponent));
            
            return Normalize(mantissa, exponent);
        }

        public static NRNumber FromInt(int value)
            => Normalize(value, 0);
        
        private const string _notationPattern = @"(^[+\-]?(?:0|[1-9]\d*)(?:\.\d+)?)(?:[eE]([+\-]?\d+))?$";
        public static NRNumber FromString(string s)
        {
            var match = Regex.Match(s, _notationPattern);
            if (match.Success)
            {
                var captures = match.Captures;
                var mantissa = double.Parse(captures[0].Value);
                var exponent = captures.Count < 3 ? 0 : int.Parse(captures[1].Value);
                
                var temp = FromDouble(mantissa);
                return new NRNumber(temp._mantissa, temp._exponent + exponent);
            }

            if (double.TryParse(s, out var d))
                return FromDouble(d);
            
            return Zero;
        }

        private const int _normalizedMantissaMin = 100000000;
        private const int _normalizedMantissaMax = 1000000000;
        private const int _normalizedExponentBase = -8;
        public static NRNumber Normalize(long mantissa, int exponent)
        {
            if (mantissa == 0)
                return Zero;
            
            var newMantissa = Math.Abs(mantissa);
            var sign = newMantissa != mantissa ? -1 : 1;
            var additionalExp = 0;
            
            while (newMantissa < _normalizedMantissaMin)
            {
                newMantissa *= 10;
                ++additionalExp;
            }

            while (newMantissa >= _normalizedMantissaMax)
            {
                newMantissa /= 10;
                --additionalExp;
            }

            return new NRNumber()
            {
                _mantissa = (int)newMantissa * sign,
                _exponent = exponent - additionalExp 
            };
        }
        
        public static implicit operator NRNumber(float value) => FromFloat(value);
        
        public static implicit operator NRNumber(int value) => FromInt(value);

        public static NRNumber operator -(NRNumber value) => new NRNumber()
        {
            _mantissa = -value._mantissa,
            _exponent = value._exponent
        };

        public static NRNumber operator +(NRNumber a, NRNumber b)
        {
            if (a._exponent < b._exponent)
                (a, b) = (b, a);
            
            var diffExp = a._exponent - b._exponent;
            if (diffExp >= 9) return a;

            var la = a._mantissa * (long)PowersOf10.LookupD(diffExp);

            return Normalize(la + b._mantissa, b._exponent);
        }

        public static NRNumber operator -(NRNumber a, NRNumber b) => a + -b;

        public static NRNumber operator *(NRNumber a, NRNumber b)
            => Normalize((long)a._mantissa * b._mantissa, a._exponent + b._exponent);

        public static NRNumber operator /(NRNumber a, NRNumber b)
            => Normalize((long)a._mantissa * _normalizedMantissaMax / b._mantissa, a._exponent - 9 - b._exponent);

        public static NRNumber operator ++(NRNumber value)
            => value + 1;

        public static NRNumber operator --(NRNumber value)
            => value + -1;

        public static bool operator ==(NRNumber l, NRNumber r)
            => l.Equals(r);

        public static bool operator !=(NRNumber l, NRNumber r)
            => !l.Equals(r);

        public static bool operator <(NRNumber l, NRNumber r)
            => l.CompareTo(r) < 0;

        public static bool operator <=(NRNumber l, NRNumber r)
            => l.CompareTo(r) <= 0;

        public static bool operator >(NRNumber l, NRNumber r)
            => l.CompareTo(r) > 0;

        public static bool operator >=(NRNumber l, NRNumber r)
            => l.CompareTo(r) >= 0;

        public static NRNumber Max(NRNumber l, NRNumber r)
            => l.CompareTo(r) > 0 ? l : r;

        public static NRNumber Min(NRNumber l, NRNumber r)
            => l.CompareTo(r) < 0 ? l : r;
        
        public static NRNumber Lerp(NRNumber a, NRNumber b, float t)
            => a + (b - a) * Math.Clamp(t, 0f, 1f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NRNumber Floor(NRNumber value)
            => Floor(value, 0);

        private const int _maxDigits = -_normalizedExponentBase;
        public static NRNumber Floor(NRNumber value, int digits)
        {
            if (digits < 0)
                throw new ArgumentOutOfRangeException(nameof(digits));

            var exponent = value._exponent - _normalizedExponentBase;
            var power = _maxDigits - exponent - digits;
            
            if (power > _maxDigits)
                return Zero;
            
            if (power <= 0)
                return value;
            
            var power10 = PowersOf10.LookupI(power);
            value._mantissa /= power10;
            value._mantissa *= power10;
            return value;
        }

        private static class PowersOf10
        {
            //the largest exponent that can appear in a Double, though not all mantissas are valid here.
            private const int _doubleExpMax = 308;

            //The smallest exponent that can appear in a Double, though not all mantissas are valid here.
            private const int _doubleExpMin = -324;
            
            private static readonly double[] _dPowers = new double[_doubleExpMax - _doubleExpMin + 1];
            private static readonly int[] _iPowers = new int[10];

            private const int _indexOf0 = -_doubleExpMin;

            static PowersOf10()
            {
                for (var i = 0; i < _dPowers.Length; ++i)
                    _dPowers[i] = double.Parse("1e" + (i - _indexOf0), CultureInfo.InvariantCulture);

                _iPowers[0] = 1;
                for (var i = 1; i < _iPowers.Length; ++i)
                    _iPowers[i] = _iPowers[i - 1] * 10;
            }

            public static double LookupD(int power) => _dPowers[_indexOf0 + power];
            
            public static int LookupI(int power) => _iPowers[power];
        }
    }
}