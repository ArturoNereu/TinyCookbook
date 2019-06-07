#Components

Components in Tiny Mode act as data containers only.
To create Components in Tiny Mode, you must create a C# script which implements the `IComponentData` interface.

The example below shows a component which 

```
using Unity.Entities;

namespace game
{
    public struct ThingComponent : IComponentData
    {
        public float radius;
        public float time;
        public float speed;

        public static ThingComponent Default
        {
            get
            {
                var thing = new ThingComponent
                {
                    radius = 1,
                    time = 1,
                    speed = 1
                };

                return thing;
            }
        }
    }
}
```