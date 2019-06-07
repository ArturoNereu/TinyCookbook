# Sample Project: UILayout, Button and Countdown Timer

This sample project shows how to setup a UILayout, do actions on the press of a button and handle a countdown timer.

[StartTimerButtonSystem](Scripts/StartTimerButtonSystem.cs): Starts a timer on the press of a button. To use, add the StartTimerButton component to any entity that has a RectTransform.

[TimerSystem](Scripts/TimerSystem.cs): Update a countdown timer, display it in a label and perform actions when it reaches zero. To use, add the Timer component to an entity that has a Text2DRenderer component and a TextString component.
