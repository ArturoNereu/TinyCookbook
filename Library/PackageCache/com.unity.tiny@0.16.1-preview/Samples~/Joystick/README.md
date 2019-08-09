# Virtual Joystick
This sample shows how to create a virtual UI joystick and use it to control character movements. The character can also be moved using the arrow keys or WASD. The project also contains a simple camera follow system.

### Date and version
v1, 5/31/2019

### Related API documentation
* [EntityManager](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityManager.html)
* [EntityQueryBuilding.WithAll](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_WithAll__1)
* [EntityQueryBuilding.ForEach](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_ForEach_Unity_Entities_EntityQueryBuilder_F_E_)
* [World.GetExistingSystem](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.World.html#Unity_Entities_World_GetExistingSystem_System_Type_)

### Prerequisite samples	
* [HelloWorld](../HelloWorld)

### Samples extending this sample
none

### Script Descriptions
* [VirtualJoystickSystem](Scripts/VirtualJoystickSystem.cs): Captures the input from a virtual Joystick. To use, add the [Joystick](Components/Joystick.cs) component along with a `RectTransform` on the entity that has the joystick sprite.
* [CharacterMovementSystem](Scripts/CharacterMovementSystem.cs): Control the [Character](Components/Character.cs) movement based on the current [Joystick](Components/Joystick.cs) input direction.
*  [CameraFollowSystem](Scripts/CameraFollowSystem.cs): Follows the target entity if it's outside of the safe rectangle at the center of the screen.

### Feedback
We are always looking to understand how we can improve our samples. If you are able, we would love to get your feedback: https://unitysoftware.co1.qualtrics.com/jfe/form/SV_3BOKADHkJsMdJrL


