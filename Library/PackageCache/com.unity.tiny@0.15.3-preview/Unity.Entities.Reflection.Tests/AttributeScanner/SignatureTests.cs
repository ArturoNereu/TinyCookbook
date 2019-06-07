using System.Linq;
using NUnit.Framework;

namespace Unity.Entities.Reflection.Tests
{
    internal class SignatureTests
    {
        public class TestConstraintAttribute : ScannableAttribute
        {
            public TestConstraintAttribute(int order = 0) : base(order)
            {
            }
        }

        public class TypeWithMethodConstraintsAttribute
        {
            [TestConstraint(0)]
            public int ReturnsInt() => default;

            [TestConstraint(1)]
            public int FloatToInt(float value) => default;

            [TestConstraint(3)]
            public void VoidToVoid()
            {
            }

            [TestConstraint(2)]
            public void FloatToVoid(float value)
            {
            }

            [TestConstraint(3)]
            public static void FloatToVoid2(float value)
            {
            }
        }

        public abstract class GenericBaseClass<T>
        {
            public abstract void TToVoid(T t);
        }

        public class FloatClass : GenericBaseClass<float>
        {
            [TestConstraint(10)]
            public override void TToVoid(float t)
            {
            }
        }

        private AttributeScanner<ScannableAttribute> AttributeScanner { get; } = default;

        [Test]
        public void CanFetchMethodWithConstraintTest()
        {
            var returnsInt = AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .WithSignature(MethodSignature.Func<int>())
                .ToList();
            Assert.IsTrue(returnsInt.Count == 1);
            Assert.AreEqual(returnsInt[0].Method.Name, nameof(TypeWithMethodConstraintsAttribute.ReturnsInt));

            var floatToInt = AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .WithSignature(MethodSignature.Func<float, int>())
                .ToList();
            Assert.IsTrue(floatToInt.Count == 1);
            Assert.AreEqual(floatToInt[0].Method.Name, nameof(TypeWithMethodConstraintsAttribute.FloatToInt));

            var voidToVoid = AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .WithSignature(MethodSignature.Action())
                .ToList();
            Assert.IsTrue(voidToVoid.Count == 1);
            Assert.AreEqual(voidToVoid[0].Method.Name, nameof(TypeWithMethodConstraintsAttribute.VoidToVoid));

            var floatToVoid = AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .WithSignature(MethodSignature.Action<float>())
                .ToList();
            Assert.IsTrue(floatToVoid.Count == 3);
            Assert.AreEqual(floatToVoid[0].Method.Name, nameof(TypeWithMethodConstraintsAttribute.FloatToVoid));
            Assert.AreEqual(floatToVoid[1].Method.Name, nameof(TypeWithMethodConstraintsAttribute.FloatToVoid2));
            Assert.AreEqual(floatToVoid[2].Method.Name, nameof(FloatClass.TToVoid));

            var multipleConstraints = AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .Static()
                .WithSignature(MethodSignature.Action<float>())
                .ToList();
            Assert.IsTrue(multipleConstraints.Count == 1);
            Assert.AreEqual(multipleConstraints[0].Method.Name,
                nameof(TypeWithMethodConstraintsAttribute.FloatToVoid2));

            Assert.IsFalse(AttributeScanner
                .GetMethodAttributes<TestConstraintAttribute>()
                .Static()
                .Instance()
                .Any());
        }
    }
}
