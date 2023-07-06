using System.Globalization;

namespace Norify
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            NRNumber.CustomToString = (number, format, _) =>
            {
                if ("e".Equals(format, StringComparison.InvariantCultureIgnoreCase))
                    return number.ToStringE();
                return null;
            };
        }

        [Test]
        public void TestFromIntToString()
        {
            Assert.That(NRNumber.FromInt(123).ToString(), Is.EqualTo("123"));
            
            Assert.That(NRNumber.FromInt(7000000).ToString(), Is.EqualTo("7000000"));
            
            Assert.That(NRNumber.FromInt(-987654321).ToString(), Is.EqualTo("-987654321"));
            
            Assert.That(NRNumber.FromInt(-1987654321).ToString(), Is.EqualTo("-1987654320"));
            
            Assert.That(NRNumber.FromInt(2000000007).ToString(), Is.EqualTo("2000000000"));
        }
        
        [Test]
        public void TestToStringE()
        {
            Assert.That(new NRNumber(1, -2147483639).ToString("e", CultureInfo.InvariantCulture), Is.EqualTo("1e-2147483639"));
            
            Assert.That(new NRNumber(1, 2147483638).ToString("e", CultureInfo.InvariantCulture), Is.EqualTo("1e2147483638"));
            
            Assert.That(NRNumber.FromFloat(3.141592f).ToString("e", CultureInfo.InvariantCulture), Is.EqualTo("3.141592"));
            
            Assert.That(NRNumber.FromFloat(7e8f).ToString("e", CultureInfo.InvariantCulture), Is.EqualTo("7e8"));
            
            Assert.That(NRNumber.FromFloat(123e-10f).ToString("e", CultureInfo.InvariantCulture), Is.EqualTo("1.23e-8"));
        }

        [Test]
        public void TestFromFloatToString()
        {
            Assert.That(NRNumber.FromFloat(3.14f).ToString(), Is.EqualTo("3.14"));
            
            Assert.That(NRNumber.FromFloat(-123.456f).ToString(), Is.EqualTo("-123.456"));
            
            Assert.That(NRNumber.FromFloat(7e8f).ToString(), Is.EqualTo("700000000"));
            
            Assert.That(NRNumber.FromFloat(123e-10f).ToString(), Is.EqualTo("0.0000000123"));
            
            Assert.That(NRNumber.FromFloat(4e20f).ToString(), Is.EqualTo("400000000000000000000"));
            
            Assert.That(NRNumber.FromFloat(12345.12345f).ToString("zz", CultureInfo.InvariantCulture), Is.EqualTo("12345.12"));
        }

        [Test]
        public void TestToFloat()
        {
            Assert.That(NRNumber.FromFloat(3.141592f).ToFloat(), Is.EqualTo(3.141592f));
            
            Assert.That(new NRNumber(1234567, -5).ToFloat(), Is.EqualTo(12.34567f));
            
            Assert.That(new NRNumber(5, 37).ToFloat(), Is.EqualTo(5e37f));
            
            Assert.That(new NRNumber(5, 38).ToFloat(), Is.EqualTo(float.MaxValue));
            
            Assert.That(new NRNumber(-5, 37).ToFloat(), Is.EqualTo(-5e37f));
            
            Assert.That(new NRNumber(-5, 38).ToFloat(), Is.EqualTo(float.MinValue));
        }
        
        [Test]
        public void TestFromDoubleToString()
        {
            Assert.That(NRNumber.FromDouble(3.14).ToString(), Is.EqualTo("3.14"));
            
            Assert.That(NRNumber.FromDouble(-123.456).ToString(), Is.EqualTo("-123.456"));
            
            Assert.That(NRNumber.FromDouble(7e8).ToString(), Is.EqualTo("700000000"));
            
            Assert.That(NRNumber.FromDouble(123e-10).ToString(), Is.EqualTo("0.0000000123"));
            
            Assert.That(NRNumber.FromDouble(4e20).ToString(), Is.EqualTo("400000000000000000000"));
            
            Assert.That(NRNumber.FromDouble(12345.12345).ToString(), Is.EqualTo("12345.1234"));
        }
        
        [Test]
        public void TestFromString()
        {
            Assert.That(NRNumber.FromString("+3"), Is.EqualTo(NRNumber.FromDouble(3)));
            Assert.That(NRNumber.FromString("3.2e23"), Is.EqualTo(NRNumber.FromDouble(3.2e23)));
            Assert.That(NRNumber.FromString("-4.70e+9"), Is.EqualTo(NRNumber.FromDouble(-4.70e+9)));
            Assert.That(NRNumber.FromString("-0.2E-123"), Is.EqualTo(NRNumber.FromDouble(-0.2E-123)));
            Assert.That(NRNumber.FromString("-7.660333441"), Is.EqualTo(NRNumber.FromDouble(-7.660333441)));
            Assert.That(NRNumber.FromString("+0003"), Is.EqualTo(NRNumber.FromDouble(+0003)));
            Assert.That(NRNumber.FromString("37.e88"), Is.EqualTo(new NRNumber(37, 88)));
            Assert.That(NRNumber.FromString("123a"), Is.EqualTo(NRNumber.Zero));
        }

        [Test]
        public void TestEqualsNRNumbers()
        {
            Assert.That(new NRNumber(50000, 0), Is.EqualTo(new NRNumber(500, 2)));
            
            Assert.That(NRNumber.FromFloat(5e4f), Is.EqualTo(new NRNumber(500, 2)));
            
            Assert.That(new NRNumber(-31400000, -7), Is.EqualTo(new NRNumber(-314, -2)));
            
            Assert.That((NRNumber)1000, Is.EqualTo(new NRNumber(1, 3)));
            
            Assert.That((NRNumber)1000, Is.EqualTo(new NRNumber(10, 2)));
        }

        [Test]
        public void TestAdd()
        {
            Assert.That(NRNumber.FromInt(3) + 7, Is.EqualTo(NRNumber.FromInt(10)));
            
            Assert.That(NRNumber.FromFloat(111.111f) + 222.222f, Is.EqualTo(NRNumber.FromFloat(333.333f)));
            
            Assert.That(NRNumber.FromInt(10) + NRNumber.FromInt(-2), Is.EqualTo(NRNumber.FromInt(8)));
            
            Assert.That(NRNumber.FromFloat(5e10f) + NRNumber.FromFloat(1f), Is.EqualTo(NRNumber.FromFloat(5e10f)));
            
            Assert.That(NRNumber.FromFloat(5e9f) + NRNumber.FromFloat(1f), Is.EqualTo(NRNumber.FromFloat(5e9f)));
            
            Assert.That(NRNumber.FromFloat(5e8f) + NRNumber.FromFloat(1f), Is.EqualTo(NRNumber.FromInt(500000001)));
        }

        [Test]
        public void TestSubtract()
        {
            Assert.That(NRNumber.FromInt(1000000) - 1, Is.EqualTo(NRNumber.FromInt(999999)));
            
            Assert.That(NRNumber.FromInt(-999999) - 1, Is.EqualTo(NRNumber.FromInt(-1000000)));
            
            Assert.That(new NRNumber(123456789, 30) - new NRNumber(5, 30), Is.EqualTo(new NRNumber(123456784, 30)));
            
            Assert.That(new NRNumber(123456789, 31) - new NRNumber(5, 30), Is.EqualTo(new NRNumber(123456789, 31)));
            
            Assert.That(new NRNumber(123456789, -30) - new NRNumber(5, -30), Is.EqualTo(new NRNumber(123456784, -30)));
            
            Assert.That(new NRNumber(123456789, -29) - new NRNumber(5, -30), Is.EqualTo(new NRNumber(123456789, -29)));
        }

        [Test]
        public void TestMultiply()
        {
            Assert.That(new NRNumber(1, 70) * 250, Is.EqualTo(new NRNumber(250, 70)));
            
            Assert.That(NRNumber.FromFloat(-333.2222f) * -3e10f, Is.EqualTo(NRNumber.FromFloat(9.996666e12f)));
            
            Assert.That(NRNumber.FromInt(50000) * 0.2f, Is.EqualTo(NRNumber.FromInt(10000)));
        }

        [Test]
        public void TestDivide()
        {
            Assert.That(new NRNumber(100, 330) / new NRNumber(3, 330), Is.EqualTo(new NRNumber(333333333, -7)));
            
            Assert.That(-NRNumber.FromFloat(791.2f) / 4.5f, Is.EqualTo(new NRNumber(-175822222, -6)));
        }

        [Test]
        public void TestCompareTo()
        {
            Assert.That(NRNumber.FromInt(0).CompareTo(0), Is.Zero);
            
            Assert.That(NRNumber.FromInt(1).CompareTo(0), Is.Positive);
            
            Assert.That(NRNumber.FromInt(0).CompareTo(-1), Is.Positive);
            
            Assert.That(NRNumber.FromInt(0).CompareTo(1), Is.Negative);
            
            Assert.That(NRNumber.FromInt(-1).CompareTo(0), Is.Negative);
            
            Assert.That(NRNumber.FromInt(72).CompareTo(80), Is.Negative);
            
            Assert.That(NRNumber.FromFloat(-8.774e24f).CompareTo(-5.34e25f), Is.Positive);
            
            Assert.That(NRNumber.FromFloat(8.774e24f).CompareTo(5.34e25f), Is.Negative);
            
            Assert.That(NRNumber.FromInt(1234567000).CompareTo(1.234567e9f), Is.Zero);
        }

        [Test]
        public void TestCompareOperator()
        {
            Assert.That(NRNumber.Zero < NRNumber.One, Is.True);
            
            Assert.That(NRNumber.One < 1, Is.False);
            
            Assert.That(NRNumber.One <= 1, Is.True);
            
            Assert.That(new NRNumber(7574, 12) > new NRNumber(3, 450), Is.False);
            
            Assert.That(new NRNumber(7574, 12) > new NRNumber(-3, 450), Is.True);
            
            Assert.That(new NRNumber(7574, 12) > new NRNumber(3, -450), Is.True);
        }

        [Test]
        public void TestFloor()
        {
            {
                var testValue = NRNumber.FromString("9.87654321");

                Assert.Throws<ArgumentOutOfRangeException>(() => NRNumber.Floor(testValue, -1));

                Assert.That(NRNumber.Floor(testValue, 0), Is.EqualTo(NRNumber.FromString("9.00000000")));
                Assert.That(NRNumber.Floor(testValue, 1), Is.EqualTo(NRNumber.FromString("9.80000000")));
                Assert.That(NRNumber.Floor(testValue, 2), Is.EqualTo(NRNumber.FromString("9.87000000")));
                Assert.That(NRNumber.Floor(testValue, 3), Is.EqualTo(NRNumber.FromString("9.87600000")));
                Assert.That(NRNumber.Floor(testValue, 4), Is.EqualTo(NRNumber.FromString("9.87650000")));
                Assert.That(NRNumber.Floor(testValue, 5), Is.EqualTo(NRNumber.FromString("9.87654000")));
                Assert.That(NRNumber.Floor(testValue, 6), Is.EqualTo(NRNumber.FromString("9.87654300")));
                Assert.That(NRNumber.Floor(testValue, 7), Is.EqualTo(NRNumber.FromString("9.87654320")));
                Assert.That(NRNumber.Floor(testValue, 8), Is.EqualTo(NRNumber.FromString("9.87654321")));
                Assert.That(NRNumber.Floor(testValue, 9), Is.EqualTo(NRNumber.FromString("9.87654321")));
            }
            
            {
                var testValue = NRNumber.FromString("-123456.789");

                Assert.That(NRNumber.Floor(testValue, 0), Is.EqualTo(NRNumber.FromString("-123456.000")));
                Assert.That(NRNumber.Floor(testValue, 1), Is.EqualTo(NRNumber.FromString("-123456.700")));
                Assert.That(NRNumber.Floor(testValue, 2), Is.EqualTo(NRNumber.FromString("-123456.780")));
                Assert.That(NRNumber.Floor(testValue, 3), Is.EqualTo(NRNumber.FromString("-123456.789")));
                Assert.That(NRNumber.Floor(testValue, 4), Is.EqualTo(NRNumber.FromString("-123456.789")));
            }
            
            {
                var testValue = NRNumber.FromString("123e-20");

                Assert.That(NRNumber.Floor(testValue, 17), Is.EqualTo(NRNumber.FromString("0e-20")));
                Assert.That(NRNumber.Floor(testValue, 18), Is.EqualTo(NRNumber.FromString("100e-20")));
                Assert.That(NRNumber.Floor(testValue, 19), Is.EqualTo(NRNumber.FromString("120e-20")));
                Assert.That(NRNumber.Floor(testValue, 20), Is.EqualTo(NRNumber.FromString("123e-20")));
            }
            
            Assert.That(NRNumber.Floor(new NRNumber(777, 100)), Is.EqualTo(NRNumber.FromString("777e100")));
        }
    }
}