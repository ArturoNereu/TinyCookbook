namespace Unity.Authoring
{
    /// <summary>
    /// Base interface for all session managers. 
    /// </summary>
    public interface ISessionManager
    {
    }

    internal interface ISessionManagerInternal : ISessionManager
    {
        void Load();
        void Unload();
    }

    internal abstract class SessionManager : ISessionManagerInternal
    {
        public Session Session { get; }

        protected SessionManager(Session session)
        {
            Session = session;
        }

        public virtual void Load() {}
        public virtual void Unload() {}
    }

    internal interface IIgnoreSessionManager
    {
    }
}
