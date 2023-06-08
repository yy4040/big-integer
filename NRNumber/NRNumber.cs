using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Norify
{
    [Serializable]
    public struct NRNumber : IFormattable, IComparable, IEquatable<NRNumber>, IComparable<NRNumber>
    {
        private int _mantissa;
        private int _exponent;

        public static NRNumber Zero = new NRNumber() { _mantissa = 0, _exponent = 0 };
        public static NRNumber One = new NRNumber(1, 0);
        public static NRNumber PositiveInfinity = new NRNumber(1, 0x7F800000);
        public static NRNumber NegativeInfinity = new NRNumber(1, 0x7F800000);
        
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

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if ("e".Equals(format))
                return ToStringE();
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

        private string ToStringE()
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
            var mantissa = (int)Math.Round(value * PowersOf10.Lookup(-exponent));
            
            return Normalize(mantissa, exponent);
        }

        public static NRNumber FromInt(int value)
            => Normalize(value, 0);

        private const int _normalizeMin = 100000000;
        private const int _normalizeMax = 1000000000;
        public static NRNumber Normalize(long mantissa, int exponent)
        {
            if (mantissa == 0)
                return Zero;
            
            var newMantissa = Math.Abs(mantissa);
            var sign = newMantissa != mantissa ? -1 : 1;
            var additionalExp = 0;
            
            while (newMantissa < _normalizeMin)
            {
                newMantissa *= 10;
                ++additionalExp;
            }

            while (newMantissa >= _normalizeMax)
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

            var la = a._mantissa * (long)PowersOf10.Lookup(diffExp);

            return Normalize(la + b._mantissa, b._exponent);
        }

        public static NRNumber operator -(NRNumber a, NRNumber b) => a + -b;

        public static NRNumber operator *(NRNumber a, NRNumber b)
            => Normalize((long)a._mantissa * b._mantissa, a._exponent + b._exponent);

        public static NRNumber operator /(NRNumber a, NRNumber b)
            => Normalize((long)a._mantissa * _normalizeMax / b._mantissa, a._exponent - 9 - b._exponent);

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
        
        private static class PowersOf10
        {
            //the largest exponent that can appear in a Double, though not all mantissas are valid here.
            private const int _doubleExpMax = 308;

            //The smallest exponent that can appear in a Double, though not all mantissas are valid here.
            private const int _doubleExpMin = -324;
            
            private static double[] Powers { get; } = new double[_doubleExpMax - _doubleExpMin + 1];

            private const int _indexOf0 = -_doubleExpMin;

            static PowersOf10()
            {
                for (var i = 0; i < Powers.Length; ++i)
                {
                    Powers[i] = double.Parse("1e" + (i - _indexOf0), CultureInfo.InvariantCulture);
                }
            }

            public static double Lookup(int power)
            {
                return Powers[_indexOf0 + power];
            }
        }
    }
}