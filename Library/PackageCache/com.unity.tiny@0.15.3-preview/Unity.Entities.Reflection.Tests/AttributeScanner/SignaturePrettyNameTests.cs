using NUnit.Framework;

namespace Unity.Entities.Reflection.Tests
{
    internal class SignaturePrettyNameTests
    {
        private interface ITestName{}
        private interface ITestGenericName<T> : ITestName{}
        private class TestGeneric<T> : ITestGenericName<T> {}
        private class IntGeneric : TestGeneric<TestGeneric<int>> {}


        [Test]
        public void SignatureNameMatchesTest()
        {
            Assert.AreEqual("()=>void", MethodSignature.Action().GetSignatureName());
            Assert.AreEqual("(string)=>void", MethodSignature.Action<string>().GetSignatureName());
            Assert.AreEqual("(float, string)=>void", MethodSignature.Action<float, string>().GetSignatureName());
            Assert.AreEqual("(TestGeneric<string>)=>void", MethodSignature.Action<TestGeneric<string>>().GetSignatureName());
            Assert.AreEqual("(TestGeneric<TestGeneric<int>>)=>void", MethodSignature.Action<TestGeneric<TestGeneric<int>>>().GetSignatureName());
            Assert.AreEqual("()=>float", MethodSignature.Func<float>().GetSignatureName());
            Assert.AreEqual("(string)=>int", MethodSignature.Func<string, int>().GetSignatureName());
            Assert.AreEqual("(float, string)=>TestGeneric<double>", MethodSignature.Func<float, string, TestGeneric<double>>().GetSignatureName());
            Assert.AreEqual("(IntGeneric)=>sbyte", MethodSignature.Func<IntGeneric, sbyte>().GetSignatureName());
            Assert.AreEqual("(ITestName)=>ITestGenericName<ulong>", MethodSignature.Func<ITestName, ITestGenericName<ulong>>().GetSignatureName());
        }
    }
}
