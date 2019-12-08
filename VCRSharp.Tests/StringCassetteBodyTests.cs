using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class StringCassetteBodyTests
    {
        [Test]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void Equals_Null_False()
        {
            var stringCassetteBody = new StringCassetteBody("some text");
            var differentStringCassetteBody = (StringCassetteBody)null;

            Assert.That(stringCassetteBody.Equals(differentStringCassetteBody), Is.False);
            Assert.That(stringCassetteBody.Equals((object)differentStringCassetteBody), Is.False);
            Assert.That(stringCassetteBody == differentStringCassetteBody, Is.False);
            Assert.That(stringCassetteBody != differentStringCassetteBody, Is.True);
        }
        
        [Test]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void Equals_String_False()
        {
            var stringCassetteBody = new StringCassetteBody("some text");

            Assert.That(stringCassetteBody.Equals("String"), Is.False);
        }
        
        [Test]
        public void Equals_TheSameObject_True()
        {
            var stringCassetteBody = new StringCassetteBody("some text");
            var differentStringCassetteBody = stringCassetteBody;
            
            Assert.That(stringCassetteBody.Equals(differentStringCassetteBody), Is.True);
            Assert.That(stringCassetteBody.Equals((object)differentStringCassetteBody), Is.True);
            Assert.That(stringCassetteBody == differentStringCassetteBody, Is.True);
            Assert.That(stringCassetteBody != differentStringCassetteBody, Is.False);
        }
        
        [Test]
        public void Equals_DifferentObjectWithDifferentValue_False()
        {
            var stringCassetteBody = new StringCassetteBody("some text");
            var differentStringCassetteBody = new StringCassetteBody("some other text");
            
            Assert.That(stringCassetteBody.Equals(differentStringCassetteBody), Is.False);
            Assert.That(stringCassetteBody.Equals((object)differentStringCassetteBody), Is.False);
            Assert.That(stringCassetteBody == differentStringCassetteBody, Is.False);
            Assert.That(stringCassetteBody != differentStringCassetteBody, Is.True);
        }
        
        [Test]
        public void Equals_DifferentObjectWithSameValue_True()
        {
            var stringCassetteBody = new StringCassetteBody("some text");
            var differentStringCassetteBody = new StringCassetteBody("some text");
            
            Assert.That(stringCassetteBody.Equals(differentStringCassetteBody), Is.True);
            Assert.That(stringCassetteBody.Equals((object)differentStringCassetteBody), Is.True);
            Assert.That(stringCassetteBody == differentStringCassetteBody, Is.True);
            Assert.That(stringCassetteBody != differentStringCassetteBody, Is.False);
        }

        [Test]
        public void GetHashCode_DifferentObjectsSameBody_EqualHashCode()
        {
            var stringCassetteBody = new StringCassetteBody("some text");
            var differentStringCassetteBody = new StringCassetteBody("some text");
            
            Assert.That(stringCassetteBody.GetHashCode(), Is.EqualTo(differentStringCassetteBody.GetHashCode()));
        }

        [Test]
        public void GetHashCode_DifferentObjectsDifferentBody_DifferentHashCode()
        {
            var stringCassetteBody = new StringCassetteBody("1");
            var differentStringCassetteBody = new StringCassetteBody("2");
            
            Assert.That(stringCassetteBody.GetHashCode(), Is.Not.EqualTo(differentStringCassetteBody.GetHashCode()));
        }

        [TestCase("Value 1")]
        [TestCase("Other Value")]
        [TestCase("Some Text")]
        public void ToString_SomeString_ContainsBody(string value)
        {
            var stringCassetteBody = new StringCassetteBody(value);
            
            Assert.That(stringCassetteBody.ToString(), Contains.Substring(value));
        }
    }
}