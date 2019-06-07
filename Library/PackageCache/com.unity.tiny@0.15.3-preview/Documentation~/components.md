# Components

There are a number of built-in component types that are provided for you to use when building your Tiny Mode projects. These are [similar but different to classic Unity Components](intro-for-unity-developers.md).

In Tiny Mode, components adhere to the [Entity-Component-System](introduction-to-ecs) (ECS) pattern, and as such they serve to _store data only_ for a particular aspect of an [Entity](entities.md). They do not provide functionality.

Many Tiny Mode modules such as [Particles](module-particles.md) and [Audio](module-audio.md) provide [built-in components](built-in-components.md) which allow you to make use of their features.

You can also create your own [custom components](scripting-components.md) which allows you to define your own sets of data that you can attach to Entities.

Note: In Tiny Mode, the editor does not have a Components menu in the main menu. To add a component, you must select an Entity in the Hierarchy, use the **Add Component** button in the inspector.