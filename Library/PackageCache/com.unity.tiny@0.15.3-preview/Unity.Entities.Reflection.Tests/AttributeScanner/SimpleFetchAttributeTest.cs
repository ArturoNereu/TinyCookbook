using System.Linq;
using System.Text;
using UnityEngine;
using NUnit.Framework;

namespace Unity.Entities.Reflection.Tests
{
    internal class SimpleFetchAttributeTest
    {
        private class TestSimpleAttribute : ScannableAttribute
        {
            public TestSimpleAttribute(int order = 0)
                : base(order)
            {
            }
        }

        [TestSimple(25)]
        public class TypeWithClassAttribute
        {
        }

        [TestSimple(-1)]
        public static class StaticTypeWithClassAttribute
        {
        }

        public class TypeWithMethodAttribute
        {
            public static string PrivateStaticMethodName => nameof(PrivateStaticMethod);
            public static string PrivateInstanceMethodName => nameof(PrivateInstanceMethod);

            [TestSimple(3)]
            private static void PrivateStaticMethod()
            {
            }

            [TestSimple(2)]
            public static void PublicStaticMethod()
            {
            }

            [TestSimple(1)]
            private void PrivateInstanceMethod()
            {
            }

            [TestSimple(0)]
            public void PublicInstanceMethod()
            {
            }
        }

        private AttributeScanner<ScannableAttribute> AttributeScanner { get; } = default;

        [Test]
        public void CanFetchTypeAttributesTest()
        {
            var typeAttributes = AttributeScanner.GetTypeAttributes<TestSimpleAttribute>()
                .ToList();
            Assert.IsTrue(typeAttributes.Count == 2);
            Assert.AreEqual(typeAttributes[0].Type, typeof(StaticTypeWithClassAttribute));
            Assert.AreEqual(typeAttributes[1].Type, typeof(TypeWithClassAttribute));
        }


        [Test]
        public void CanFetchNonStaticTypeAttributesTest()
        {
            {
                var typeAttributes = AttributeScanner.GetTypeAttributes<TestSimpleAttribute>()
                    .NonStatic()
                    .ToList();
                Assert.IsTrue(typeAttributes.Count == 1);
                Assert.AreEqual(typeAttributes[0].Type, typeof(TypeWithClassAttribute));
            }

            // Same test, but with callback for the mismatch
            {
                var sb = new StringBuilder();
                var typeAttributes = AttributeScanner.GetTypeAttributes<TestSimpleAttribute>()
                    .NonStatic(ta => sb.Append(ta.Type.Name))
                    .ToList();
                Assert.IsTrue(typeAttributes.Count == 1);
                Assert.AreEqual(typeAttributes[0].Type, typeof(TypeWithClassAttribute));
                Assert.AreEqual(typeof(StaticTypeWithClassAttribute).Name, sb.ToString());
            }
        }

        [Test]
        public void CanFetchStaticTypeAttributesTest()
        {
            {
                var typeAttributes = AttributeScanner.GetTypeAttributes<TestSimpleAttribute>()
                    .Static()
                    .ToList();
                Assert.IsTrue(typeAttributes.Count == 1);
                Assert.AreEqual(typeAttributes[0].Type, typeof(StaticTypeWithClassAttribute));
            }

            // Same test, but with callback for the mismatch
            {
                var sb = new StringBuilder();
                var typeAttributes = AttributeScanner.GetTypeAttributes<TestSimpleAttribute>()
                    .Static(ta => sb.Append(ta.Type.Name))
                    .ToList();

                Assert.IsTrue(typeAttributes.Count == 1);
                Assert.AreEqual(typeAttributes[0].Type, typeof(StaticTypeWithClassAttribute));
                Assert.AreEqual(typeof(TypeWithClassAttribute).Name, sb.ToString());
            }
        }

        [Test]
        public void CanFetchMethodAttributesTest()
        {
            var methodAttributes = AttributeScanner.GetMethodAttributes<TestSimpleAttribute>().ToList();
            Assert.IsTrue(methodAttributes.Count == 4);
            Assert.AreEqual(methodAttributes[0].Method.Name, nameof(TypeWithMethodAttribute.PublicInstanceMethod));
            Assert.AreEqual(methodAttributes[1].Method.Name, TypeWithMethodAttribute.PrivateInstanceMethodName);
            Assert.AreEqual(methodAttributes[2].Method.Name, nameof(TypeWithMethodAttribute.PublicStaticMethod));
            Assert.AreEqual(methodAttributes[3].Method.Name, TypeWithMethodAttribute.PrivateStaticMethodName);
        }

        [Test]
        public void CanFetchInstanceMethodAttributesTest()
        {
            var methodAttributes = AttributeScanner.GetMethodAttributes<TestSimpleAttribute>().Instance().ToList();
            Assert.IsTrue(methodAttributes.Count == 2);
            Assert.AreEqual(methodAttributes[0].Method.Name, nameof(TypeWithMethodAttribute.PublicInstanceMethod));
            Assert.AreEqual(methodAttributes[1].Method.Name, TypeWithMethodAttribute.PrivateInstanceMethodName);
        }

        [Test]
        public void CanFetchStaticMethodAttributesTest()
        {
            var methodAttributes = AttributeScanner.GetMethodAttributes<TestSimpleAttribute>().Static().ToList();
            Assert.IsTrue(methodAttributes.Count == 2);
            Assert.AreEqual(methodAttributes[0].Method.Name, nameof(TypeWithMethodAttribute.PublicStaticMethod));
            Assert.AreEqual(methodAttributes[1].Method.Name, TypeWithMethodAttribute.PrivateStaticMethodName);
        }
    }
}
