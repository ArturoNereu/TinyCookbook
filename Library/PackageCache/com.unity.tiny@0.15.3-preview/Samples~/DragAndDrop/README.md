# Sample Project: Drag and Drop

This puzzle game aims to show how to use inputs and make a drag and drop functionality. To drag the pieces, players can use the mouse, touch or a virtual cursor controlled by the arrow keys and the Space key. On touch screens, multitouch is supported to drag multiple puzzle pieces at the same time.

[MouseDragSystem](Scripts/MouseDragSystem.cs): Use the mouse to drag and drop any entity that has the [Draggable](Components/Draggable.cs) component.

[MultiTouchDragSystem](Scripts/MultiTouchDragSystem.cs): Use 1 or more fingers to drag and drop any entity that has the [Draggable](Components/Draggable.cs) component.

[VirtualCursorDragSystem](Scripts/VirtualCursorDragSystem.cs): Use the arrow keys and the Space key to drag and drop the entity selected by the virtual cursor. Add the SelectionCursor component to the virtual cursor entity and the [Draggable](Components/Draggable.cs) component to objects that can be dragged.
