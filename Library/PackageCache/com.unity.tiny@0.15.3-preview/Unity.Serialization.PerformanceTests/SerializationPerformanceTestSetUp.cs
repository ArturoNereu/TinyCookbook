using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Unity.Serialization.PerformanceTests
{
    /// <summary>
    /// One time setup for all serialization performance tests
    /// </summary>
    [SetUpFixture]
    public class SerializationPerformanceTestSetUp
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // ...
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            // ...
        }
    }
}
