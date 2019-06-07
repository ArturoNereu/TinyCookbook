using System;
using UnityEngine;

namespace Unity.Editor
{
    /// <summary>
    /// Temporary link between a <see cref="GameObject"/> and an <see cref="Entity"/>.
    /// </summary>
    [ExecuteInEditMode]
    public class EntityReference : MonoBehaviour
    {
        public delegate void OnDestroyedHandler(Guid guid);

        public Guid Guid;
        
        internal event OnDestroyedHandler OnDestroyed;

        private void OnDestroy()
        {
            var onDestroyed = OnDestroyed;
            onDestroyed?.Invoke(Guid);
        }
    }
}
