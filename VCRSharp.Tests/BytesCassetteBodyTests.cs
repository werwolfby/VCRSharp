using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace VCRSharp.Tests
{
    public class BytesCassetteBodyTests
    {
        [Test]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void Equals_Null_False()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            var differentBytesCassetteBody = (BytesCassetteBody)null;

            Assert.That(stringCassetteBody.Equals(differentBytesCassetteBody), Is.False);
            Assert.That(stringCassetteBody.Equals((object)differentBytesCassetteBody), Is.False);
            Assert.That(stringCassetteBody == differentBytesCassetteBody, Is.False);
            Assert.That(stringCassetteBody != differentBytesCassetteBody, Is.True);
        }
        
        [Test]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void Equals_String_False()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});

            Assert.That(stringCassetteBody.Equals("String"), Is.False);
        }
        
        [Test]
        public void Equals_TheSameObject_True()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            var differentBytesCassetteBody = stringCassetteBody;
            
            Assert.That(stringCassetteBody.Equals(differentBytesCassetteBody), Is.True);
            Assert.That(stringCassetteBody.Equals((object)differentBytesCassetteBody), Is.True);
            Assert.That(stringCassetteBody == differentBytesCassetteBody, Is.True);
            Assert.That(stringCassetteBody != differentBytesCassetteBody, Is.False);
        }
        
        [Test]
        public void Equals_DifferentObjectWithDifferentValue_False()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            var differentBytesCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
            
            Assert.That(stringCassetteBody.Equals(differentBytesCassetteBody), Is.False);
            Assert.That(stringCassetteBody.Equals((object)differentBytesCassetteBody), Is.False);
            Assert.That(stringCassetteBody == differentBytesCassetteBody, Is.False);
            Assert.That(stringCassetteBody != differentBytesCassetteBody, Is.True);
        }
        
        [Test]
        public void Equals_DifferentObjectWithSameValue_True()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            var differentBytesCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            
            Assert.That(stringCassetteBody.Equals(differentBytesCassetteBody), Is.True);
            Assert.That(stringCassetteBody.Equals((object)differentBytesCassetteBody), Is.True);
            Assert.That(stringCassetteBody == differentBytesCassetteBody, Is.True);
            Assert.That(stringCassetteBody != differentBytesCassetteBody, Is.False);
        }

        [Test]
        public void GetHashCode_DifferentObjectsSameBody_EqualHashCode()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            var differentBytesCassetteBody = new BytesCassetteBody(new byte[] {1, 2, 3, 4, 5});
            
            Assert.That(stringCassetteBody.GetHashCode(), Is.EqualTo(differentBytesCassetteBody.GetHashCode()));
        }

        [Test]
        public void GetHashCode_DifferentObjectsDifferentBody_DifferentHashCode()
        {
            var stringCassetteBody = new BytesCassetteBody(new byte[] {1, 2});
            var differentBytesCassetteBody = new BytesCassetteBody(new byte[] {3, 4});
            
            Assert.That(stringCassetteBody.GetHashCode(), Is.Not.EqualTo(differentBytesCassetteBody.GetHashCode()));
        }
    }
}