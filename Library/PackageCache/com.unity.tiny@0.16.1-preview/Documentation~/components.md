# Components

There are a number of built-in component types that are provided for you to use when building your DOTS Mode projects. These are [similar but different to classic Unity Components](intro-for-unity-developers.html).

In DOTS Mode, components adhere to the [Entity-Component-System](introduction-to-ecs) (ECS) pattern, and as such they serve to *store data only* for a particular aspect of an [Entity](entities.html). They do not provide functionality.

Many DOTS Mode modules such as [Particles](module-particles.html) and [Audio](module-audio.html) provide [built-in components](built-in-components.html) which allow you to make use of their features.

You can also create your own [custom components](scripting-components.html) which allows you to define your own sets of data that you can attach to Entities.

Note: In DOTS Mode, the editor does not have a Components menu in the main menu. To add a component, you must select an Entity in the Hierarchy, use the **Add Component** button in the inspector.