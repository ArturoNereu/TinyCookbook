using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Editor.Extensions;
using Unity.Tiny.Scenes;
using UnityEngine.TestTools;

namespace Unity.Editor.Tests
{
    // Creating a project triggers a domain reload, as well as deleting it. For this reason,
    // put the CreateProject test into its own class rather than with all other project tests.
    internal class ProjectCreateTest
    {
        [Ignore("Still unable to have EditMode test that triggers a domain reload")]
        [UnityTest]
        public IEnumerator CreateProject()
        {
            using (var project = Project.Create(Application.TestDirectory, "Test Project"))
            {
                // WaitForDomainReload
                {
                    UnityEditor.EditorApplication.UnlockReloadAssemblies();
                    bool isAsync = UnityEditor.EditorApplication.isCompiling;
                    yield return null;
                    if (!isAsync)
                    {
                        UnityEditor.EditorApplication.LockReloadAssemblies();
                        throw new Exception("Expected domain reload, but it did not occur");
                    }
                    while (UnityEditor.EditorApplication.isCompiling)
                    {
                        yield return null;
                    }
                    if (UnityEditor.EditorUtility.scriptCompilationFailed)
                    {
                        UnityEditor.EditorApplication.LockReloadAssemblies();
                        throw new Exception("Script compilation failed");
                    }
                }

                Assert.IsNotNull(project);
                Assert.IsTrue(Project.Projects.Contains(project));
            }

            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Application.TestDirectory.Delete(true);

            // WaitForDomainReload
            {
                UnityEditor.EditorApplication.UnlockReloadAssemblies();
                bool isAsync = UnityEditor.EditorApplication.isCompiling;
                yield return null;
                if (!isAsync)
                {
                    UnityEditor.EditorApplication.LockReloadAssemblies();
                    throw new Exception("Expected domain reload, but it did not occur");
                }
                while (UnityEditor.EditorApplication.isCompiling)
                {
                    yield return null;
                }
                if (UnityEditor.EditorUtility.scriptCompilationFailed)
                {
                    UnityEditor.EditorApplication.LockReloadAssemblies();
                    throw new Exception("Script compilation failed");
                }
            }

            yield break;
        }
    }

    internal class ProjectTests
    {
        private static string[] GetAllProjectPaths()
        {
            return Directory.GetFiles(Application.DataDirectory.FullName, "*.project", SearchOption.AllDirectories);
        }

        private static string[] GetAnyProjectPath()
        {
            return GetAllProjectPaths().FirstOrDefault().AsArray();
        }

        [Test, TestCaseSource(nameof(GetAllProjectPaths))]
        public void OpenProject(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                return;
            }

            using (var project = Project.Open(new FileInfo(projectPath)))
            {
                Assert.IsNotNull(project);
                Assert.IsTrue(Project.Projects.Contains(project));

                var projectFile = project.GetProjectFile();
                Assert.IsNotNull(projectFile);
                Assert.IsTrue(projectFile.Exists);

                var asmdefFile = project.GetAssemblyDefinitionFile();
                Assert.IsNotNull(asmdefFile);
                Assert.IsTrue(asmdefFile.Exists);

                var configurationFile = project.GetConfigurationFile();
                Assert.IsNotNull(configurationFile);
                Assert.IsTrue(configurationFile.Exists);
            }
        }

        [Test, TestCaseSource(nameof(GetAllProjectPaths))]
        public void SaveProject(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                return;
            }

            var timeStamp = File.GetLastWriteTime(projectPath);
            using (var project = Project.Open(new FileInfo(projectPath)))
            {
                project.Save();
                Assert.AreNotEqual(timeStamp, File.GetLastWriteTime(projectPath));
            }
        }

        [Test, TestCaseSource(nameof(GetAnyProjectPath))]
        public void AddRemoveScenesAndStartupScenes(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                return;
            }

            using (var project = Project.Open(new FileInfo(projectPath)))
            {
                var sceneReference = new SceneReference { SceneGuid = Guid.NewGuid() };
                Assert.IsFalse(project.GetScenes().Contains(sceneReference));
                Assert.IsFalse(project.GetStartupScenes().Contains(sceneReference));

                project.AddScene(sceneReference);
                Assert.IsTrue(project.GetScenes().Contains(sceneReference));

                project.AddStartupScene(sceneReference);
                Assert.IsTrue(project.GetStartupScenes().Contains(sceneReference));

                project.RemoveStartupScene(sceneReference);
                Assert.IsFalse(project.GetStartupScenes().Contains(sceneReference));

                project.RemoveScene(sceneReference);
                Assert.IsFalse(project.GetScenes().Contains(sceneReference));
            }
        }
    }
}
