using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Tiny.Scenes;

namespace DragAndDrop
{
    public struct ButtonReplay : IComponentData
    {
        public SceneReference SceneToLoad;
    }
}
