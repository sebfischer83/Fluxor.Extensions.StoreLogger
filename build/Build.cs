using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitReleaseManager.GitReleaseManagerTasks;
using System.IO;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    const string MasterBranch = "master";
    const string DevelopBranch = "develop";
    const string ReleaseBranchPrefix = "release";
    const string HotfixBranchPrefix = "hotfix";

    [Parameter] readonly string ApiKey;
    [Parameter("NuGet Source for Packages")] readonly string Source = "https://api.nuget.org/v3/index.json";
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    [CI] readonly GitHubActions GitHubActions;
    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PackageDirectory => ArtifactsDirectory / "packages";

    AbsolutePath SiteDirectory => ArtifactsDirectory / "site";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });
    Target Pack => _ => _
     .DependsOn(Compile)
     .Produces(PackageDirectory / "*.nupkg")
     .Executes(() =>
     {
         DotNetPack(_ => _
             .SetProject(Solution.GetProject("Fluxor.Extensions.StoreLogger"))
             .SetNoBuild(InvokedTargets.Contains(Compile))
             .SetConfiguration(Configuration)
             .SetOutputDirectory(PackageDirectory)
             .SetVersion(GitVersion.NuGetVersionV2)
             .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangelogFile, GitRepository)));
     });

    //Target Release => _ => _
    // .DependsOn(Pack)
    // .Executes(() =>
    // {
    //     GitReleaseManagerCreate(_ => _
    //     .)
    // });

     Target BuildDemoPage => _ => _
     .DependsOn(Compile)
     .Executes(() =>
     {
         DotNetPublish(_ => _
         .SetProject(Solution.GetProject("Fluxor.Extensions.StoreLogger.Demo"))
         .SetNoBuild(InvokedTargets.Contains(Compile))
         .SetConfiguration(Configuration)
         .SetVersion(GitVersion.NuGetVersionV2)
         .SetOutput(SiteDirectory)
         );
     });

    Target Publish => _ => _
     .Unlisted()
     .ProceedAfterFailure()
     .DependsOn(Clean, Pack)
     .Requires(() => GitHasCleanWorkingCopy())
     .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch())
     .Executes(() =>
     {
         var packages = PackageDirectory.GlobFiles("*.nupkg");
         DotNetNuGetPush(_ => _
                 .SetSource(Source)
                 .SetApiKey(ApiKey)
                 .CombineWith(packages, (_, v) => _
                     .SetTargetPath(v)),
             degreeOfParallelism: 5,
             completeOnFailure: false);
         FinalizeChangelog(ChangelogFile, GitVersion.MajorMinorPatch, GitRepository);

         Git($"add {ChangelogFile}");
         Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.MajorMinorPatch}\"");

         Git($"checkout {MasterBranch}");
         Git($"merge --no-ff --no-edit {GitRepository.Branch}");
         Git($"tag {GitVersion.MajorMinorPatch}");

         Git($"checkout {DevelopBranch}");
         Git($"merge --no-ff --no-edit {GitRepository.Branch}");

         Git($"branch -D {GitRepository.Branch}");

         Git($"push origin {MasterBranch} {DevelopBranch} {GitVersion.MajorMinorPatch}");
     });

    public static bool GitHasCleanWorkingCopy()
    {
        return GitHasCleanWorkingCopy(null);
    }

    public static bool GitHasCleanWorkingCopy(string? workingDirectory)
    {
        return !Git("status --short", workingDirectory, logOutput: false).Any();
    }
}
