using System;
using System.Collections.Generic;

#if !NET_DOTS
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
#endif

namespace Unity.Authoring
{
    /// <summary>
    /// A session encapsulates an editing context.
    /// </summary>
    public class Session : IDisposable
    {
        internal static event Action<Session> SessionCreated = delegate { };
        internal static event Action<Session> SessionDisposing = delegate { };
        internal static event Action<Session> SessionDisposed = delegate { };

#if !NET_DOTS
        private static class Cache
        {
            private delegate ISessionManager ConstructManagerHandler(Session session);
            private static readonly List<ConstructManagerHandler> s_ManagerConstructors;

            static Cache()
            {
                Type[] constructorParameters = { typeof(Session) };
                var constructMethod = typeof(Cache).GetMethod(nameof(ConstructFromType), BindingFlags.NonPublic | BindingFlags.Static);

                if (null == constructMethod)
                {
                    throw new NullReferenceException($"Failed to resolve method {nameof(Cache)}.{nameof(ConstructFromType)}");
                }

                var managerTypes = GetSessionManagerTypes()
                    .Where(type => null == type.GetInterface(nameof(IIgnoreSessionManager)))
                    .ToArray();

                s_ManagerConstructors = new List<ConstructManagerHandler>(managerTypes.Length);

                for (var i = 0; i < managerTypes.Length; ++i)
                {
                    var type = managerTypes[i];

                    if (type.IsAbstract || type.IsGenericType)
                    {
                        continue;
                    }

                    if (null == type.GetConstructor(constructorParameters))
                    {
                        throw new ArgumentException($"{type.FullName}: A Session manager needs to have a constructor taking an {nameof(Session)} parameter.");
                    }

                    s_ManagerConstructors.Add((ConstructManagerHandler) Delegate.CreateDelegate(typeof(ConstructManagerHandler), constructMethod.MakeGenericMethod(type)));
                }
            }

            private static IEnumerable<Type> GetSessionManagerTypes()
            {
                var authoringAssembly = typeof(Session).Assembly;
                var friendAssemblyNames = authoringAssembly.GetCustomAttributes()
                                                           .OfType<InternalsVisibleToAttribute>()
                                                           .Select(x => x.AssemblyName);


                return GetLoadableTypes(authoringAssembly)
                    .Concat(friendAssemblyNames.Select(TryLoadAssembly)
                        .Where(a => a != null)
                        .SelectMany(GetLoadableTypes))
                    .Where(x => typeof(ISessionManager).IsAssignableFrom(x));
            }

            private static Assembly TryLoadAssembly(string assemblyName)
            {
                try
                {
                    return Assembly.Load(assemblyName);
                }
                catch
                {
                    return null;
                }
            }

            private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    return e.Types.Where(t => t != null);
                }
            }

            public static void PopulateFromCache(Session session, out List<ISessionManagerInternal> managers)
            {
                managers = new List<ISessionManagerInternal>(s_ManagerConstructors.Count);
                for (var i = 0; i < s_ManagerConstructors.Count; ++i)
                {
                    var manager = s_ManagerConstructors[i].Invoke(session);

                    if (!(manager is ISessionManagerInternal internalManager))
                    {
                        throw new Exception($"{manager.GetType()}: A Session manager must implement ISessionManagerInternal");
                    }

                    managers.Add(internalManager);
                }
            }

            private static ISessionManager ConstructFromType<TManager>(Session session)
                where TManager : SessionManager
            {
                return (ISessionManager) Activator.CreateInstance(typeof(TManager), session);
            }
        }
#endif

        private static readonly List<Session> s_Sessions = new List<Session>();

        internal static IEnumerable<Session> Sessions => s_Sessions;

        private readonly List<ISessionManagerInternal> m_Managers;

        internal static Session Create()
        {
            var session = new Session();
            s_Sessions.Add(session);
            SessionCreated(session);
            return session;
        }

        private Session()
        {
#if !NET_DOTS
            Cache.PopulateFromCache(this, out m_Managers);
#endif
            LoadManagers();
        }

        public void Dispose()
        {
            SessionDisposing(this);
            UnloadManagers();
            s_Sessions.Remove(this);
            SessionDisposed(this);
        }

        /// <summary>
        /// Returns a manager of type <see cref="TManager"/> if it is created.
        /// </summary>
        /// <typeparam name="TManager">The <see cref="SessionManager"/> type.</typeparam>
        /// <returns>An instance of <see cref="TManager"/> or null</returns>
        public TManager GetManager<TManager>() where TManager : class, ISessionManager
        {
            for (var i = 0; i < m_Managers.Count; ++i)
            {
                if (m_Managers[i] is TManager manager)
                {
                    return manager;
                }
            }

            return null;
        }

        private void LoadManagers()
        {
            for (var i = 0; i < m_Managers.Count; ++i)
            {
                m_Managers[i].Load();
            }
        }

        private void UnloadManagers()
        {
            for (var i = m_Managers.Count - 1; i >= 0; --i)
            {
                m_Managers[i].Unload();
            }
        }
    }
}
