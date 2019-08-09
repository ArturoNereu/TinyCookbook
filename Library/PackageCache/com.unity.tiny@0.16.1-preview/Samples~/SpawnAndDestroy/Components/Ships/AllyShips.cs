using Unity.Entities;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    public struct AllyShips : IBufferElementData
    {
        public SceneReference Ship;
    }
}
