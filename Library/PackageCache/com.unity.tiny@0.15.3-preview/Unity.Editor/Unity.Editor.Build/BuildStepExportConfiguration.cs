using System;
using System.Linq;
using Unity.Authoring;
using Unity.Editor.Assets;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Scenes;

using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep ExportConfiguration => new BuildStepExportConfiguration();

        private class BuildStepExportConfiguration : IBuildStep
        {
            public string Description => "Exporting configuration";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                using (var tmpWorld = new World(ConfigurationScene.Guid.ToString("N")))
                {
                    var configEntity = CopyEntity(context.WorldManager.GetConfigEntity(), context.World, tmpWorld);

                    // Insert asset scene before all other startup scenes, if there's any asset
                    if (AssetEnumerator.GetAllReferencedAssets(context.Project).Count > 0)
                    {
                        Assert.IsTrue(tmpWorld.EntityManager.HasComponent<StartupScenes>(configEntity));
                        var startupScenes = tmpWorld.EntityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<Guid>();
                        if (startupScenes.Length == 0)
                        {
                            Debug.LogWarning($"Project {context.Project.Name} contains no startup scenes.");
                        }
                        startupScenes.Insert(0, AssetsScene.Guid);
                    }

                    // Make sure components not owned by the users are removed if their assemblies are missing
                    var configArchetype = context.Session.GetManager<IArchetypeManager>().Config;
                    var componentsToRemove = GetAllComponentTypes(configArchetype).Where(t => !DomainCache.IsIncludedInProject(context.Project, t.GetManagedType()));
                    tmpWorld.EntityManager.RemoveComponent(tmpWorld.EntityManager.UniversalQuery, new ComponentTypes(componentsToRemove.ToArray()));

                    // Export configuration scene
                    var outputFile = context.DataDirectory.GetFile(tmpWorld.Name);
                    if (!ExportWorld(outputFile, context.Project, ConfigurationScene.Path, tmpWorld))
                    {
                        return false;
                    }

                    // Update manifest
                    context.Manifest.Add(ConfigurationScene.Guid, ConfigurationScene.Path, outputFile.AsEnumerable());

                    // Dump debug file
                    var debugFile = context.DataDirectory.GetFile(".debug.txt");
                    var debugLines = context.Manifest.Assets.OrderBy(x => x.Value).Select(x => $"{x.Key.ToString("N")} = {x.Value.DoubleQuoted()}");
                    debugFile.WriteAllLines(debugLines.ToArray());
                }

                return true;
            }
        }
    }
}
