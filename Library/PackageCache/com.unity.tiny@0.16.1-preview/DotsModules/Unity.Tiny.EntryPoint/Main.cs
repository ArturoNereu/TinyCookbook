using System;
using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;
using Unity.Tiny.Debugging;
using System.Collections.Generic;

namespace Unity.Tiny.EntryPoint
{
    public static class Program
    {
        enum BootPhase
        {
            Booting = 0,
            LoadingConfig,
            Running
        }

        private static World m_World;
        private static TinyEnvironment m_Environment;
        private static BootPhase m_BootPhase;
        private static Entity m_ConfigScene;
        private static Entity m_AssetsScene;

        // NOTE: The boot up flow is inside the MainLoop as we want to be able to load configuration files
        // before executing any systems, however for web builds we must do such loading inside the mainloop to ensure
        // the browser doesn't hang while we wait on async work to complete
        private static void Main()
        {
            // Create main world
            m_World = DefaultTinyWorldInitialization.InitializeWorld("main");
            m_Environment = m_World.GetOrCreateSystem<TinyEnvironment>();
            m_BootPhase = BootPhase.Booting;

            // Setup systems
            DefaultTinyWorldInitialization.InitializeSystems(m_World);

            // Run program
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
            var windowSystem = m_World.GetExistingSystem<WindowSystem>();
            if (windowSystem != null)
            {
                windowSystem.InfiniteMainLoop(MainLoop);
            }
            else
            {
                Debug.Log("No window system found.");
            }

            // Free world and all systems/managers in it
            m_World.Dispose();
        }

        private static bool MainLoop()
        {
            if(m_BootPhase == BootPhase.Running)
            {
                m_World.Update();
            }
            else
            {
                var em = m_World.EntityManager;
                var sceneStreamingSystem = m_World.GetExistingSystem<SceneStreamingSystem>();

                switch (m_BootPhase)
                {
                    case BootPhase.Booting:
                        {
                            // Destroy current config entity
                            if (em.Exists(m_Environment.configEntity))
                            {
                                em.DestroyEntity(m_Environment.configEntity);
                                m_Environment.configEntity = Entity.Null;
                            }

                            m_ConfigScene = SceneService.LoadConfigAsync();
                            m_AssetsScene = SceneService.LoadAssetsAsync();

                            m_BootPhase = BootPhase.LoadingConfig;
                            break;
                        }
                    case BootPhase.LoadingConfig:
                        {
                            // Tick this world specifically to ensure our load requests are handled
                            sceneStreamingSystem.Update();

                            var configStatus = SceneService.GetSceneStatus(m_ConfigScene);
                            if (m_Environment.configEntity == Entity.Null && configStatus == SceneStatus.Loaded)
                            {
                                using (var configurationQuery = em.CreateEntityQuery(typeof(ConfigurationTag)))
                                {
                                    if (configurationQuery.CalculateLength() == 0)
                                    {
                                        throw new Exception($"Failed to load boot configuration scene.");
                                    }

                                    using (var configEntityList = configurationQuery.ToEntityArray(Allocator.Temp))
                                    {
                                        // Set new config entity
                                        if (configEntityList.Length > 1)
                                        {
                                            throw new Exception($"More than one configuration entity found in boot configuration scene.");
                                        }
                                        m_Environment.configEntity = configEntityList[0];
                                    }
                                }
                            }
                            else if (configStatus == SceneStatus.FailedToLoad)
                            {
                                throw new Exception($"Failed to load the boot configuration scene.");
                            }

                            var assetStatus = SceneService.GetSceneStatus(m_AssetsScene);
                            // Note, failing to load the asset scene since this is acceptable e.g. there may not be an asset scene
                            if (assetStatus == SceneStatus.Loaded || assetStatus == SceneStatus.FailedToLoad)
                            {
                                LoadStartupScenes();
                                m_BootPhase = BootPhase.Running;
                            }

                            break;
                        }
                    default:
                        throw new Exception("Invalid BootPhase specified");
                }
            }

            return !m_World.QuitUpdate;
        }

        private static void LoadStartupScenes()
        {
            using (var startupScenes = m_Environment.GetConfigBufferData<StartupScenes>().ToNativeArray(Allocator.Temp))
            {
                var em = m_World.EntityManager;
                for (var i = 0; i < startupScenes.Length; ++i)
                {
                    SceneService.LoadSceneAsync(startupScenes[i].SceneReference);
                }
            }
        }
    }
}
