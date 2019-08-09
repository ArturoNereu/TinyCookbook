using Unity.Entities;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    public struct EnemyShips : IBufferElementData
    {
        public SceneReference Ship;
    }
}
