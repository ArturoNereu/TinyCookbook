# Drag and Drop
This puzzle game aims to show how to use inputs and make a drag and drop functionality. To drag the pieces, players can use the mouse, touch or a virtual cursor controlled by the arrow keys and the Space key. On touch screens, multitouch is supported to drag multiple puzzle pieces at the same time.

### Date and version
v1, 5/31/2019

### Related API documentation
* [EntityQueryBuilding.WithAll](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_WithAll__1)
* [EntityQueryBuilding.ForEach](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_ForEach_Unity_Entities_EntityQueryBuilder_F_E_)
* [World.GetExistingSystem](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.World.html#Unity_Entities_World_GetExistingSystem_System_Type_)

### Prerequisite samples	
* [HelloWorld](../HelloWorld)

### Samples extending this sample
none

### Script Descriptions
* [MouseDragSystem](Scripts/MouseDragSystem.cs): Use the mouse to drag and drop any entity that has the [Draggable](Components/Draggable.cs) component.
* [MultiTouchDragSystem](Scripts/MultiTouchDragSystem.cs): Use 1 or more fingers to drag and drop any entity that has the [Draggable](Components/Draggable.cs) component.
* [VirtualCursorDragSystem](Scripts/VirtualCursorDragSystem.cs): Use the arrow keys and the Space key to drag and drop the entity selected by the virtual cursor. Add the SelectionCursor component to the virtual cursor entity and the [Draggable](Components/Draggable.cs) component to objects that can be dragged.

### Feedback
We are always looking to understand how we can improve our samples. If you are able we would love to get your feedback: https://unitysoftware.co1.qualtrics.com/jfe/form/SV_3BOKADHkJsMdJrL