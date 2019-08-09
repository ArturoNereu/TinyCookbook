using System.Linq;
using NUnit.Framework;
using Unity.Entities.Reflection.Modifiers;

namespace Unity.Entities.Reflection.Tests
{
    internal class CallableWithTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public class TestCallableAttribute : ScannableAttribute
        {
            internal TestCallableAttribute(int order = 0)
                : base(order)
            {
            }
        }

        public interface IBase
        {
        }

        public interface IDerived : IBase
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Concrete : IDerived
        {

        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once ClassNeverInstantiated.Global
        public class CallableTypes
        {
            [TestCallable(0)]
            public static void TakeBase(IBase argument)
            {
            }

            [TestCallable(1)]
            public static void TakeDerived(IDerived argument)
            {
            }

            [TestCallable(2)]
            public static void TakeConcrete(Concrete argument)
            {
            }

            [TestCallable(3)]
            public static void TakeBaseRef(ref IBase argument)
            {
            }

            [TestCallable(4)]
            public static void TakeDerivedRef(ref IDerived argument)
            {
            }

            [TestCallable(5)]
            public static void TakeConcreteRef(ref Concrete argument)
            {
            }

            [TestCallable(6)]
            public static void TakeOptionalParameters(int first, float second = 5.0f)
            {

            }
        }

        private AttributeScanner<ScannableAttribute> AttributeScanner { get; } = default;

        [Test]
        public void CallableWithTest()
        {
            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<IBase>()).ToList();

                Assert.AreEqual(1, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBase));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<IDerived>()).ToList();

                Assert.AreEqual(2, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBase));
                Assert.AreEqual(callables[1].Method.Name, nameof(CallableTypes.TakeDerived));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<Concrete>()).ToList();

                Assert.AreEqual(3, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBase));
                Assert.AreEqual(callables[1].Method.Name, nameof(CallableTypes.TakeDerived));
                Assert.AreEqual(callables[2].Method.Name, nameof(CallableTypes.TakeConcrete));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<Ref<IBase>>()).ToList();

                Assert.AreEqual(1, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBaseRef));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<Ref<IDerived>>()).ToList();

                Assert.AreEqual(2, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBaseRef));
                Assert.AreEqual(callables[1].Method.Name, nameof(CallableTypes.TakeDerivedRef));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<int>()).ToList();

                Assert.AreEqual(1, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeOptionalParameters));
            }

            {
                var callables = AttributeScanner
                    .GetMethodAttributes<TestCallableAttribute>()
                    .CallableWith(MethodSignature.Params<IBase>()).ToList();

                Assert.AreEqual(1, callables.Count);
                Assert.AreEqual(callables[0].Method.Name, nameof(CallableTypes.TakeBase));
            }
        }
    }
}
