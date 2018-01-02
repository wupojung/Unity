using System.Threading;
using FluentAssertions;
using GitHub.Unity;
using NSubstitute;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    class PortableGitInstallTaskTests : BaseTaskManagerTest
    {
        [Test]
        public void GitInstallTest()
        {
            InitializeTaskManager();

            var cacheContainer = Substitute.For<ICacheContainer>();
            Environment = new IntegrationTestEnvironment(cacheContainer, TestBasePath, SolutionDirectory, enableTrace: true);

            var gitInstallationPath = TestBasePath.Combine("GitInstall").CreateDirectory();

            var zipArchivesPath = TestBasePath.Combine("ZipArchives").CreateDirectory();
            var gitArchivePath = AssemblyResources.ToFile(ResourceType.Platform, "git.zip", zipArchivesPath, Environment);
            var gitLfsArchivePath = AssemblyResources.ToFile(ResourceType.Platform, "git-lfs.zip", zipArchivesPath, Environment);

            var gitInstallDetails = new PortableGitInstallDetails(gitInstallationPath, DefaultEnvironment.OnWindows);
            var gitInstallTask = new PortableGitInstallTask(CancellationToken.None, Environment, gitArchivePath, gitLfsArchivePath, gitInstallDetails);

            gitInstallTask.Start().Wait();

            Environment.FileSystem.CalculateFolderMD5(gitInstallDetails.GitInstallPath).Should().Be(PortableGitInstallDetails.ExtractedMD5);
            Environment.FileSystem.CalculateFolderMD5(gitInstallDetails.GitInstallPath, false).Should().Be(PortableGitInstallDetails.FileListMD5);

            new PortableGitInstallTask(CancellationToken.None, Environment, gitArchivePath, gitLfsArchivePath, gitInstallDetails)
                .Then(new PortableGitInstallTask(CancellationToken.None, Environment, gitArchivePath, gitLfsArchivePath, gitInstallDetails))
                .Start()
                .Wait();
        }
    }
}