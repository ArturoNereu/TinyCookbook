using System;
using System.Collections.Generic;

#if !NET_DOTS
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
            private static readonly Type[] s_AllManagerTypes;

            private readonly static List<Type> m_UserDefinedManagers = new List<Type>();
            private readonly static List<Type> m_TooManyCtorManagers = new List<Type>();
            private readonly static List<Type> m_NoParametersLessCtorManagers = new List<Type>();

            static Cache() 
            {
                s_AllManagerTypes = GetSessionManagerTypes();
            }

            private static Type[] GetSessionManagerTypes()
            {
                var authoringAssembly = typeof(Session).Assembly;
                var friendAssemblyNames = authoringAssembly.GetCustomAttributes()
                                                           .OfType<InternalsVisibleToAttribute>()
                                                           .Select(x => x.AssemblyName);

                return GetLoadableTypes(authoringAssembly)
                            .Concat(friendAssemblyNames.Select(TryLoadAssembly)
                            .Where(a => a != null)
                            .SelectMany(GetLoadableTypes))
                            .Where(x => !x.IsAbstract
                                        && !x.IsGenericType
                                        && !typeof(IIgnoreSessionManager).IsAssignableFrom(x)
                                        && typeof(ISessionManager).IsAssignableFrom(x))
                            .ToArray();
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

            public static ISessionManagerInternal[] PopulateFromCache(SessionFactoryFilter filter)
            {
                var managerInstances = new List<ISessionManagerInternal>();

                m_UserDefinedManagers.Clear();
                m_TooManyCtorManagers.Clear();
                m_NoParametersLessCtorManagers.Clear();

                foreach (var type in filter.FilterManagers(s_AllManagerTypes))
                {
                    if (IsValidManagerType(type, out var reason))
                    {
                        managerInstances.Add((ISessionManagerInternal)Activator.CreateInstance(type));
                    }
                    else
                    {
                        switch (reason)
                        {
                            case SessionManagerRejectionReason.UserDefined:
                                m_UserDefinedManagers.Add(type);
                                break;
                            case SessionManagerRejectionReason.TooManyConstructors:
                                m_TooManyCtorManagers.Add(type);
                                break;
                            case SessionManagerRejectionReason.NoParameterlessConstructor:
                                m_NoParametersLessCtorManagers.Add(type);
                                break;
                        }
                    }
                }

                if (m_UserDefinedManagers.Any())
                {
                    Debug.LogWarning($"User-defined session managers are not currently supported. The following session manager(s) have been ignored: [{string.Join(" ,", m_UserDefinedManagers.Select(t => t.Name))}]");
                }

                if (m_TooManyCtorManagers.Any() || m_NoParametersLessCtorManagers.Any())
                {
                    var message = new StringBuilder().AppendLine($"Invalid internal session managers detected:");
                    if (m_TooManyCtorManagers.Any())
                    {
                        message.AppendLine($"    - Session managers must not define multiple constructors. The following session manager(s) have been ignored: [{string.Join(" ,", m_TooManyCtorManagers.Select(t => t.Name))}]");
                    }
                    if (m_NoParametersLessCtorManagers.Any())
                    {
                        message.AppendLine($"    - Session managers must define 0 or 1 parameterless constructor. The following session manager(s) have been ignored: [{string.Join(" ,", m_NoParametersLessCtorManagers.Select(t => t.Name))}]");
                    }

                    throw new ArgumentException(message.ToString());
                }

                return managerInstances.ToArray();
            }

            private static bool IsValidManagerType(Type type, out SessionManagerRejectionReason reason)
            {
                if (!typeof(ISessionManagerInternal).IsAssignableFrom(type))
                {
                    reason = SessionManagerRejectionReason.UserDefined;
                    return false;
                }

                var constructors = type.GetConstructors();
                if (constructors.Length > 1)
                {
                    reason = SessionManagerRejectionReason.TooManyConstructors;
                    return false;
                }

                if (constructors.Length == 1 && constructors[0].GetParameters().Length > 0)
                {
                    reason = SessionManagerRejectionReason.NoParameterlessConstructor;
                    return false;
                }

                reason = default;
                return true;
            }

            private enum SessionManagerRejectionReason
            {
                UserDefined,
                TooManyConstructors,
                NoParameterlessConstructor
            }
        }
#endif

        private static readonly List<Session> s_Sessions = new List<Session>();

        internal static IEnumerable<Session> Sessions => s_Sessions;

        private readonly ISessionManagerInternal[] m_Managers;

        internal Session(SessionFactoryFilter filter)
        {
#if !NET_DOTS
            m_Managers = Cache.PopulateFromCache(filter);
#endif
            LoadManagers();
            s_Sessions.Add(this);
            SessionCreated(this);
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
#if !NET_DOTS
            foreach (var manager in m_Managers)
            {
                if (manager is TManager typedManager)
                {
                    return typedManager;
                }
            }
#endif
            return null;
        }

        private void LoadManagers()
        {
#if !NET_DOTS
            foreach (var manager in m_Managers)
            {
                manager.Load(this);
            }
#endif
        }

        private void UnloadManagers()
        {
#if !NET_DOTS
            for (var i = m_Managers.Length - 1; i >= 0; --i)
            {
                m_Managers[i].Unload(this);
            }
#endif
        }
    }
}
