using NUnit.Framework;

namespace Unity.Authoring.Tests
{
    internal partial class SessionFactoryTests
    {
        [Test]
        public void ShouldReplaceTypesByMockWhenRequested()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                    .ExplicitlyInclude<ActualImplementation>());

            Assert.That(session.GetManager<IBaseInterface>(), Is.Not.Null);

            var session2 = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                     .ExplicitlyInclude<MockImplementation>()
                                                                     .SetupMock<IBaseInterface, MockImplementation>());

            var manager = session2.GetManager<IBaseInterface>();
            Assert.That(manager, Is.Not.Null);
            Assert.That(manager, Is.InstanceOf<MockImplementation>());
        }

        [Test]
        public void ShouldExplicitelyIncludeMockTypeWhenSettingUpMock()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                    .SetupMock<IBaseInterface, MockImplementation>());

            Assert.That(session.GetManager<MockImplementation>, Is.Not.Null);
        }

        [Test]
        public void ShouldReplaceMockedType()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                     .ExplicitlyInclude<ActualImplementation>()
                                                                     .ExplicitlyInclude<MockImplementation>()
                                                                     .ExplicitlyInclude<MockImplementation2>()
                                                                     .SetupMock<IBaseInterface, MockImplementation>()
                                                                     .SetupMock<IBaseInterface, MockImplementation2>());

            Assert.That(session.GetManager<IBaseInterface>(), Is.InstanceOf<MockImplementation2>());
            Assert.That(session.GetManager<ActualImplementation>(), Is.Null);
            Assert.That(session.GetManager<MockImplementation>(), Is.Null);
        }

        private interface IBaseInterface : ISessionManagerInternal { }

        private class ActualImplementation : IBaseInterface, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class MockImplementation : IBaseInterface, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class MockImplementation2 : IBaseInterface, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }
    }
}
