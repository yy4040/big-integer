namespace Norify
{
    public class Tests
    {
        [SetUp]
        public void Setup() { }

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
        public void TestFromFloatToString()
        {
            Assert.That(NRNumber.FromFloat(3.14f).ToString(), Is.EqualTo("3.14"));
            
            Assert.That(NRNumber.FromFloat(-123.456f).ToString(), Is.EqualTo("-123.456"));
            
            Assert.That(NRNumber.FromFloat(7e8f).ToString(), Is.EqualTo("700000000"));
            
            Assert.That(NRNumber.FromFloat(123e-10f).ToString(), Is.EqualTo("0.0000000123"));
            
            Assert.That(NRNumber.FromFloat(4e20f).ToString(), Is.EqualTo("400000000000000000000"));
            
            Assert.That(NRNumber.FromFloat(12345.12345f).ToString(), Is.EqualTo("12345.12"));
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
    }
}