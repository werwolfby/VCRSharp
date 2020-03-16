using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Specify to not open generated report")]
    readonly bool NotOpenReport = false;

    [Solution] readonly Solution Solution;
    Project UnitTestsProject => Solution.GetProject("VCRSharp.Tests");
    
    Project IntegrationTestsProject => Solution.GetProject("VCRSharp.IntegrationTests");

    AbsolutePath ReportsDirectory => RootDirectory / "Reports";
    
    List<AbsolutePath> CoverageReports { get; } = new List<AbsolutePath>();

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            foreach (var buildDir in GlobDirectories(RootDirectory, "**/bin")
                .Concat(GlobDirectories(RootDirectory, "**/obj"))
                .Concat(GlobDirectories(RootDirectory, "*Tests/TestResults"))
                .Concat(ReportsDirectory)
                .Where(d => !d.Contains("build")))
            {
                EnsureCleanDirectory(buildDir);
            }
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
                .SetNoRestore(true));
        });

    Target Tests => _ => _
        .Triggers(UnitTests)
        .Triggers(IntegrationTests);

    Target UnitTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var outputs = DotNetTest(s => s
                .SetProjectFile(UnitTestsProject.Path)
                .SetDataCollector("XPlat Code Coverage"));
            CoverageReports.Add(GetAttachment(outputs));
        });

    Target IntegrationTests => _ => _
        .After(UnitTests)
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(IntegrationTestsProject.Path));
        });

    Target CoverageReport => _ => _
        .After(UnitTests)
        .After(IntegrationTests)
        .Executes(() =>
        {
            ControlFlow.Assert(CoverageReports.Count > 0,
                "CoverageReport task has to be executed with *Tests target's only");

            ReportGenerator(s => s
                .SetFramework("netcoreapp3.0")
                .SetReports(CoverageReports.Select(s => (string)s))
                .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines)
                .SetTargetDirectory(ReportsDirectory));
            
            if (NotOpenReport) return;

            if (IsWin)
            {
                Process.Start("cmd.exe", @"/c " + ReportsDirectory / "index.htm");
            }
        });

    AbsolutePath GetAttachment(IEnumerable<Output> outputs)
    {
        var attachment = outputs
            .Where(o => o.Type == OutputType.Std)
            .SkipWhile(o => !o.Text.StartsWith("Attachments:"))
            .Skip(1)
            .Cast<Output?>()
            .FirstOrDefault();
        ControlFlow.Assert(attachment != null, "Can't find test result");
        return (AbsolutePath) attachment.Value.Text.Trim();
    }
}
