using NUnit.Framework;
using System.IO;
using Unity.Editor.Build;
using Unity.Editor.Extensions;

namespace Unity.Editor.Tests
{
    internal class BuildTests
    {
        private DirectoryInfo TestDirectory;
        private Project Project;

        [SetUp]
        public void SetUp()
        {
            TestDirectory = Application.RootDirectory.Combine("Tests");
            if (TestDirectory.Exists)
            {
                TestDirectory.Delete(true);
            }
            Project = Project.Create(TestDirectory.Combine("Projects"), "Test Project Banana");
        }

        [Test]
        [Ignore("Cannot test domain reload")]
        public void BuildNewProject()
        {
            var result = BuildPipeline.Build(new BuildSettings()
            {
                Project = Project,
                Platform = new DesktopDotNetPlatform(),
                Configuration = Configuration.Debug,
                OutputDirectory = TestDirectory.Combine("DotsRuntimeBuild")
            });
            Assert.IsTrue(result.Success);
        }

        [TearDown]
        public void TearDown()
        {
            if (Project != null)
            {
                Project.Dispose();
                Project = null;
            }
        }
    }
}
