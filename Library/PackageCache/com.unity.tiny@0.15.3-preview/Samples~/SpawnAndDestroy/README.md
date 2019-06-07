# Sample Project: Spawn and Destroy Scenes

This project shows how to load, initialize and unload scenes.

## Loading a Scene

On start, the game will load the startup scenes. To instantiate more scenes on demand, use `SceneService.LoadSceneAsync()` and pass the `SceneReference`. `SceneReference` can be used as a field inside a Component. You can then drag your scene into that field in the Editor.

[SpawnShipSystem](Scripts/Ships/SpawnShipSystem.cs): Spawn ships when user presses a button that has the [ButtonSpawnShips](Components/Ships/ButtonSpawnShips.cs) component. This system takes a random SceneReference found on the Configuration entity and spawns a ship.

[SpawnBulletSystem](Scripts/Bullets/SpawnBulletSystem.cs): Update the attack countdown timer of every entity with the [Ship](Components/Ships/Ship.cs) component. When the timer reaches zero, spawn a bullet and reset the attack timer.

## Initializing a Scene

`SceneService.LoadSceneAsync()` loads scenes asynchronously. If manipulations are needed to initialize a scene after loading it, it can be done in a different system, after the scene has been loaded. You can use a boolean field or a tag components to identify components that have already been initialized.

[InitShipSystem](Scripts/Ships/InitShipSystem.cs): Init ships after they have been loaded. Set their start position at the edge of the screen and set a random destination.

[InitBulletSystem](Scripts/Bullets/InitBulletSystem.cs): When a bullet has been loaded (scene loading is async), find a [Ship](Components/Ships/Ship.cs) that wants to spawn a bullet (`SpawningBullet` is `true`) and set the bullet position at that ship's position.

## Unloading a Scene

`SceneService.LoadSceneAsync()` returns a scene entity. This can be cached inside a component and be used to unload a specific scene instance using `SceneService.UnloadSceneInstance()`. To unload all instances of a scene, use `SceneService.UnloadAllSceneInstances()`. Scenes that contain a single entity can also be destroyed using `PostUpdateCommands.DestroyEntity()` from inside an `Entities.ForEach`.

[DestroyAllShipsSystem](Scripts/Ships/DestroyAllShipsSystem.cs): On the press of a UI button, unloads all scene instances of every [Ship scenes](Scenes/Ships). 

[DestroyAfterDelaySystem](Scripts/DestroyAfterDelaySystem.cs): Entities that have the [DestroyAfterDelay](Components/Bullets/DestroyAfterDelay.cs) component will be destroyed after a delay (in seconds). This is used to destroy bullets 2 seconds after they are spawned.
