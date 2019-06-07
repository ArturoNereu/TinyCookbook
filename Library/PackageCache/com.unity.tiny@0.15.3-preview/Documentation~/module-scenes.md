# Scenes Module

The Scenes module provides the mechanism which developers can load/unload scenes from code, useful for any scenario where it's impractical to load all scenes at startup. 

Scenes represent a collection of entities to be loaded and unloaded together. All entities in a scene are given a `SceneGuid` shared component and a `SceneInstanceId` shared component once loaded. As such it's possible to load the same scene multiple times without issue; `SceneGuid` allows differentiating entities from different scenes (e.g. SceneA vs SceneB), and the `SceneInstanceId` allows differentiating entities from different instances for the same scene (e.g. SceneA loaded at startup vs SceneA loaded just now)

## Loading Scenes

Scenes load requests are made via `SceneService.LoadSceneAsync` by passing the `SceneReference` for a scene created in the editor. Multiple scenes can be requested for load one after the other, and the requests will be batched and handled asynchronously. `SceneService.LoadSceneAsync` returns an entity with a `SceneData` component which can be read for inspecting the state of the scene should polling for completion be required; alternatively the entity can be passed to `SceneService.GetStatus` to obtain the scene state.

The `SceneStreamingSystem` is responsible for handling all aspects of scene loading (IO request, decompression, entity instantiation, etc..) and as load requests are processed, the `SceneData` component data for the requested scene is updated accordingly. Once a scene is `SceneStatus.Loaded`, all scene entities are moved into the active world (i.e. `World.Active`) such that a scene is only seen to be loaded atomically. Should a scene fail to load the `SceneData.Status` will equal `SceneStatus.FailedToLoad` and the failed scene will not be enqueued to be retried.

## Unloading Scenes

There are two methods for scene unloading:
*    Unloading scenes per instance
*    Unloading scenes per `SceneReference`

`SceneService.UnloadSceneInstance` allows one to unload a specific instance of a scene. This function inspects all entities in the `World.Active` matching the `SceneGuid`, and `SceneInstance` of the passed in entity and destroys them. 

`SceneService.UnloadAllSceneInstances` allows one to unload all instances of a specific scene. This function inspects all entities in the `World.Active` matching the `SceneGuid`, **but not** the `SceneInstance` of the passed in entity and destroys them.

Given the approach above to unloading scenes, it is perfectly fine for a subset of entities in a scene to be destroyed by direct calls to `EntityManager.DestroyEntity` before a call to `SceneService.UnloadSceneInstance` or `SceneService.UnloadAllScenes` is made (e.g. You may load a scene of destroyable objects for the player to shoot and you later call SceneService.UnloadAllSceneInstances as part of unloading the level)