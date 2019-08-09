using NUnit.Framework;
using System.Collections.Generic;

namespace Unity.Authoring.Tests
{
    internal partial class SessionFactoryTests
    {
        private static readonly List<string> s_InitializationMessage = new List<string>();

        [Test]
        public void ShouldLoadAndUnloadInCorrectOrder()
        {
            s_InitializationMessage.Clear();

            using (var session = SessionFactory.Create(SessionFactoryFilter.ExcludeAll
                                                                    .Except<NotifierManagerA>()
                                                                    .Except<NotifierManagerB>()))
            {
            }

            Assert.That(s_InitializationMessage, Is.EqualTo(new[]{
                $"Load {nameof(NotifierManagerA)}",
                $"Load {nameof(NotifierManagerB)}",
                $"Unload {nameof(NotifierManagerB)}",
                $"Unload {nameof(NotifierManagerA)}",
            }));
        }

        private class NotifierManagerA : ISessionManagerInternal, ITestSessionManager
        {
            public void Load(Session session)
            {
                s_InitializationMessage.Add($"Load {nameof(NotifierManagerA)}");
            }

            public void Unload(Session session)
            {
                s_InitializationMessage.Add($"Unload {nameof(NotifierManagerA)}");
            }
        }

        private class NotifierManagerB : ISessionManagerInternal, ITestSessionManager
        {
            public void Load(Session session)
            {
                s_InitializationMessage.Add($"Load {nameof(NotifierManagerB)}");
            }

            public void Unload(Session session)
            {
                s_InitializationMessage.Add($"Unload {nameof(NotifierManagerB)}");
            }
        }
    }
}
