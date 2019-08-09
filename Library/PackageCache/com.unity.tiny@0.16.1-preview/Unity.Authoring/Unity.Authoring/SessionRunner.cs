#if !NET_DOTS
using Unity.Authoring.Undo;
using Unity.Authoring.ChangeTracking;

namespace Unity.Authoring
{
    internal static class SessionRunner
    {
        public static void Update(Session session)
        {
            session.GetManager<IUndoManager>()?.BeginRecording();
            session.GetManager<IChangeManager>()?.Update();
            session.GetManager<IUndoManager>()?.EndRecording();
        }
    }
}
#endif
