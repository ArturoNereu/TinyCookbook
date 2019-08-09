#if !NET_DOTS
using System;
using System.Collections.Generic;
#endif

namespace Unity.Authoring
{
#if !NET_DOTS
    /// <summary>
    /// Factory allowing to create new <see cref="Session"/> supporting filtering
    /// when used with <see cref="SessionFactoryFilter"/>.
    /// </summary>
    public static class SessionFactory
    {
        /// <summary>
        /// Creates a new <see cref="Session"/> containing all <see cref="ISessionManager"/> available. 
        /// </summary>
        /// <returns>A new <see cref="Session"/></returns>
        public static Session Create() => new Session(SessionFactoryFilter.IncludeAll);

        /// <summary>
        /// Creates a new <see cref="Session"/> containing all <see cref="ISessionManager"/> satisfying
        /// the configured <see cref="SessionFactoryFilter"/>. 
        /// </summary>
        /// <returns>A new <see cref="Session"/></returns>
        public static Session Create(SessionFactoryFilter filter) => new Session(filter);
    }
#endif

    /// <summary>
    /// Class supporting APIs to filter which <see cref="ISessionManager"/> are going to be included 
    /// in the <see cref="Session"/> constructed by <see cref="SessionFactory.Create(SessionFactoryFilter)"/>.
    /// </summary>
    public class SessionFactoryFilter
    {
#if !NET_DOTS
        private readonly bool m_IncludeAllByDefault;
        private readonly HashSet<Type> m_ExplicitIncludes = new HashSet<Type>();
        private readonly List<Func<Type, bool>> m_Constraints = new List<Func<Type, bool>>();
        private readonly Dictionary<Type, Type> m_MockMapping = new Dictionary<Type, Type>();

        /// <summary>
        /// Initialize a <see cref="SessionFactoryFilter"/> including all available <see cref="ISessionManager"/>s.
        /// Use <see cref="SessionFactoryFilterExtensions.Except{TManager}(SessionFactoryFilter)"/> to remove a specific <see cref="ISessionManager"/>.
        /// </summary>
        public static SessionFactoryFilter IncludeAll => new SessionFactoryFilter(includeAllByDefault: true).ExceptAll<ITestSessionManager>();

        // Used by unit test to actually include test managers.
        internal static SessionFactoryFilter IncludeAllWithUnitTests => new SessionFactoryFilter(includeAllByDefault: true);

        /// <summary>
        /// Initialize a <see cref="SessionFactoryFilter"/> exclude all available <see cref="ISessionManager"/>s.
        /// Use <see cref="SessionFactoryFilterExtensions.Except{TManager}(SessionFactoryFilter)"/> to add a specific <see cref="ISessionManager"/>.
        /// </summary>
        public static SessionFactoryFilter ExcludeAll => new SessionFactoryFilter(includeAllByDefault: false);

        private SessionFactoryFilter(bool includeAllByDefault)
        {
            m_IncludeAllByDefault = includeAllByDefault;
        }

        internal IEnumerable<Type> FilterManagers(IEnumerable<Type> types)
        {
            var validTypes = new HashSet<Type>();
            foreach (var type in types)
            {
                var result = FilterManager(type);
                if (!m_IncludeAllByDefault)
                {
                    result = !result;
                }

                if (result)
                {
                    validTypes.Add(type);
                }
            }

            foreach (var type in m_ExplicitIncludes)
            {
                validTypes.Add(type);
            }

            var typesReplacedByMock = new List<Type>();
            foreach (var mockMapping in m_MockMapping)
            {
                foreach (var validType in validTypes)
                {
                    if (validType == mockMapping.Value)
                        continue;

                    if (mockMapping.Key.IsAssignableFrom(validType))
                        typesReplacedByMock.Add(validType);
                }

                validTypes.Add(mockMapping.Value);
            }

            foreach (var typeToRemove in typesReplacedByMock)
            {
                validTypes.Remove(typeToRemove);
            }

            return validTypes;
        }

        internal SessionFactoryFilter AppendConstraint(Func<Type, bool> constraint)
        {
            m_Constraints.Add(constraint);
            return this;
        }

        internal SessionFactoryFilter ExplicitlyInclude<TManager>() where TManager : class, ISessionManager
        {
            m_ExplicitIncludes.Add(typeof(TManager));
            return this;
        }

        internal SessionFactoryFilter SetupMock<TManagerFrom, TManagerTo>()
            where TManagerFrom : class, ISessionManager
            where TManagerTo : class, TManagerFrom, ISessionManager, ITestSessionManager
        {
            var destinationType = typeof(TManagerTo);
            m_MockMapping[typeof(TManagerFrom)] = destinationType;
            return this;
        }

        private bool FilterManager(Type type)
        {
            var isIncluded = true;
            foreach (var constraint in m_Constraints)
            {
                isIncluded = !constraint(type);
                if (!isIncluded)
                    break;
            }
            return isIncluded;
        }
#endif
    }
#if !NET_DOTS

    /// <summary>
    /// <see cref="SessionFactoryFilter"/> extensions methods supporting the filtering API.
    /// </summary>
    public static class SessionFactoryFilterExtensions
    {
        /// <summary>
        /// Adds an exception to the current filter for the given specific <typeparamref name="TManager"/> type.
        /// </summary>
        /// <typeparam name="TManager">Manager type specified to be included or excluded by the current filter.</typeparam>
        /// <param name="filter">The filter to add this exception to.</param>
        /// <returns>A filter to chain other exceptions onto.</returns>
        public static SessionFactoryFilter Except<TManager>(this SessionFactoryFilter filter) where TManager : class, ISessionManager, new()
            => filter.AppendConstraint(t => t == typeof(TManager));

        /// <summary>
        /// Adds an exception to the current filter for the given <typeparamref name="TBaseManager"/> base type.
        /// All managers deriving from <typeparamref name="TBaseManager"/> are going to targeted by this exception.
        /// </summary>
        /// <typeparam name="TBaseManager">Manager base type to be included or excluded by the current filter.</typeparam>
        /// <param name="filter">The filter to add this exception to.</param>
        /// <returns>A filter to chain other exceptions onto.</returns>
        public static SessionFactoryFilter ExceptAll<TBaseManager>(this SessionFactoryFilter filter)
            => filter.AppendConstraint(t => typeof(TBaseManager).IsAssignableFrom(t));

        /// <summary>
        /// Adds a custom exception implementation to the current filter.
        /// </summary>
        /// <param name="filter">The filter to add this exception to.</param>
        /// <param name="func">The implementation of the custom exception.</param>
        /// <returns>A filter to chain other exceptions onto.</returns>
        public static SessionFactoryFilter Except(this SessionFactoryFilter filter, Func<Type, bool> func)
            => filter.AppendConstraint(func);
    }
#endif
}