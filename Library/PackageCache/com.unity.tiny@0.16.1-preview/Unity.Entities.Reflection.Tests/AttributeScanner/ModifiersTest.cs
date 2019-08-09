using System.Linq;
using NUnit.Framework;
using Unity.Entities.Reflection.Modifiers;

namespace Unity.Entities.Reflection.Tests
{
    internal class ModifiersTest
    {
        public class TestModifierAttribute : ScannableAttribute
        {
            internal TestModifierAttribute(int order = 0)
                : base(order)
            {
            }
        }

        public class OutParameterTypes
        {
            [TestModifier(0)]
            public static void WithOutParameter(out float value)
            {
                value = 54;
            }

            [TestModifier(1)]
            public static void WithRefParameter(ref float value)
            {
                value = 54;
            }

            [TestModifier(2)]
            public static void WithInParameter(in float value)
            {
            }

            [TestModifier(3)]
            public static void WithAllParameter(int v1, ref float v2, out string v3, in double value)
            {
                v3 = "Hey";
            }

            [TestModifier(4)]
            public static string WithAllParameterAndReturn(int v1, ref float v2, out string v3, in double value)
            {
                v3 = "Hey";
                return v3;
            }
        }

        private AttributeScanner<ScannableAttribute> AttributeScanner { get; } = default;

        [Test]
        public void CanSpecifyParameterModifiersTest()
        {
            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestModifierAttribute>()
                    .WithSignature(MethodSignature.Action<Out<float>>())
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(OutParameterTypes.WithOutParameter));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestModifierAttribute>()
                    .WithSignature(MethodSignature.Action<Ref<float>>())
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(OutParameterTypes.WithRefParameter));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestModifierAttribute>()
                    .WithSignature(MethodSignature.Action<In<float>>())
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(OutParameterTypes.WithInParameter));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestModifierAttribute>()
                    .WithSignature(MethodSignature.Action<int, Ref<float>, Out<string>, In<double>>())
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(OutParameterTypes.WithAllParameter));
            }

            {
                var withModifier = AttributeScanner
                    .GetMethodAttributes<TestModifierAttribute>()
                    .WithSignature(MethodSignature.Func<int, Ref<float>, Out<string>, In<double>, string>())
                    .ToList();
                Assert.IsTrue(withModifier.Count == 1);
                Assert.AreEqual(withModifier[0].Method.Name, nameof(OutParameterTypes.WithAllParameterAndReturn));
            }
        }
    }
}
