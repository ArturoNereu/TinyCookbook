using NUnit.Framework;
using System;
using UnityEngine.TestTools;

namespace Unity.Authoring.Tests
{
    [TestFixture]
    internal partial class SessionFactoryTests
    {
        [Test]
        public void ShouldIncludeAllExceptTestManagers()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.IncludeAll);

            Assert.That(session.GetManager<ManagerA>(), Is.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Null);
            Assert.That(session.GetManager<ManagerC>(), Is.Null);
        }

        [Test]
        public void ShouldIncludeEverythingExceptManagerA()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.IncludeAllWithUnitTests
                                                                     .Except<ManagerA>());

            Assert.That(session.GetManager<ManagerA>(), Is.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Not.Null);
        }

        [Test]
        public void ShouldIncludeEverythingExceptBaseClassX()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.IncludeAllWithUnitTests.ExceptAll<IBaseManagerX>());

            Assert.That(session.GetManager<ManagerA>(), Is.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Not.Null);
        }

        [Test]
        public void ShouldIncludeEverythingExceptCustomFilters()
        {

            var session = SessionFactory.Create(SessionFactoryFilter.IncludeAllWithUnitTests
                                                                     .Except(t => t.Name == nameof(ManagerA)));

            Assert.That(session.GetManager<ManagerA>(), Is.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Not.Null);
        }

        [Test]
        public void ShouldExcludeEverythingExceptManagerA()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll.Except<ManagerA>());

            Assert.That(session.GetManager<ManagerA>(), Is.Not.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Null);
        }

        [Test]
        public void ShouldExcludeEverythingExceptBaseClassX()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll.ExceptAll<IBaseManagerX>());

            Assert.That(session.GetManager<ManagerA>(), Is.Not.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Null);
        }

        [Test]
        public void ShouldExcludeEverythingExceptCustomFilters()
        {

            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                     .Except(t => t.Name == nameof(ManagerA))
                                                                     .Except(t => t.Name == nameof(ManagerB)));

            Assert.That(session.GetManager<ManagerA>(), Is.Not.Null);
            Assert.That(session.GetManager<ManagerB>(), Is.Not.Null);
        }

        [Test]
        public void ShouldIncludeExplicitelyEvenIgnoredManagers()
        {
            // ManagerC is IIgnoreSessionManager so it should be ignored even when there's no fitlering enabled
            Assert.That(SessionFactory.Create(SessionFactoryFilter.IncludeAllWithUnitTests).GetManager<ManagerC>(), Is.Null);

            // But it should be added when requested
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll.ExplicitlyInclude<ManagerC>());
            Assert.That(session.GetManager<ManagerC>(), Is.Not.Null);
        }

        [Test]
        public void UserDefinedSessionManagersShouldBeDetectedAndIgnored()
        {
            var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll.ExplicitlyInclude<InvalidManagerA>());

            Assert.That(session.GetManager<InvalidManagerA>(), Is.Null);
            LogAssert.Expect(UnityEngine.LogType.Warning, $"User-defined session managers are not currently supported. The following session manager(s) have been ignored: [{nameof(InvalidManagerA)}]");
        }

        [Test]
        public void InvalidSessionManagersShouldBeDetected()
        {
            var exception = Assert.Throws<ArgumentException>(() => SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                                                     .ExplicitlyInclude<InvalidManagerB>()
                                                                                                     .ExplicitlyInclude<InvalidManagerC>()));

            var expectedMessage = $"Invalid internal session managers detected:{Environment.NewLine}" +
                $"    - Session managers must not define multiple constructors. The following session manager(s) have been ignored: [{nameof(InvalidManagerC)}]{Environment.NewLine}" +
                $"    - Session managers must define 0 or 1 parameterless constructor. The following session manager(s) have been ignored: [{nameof(InvalidManagerB)}]{Environment.NewLine}";

            Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        }

        private interface IBaseManagerX { }

        private class ManagerA : ISessionManagerInternal, IBaseManagerX, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class ManagerB : ISessionManagerInternal, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class ManagerC : ISessionManagerInternal, IIgnoreSessionManager, ITestSessionManager
        {
            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class InvalidManagerA : ISessionManager, IIgnoreSessionManager, ITestSessionManager { }

        private class InvalidManagerB : ISessionManagerInternal, IIgnoreSessionManager, ITestSessionManager
        {
            public InvalidManagerB(int i) { }

            public void Load(Session session) { }

            public void Unload(Session session) { }
        }

        private class InvalidManagerC : ISessionManagerInternal, IIgnoreSessionManager, ITestSessionManager
        {
            public InvalidManagerC() { }

            public InvalidManagerC(int i) { }

            public void Load(Session session) { }

            public void Unload(Session session) { }
        }
    }
}
