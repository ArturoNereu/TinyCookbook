# UILayout, Button and Countdown Timer
This sample project shows how to setup a UILayout, do actions on the press of a button and handle a countdown timer.

### Date and version
v1, 5/31/2019

### Related API documentation
* [SetComponentData](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityManager.html#Unity_Entities_EntityManager_SetComponentData__1_Unity_Entities_Entity___0_)
* [EntityQueryBuilding.WithAll](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_WithAll__1)
* [EntityQueryBuilding.ForEach](https://docs.unity3d.com/Packages/com.unity.entities@0.0/api/Unity.Entities.EntityQueryBuilder.html#Unity_Entities_EntityQueryBuilder_ForEach_Unity_Entities_EntityQueryBuilder_F_E_)

### Prerequisite samples	
* [HelloWorld](../HelloWorld)

### Samples extending this sample
none

### Script Descriptions
* [StartTimerButtonSystem](Scripts/StartTimerButtonSystem.cs): Starts a timer on the press of a button. To use, add the StartTimerButton component to any entity that has a RectTransform.
* [TimerSystem](Scripts/TimerSystem.cs): Update a countdown timer, display it in a label and perform actions when it reaches zero. To use, add the Timer component to an entity that has a Text2DRenderer component and a TextString component.

### Feedback
We are always looking to understand how we can improve our samples. If you are able, we would love to get your feedback: https://unitysoftware.co1.qualtrics.com/jfe/form/SV_3BOKADHkJsMdJrL