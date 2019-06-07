using System.Linq;
using NUnit.Framework;

namespace Unity.Entities.Reflection.Tests
{
    internal class VisibilityTest
    {
        public class TestVisibilityAttribute : ScannableAttribute
        {
            internal TestVisibilityAttribute(int order = 0)
                : base(order)
            {
            }
        }

        public class VisibilityTypes
        {
            public static string StaticPrivateName => nameof(StaticPrivate);
            public static string InstancePrivateName => nameof(InstancePrivate);
            public static string InstanceProtectedName => nameof(InstanceProtected);

            [TestVisibility(0)]
            public static void StaticPublic()
            {
            }

            [TestVisibility(1)]
            private static void StaticPrivate()
            {
            }

            [TestVisibility(2)]
            internal static void StaticInternal()
            {
            }

            [TestVisibility(3)]
            public void InstancePublic()
            {
            }

            [TestVisibility(4)]
            private void InstancePrivate()
            {
            }

            [TestVisibility(5)]
            internal void InstanceInternal()
            {
            }

            [TestVisibility(6)]
            protected void InstanceProtected()
            {
            }
        }

        private AttributeScanner<ScannableAttribute> AttributeScanner { get; } = default;

        [Test]
        public void CanDetectVisibilityTest()
        {
            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Static()
                    .Public()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(VisibilityTypes.StaticPublic));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Static()
                    .Private()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, VisibilityTypes.StaticPrivateName);
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Static()
                    .Internal()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(VisibilityTypes.StaticInternal));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Instance()
                    .Public()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(VisibilityTypes.InstancePublic));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Instance()
                    .Private()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, VisibilityTypes.InstancePrivateName);
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Instance()
                    .Internal()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(VisibilityTypes.InstanceInternal));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestVisibilityAttribute>()
                    .WithSignature(MethodSignature.Action())
                    .Instance()
                    .Protected()
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, VisibilityTypes.InstanceProtectedName);
            }
        }
    }
}
