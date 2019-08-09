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
        void Load(Session session);
        void Unload(Session session);
    }

    internal interface IIgnoreSessionManager
    {
    }

    internal interface ITestSessionManager
    {
    }
}
