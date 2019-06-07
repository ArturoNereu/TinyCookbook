using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Scenes;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep ExportEntities => new BuildStepExportEntities();

        private class BuildStepExportEntities : IBuildStep
        {
            public string Description => "Exporting entities";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                var scenes = context.Project.GetScenes();
                if (scenes.Length == 0)
                {
                    Debug.LogWarning($"Project {context.Project.Name} contains no scenes.");
                }

                var editorSceneManager = context.Session.GetManager<IEditorSceneManagerInternal>();
                foreach (var sceneReference in scenes)
                {
                    var assetGuid = sceneReference.SceneGuid;
                    var sceneName = context.PersistenceManager.GetSceneAssetName(assetGuid);
                    context.ProgressBar.Update($"{Description} for {sceneName}");

                    using (var tmpWorld = new World(assetGuid.ToString("N")))
                    {
                        // Scene asset path must exist in asset database
                        var assetPath = context.PersistenceManager.GetSceneAssetPath(assetGuid);
                        if (string.IsNullOrEmpty(assetPath))
                        {
                            Debug.LogError($"Could not locate asset path for asset guid {assetGuid.ToString("N")}.");
                            continue;
                        }

                        var scene = editorSceneManager.GetFirstInstanceOfSceneLoaded(assetGuid);
                        if (scene != Scene.Null)
                        {
                            // Copy scene entities into temporary world
                            using (var entities = scene.ToEntityArray(context.EntityManager, Collections.Allocator.TempJob))
                            {
                                foreach (var entity in entities)
                                {
                                    CopyEntity(entity, context.World, tmpWorld);
                                }
                            }
                        }
                        else
                        {
                            // Load scene into temporary world
                            scene = context.PersistenceManager.LoadScene(tmpWorld.EntityManager, assetPath, removeRemapInfo: false);
                            if (scene == Scene.Null)
                            {
                                Debug.LogError($"Failed to load scene at path '{assetPath}'.");
                                continue;
                            }
                        }

                        // Export scene
                        var outputFile = context.DataDirectory.GetFile(tmpWorld.Name);
                        if (!ExportWorld(outputFile, context.Project, assetPath, tmpWorld))
                        {
                            return false;
                        }

                        // Update manifest
                        context.Manifest.Add(assetGuid, assetPath, outputFile.AsEnumerable());
                    }
                }

                return true;
            }
        }
    }
}
