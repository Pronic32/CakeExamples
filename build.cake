#addin "nuget:?package=Cake.Sonar"
#addin "nuget:?package=Newtonsoft.Json"
#addin "nuget:?package=Cake.Git"

#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"
#tool "nuget:?package=OpenCover"

using Newtonsoft.Json;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var enableSonarAnalysis = Argument("sonar", false);
var sonarKey = Argument("sonarKey", string.Empty);
var sonarUrl = Argument("sonarUrl", "http://localhost:9000");
var sonarLogin = Argument("sonarLogin", "admin");
var sonarPassword = Argument("sonarPassword", "admin");

var outputPath = Argument("outputPath", "./artifacts");
var publishPath = outputPath + "/CakeExampleNetCore/";
var testResultsPath = outputPath + "/testResults/";
var testCoverageReportsPath = outputPath + "/testCoverageResults/";
var solutionName = "CakeExamplesNetCore.sln";

var testProjects = GetFiles("**/*Test*.csproj");
var testProjectsSerializedInJson = JsonConvert.SerializeObject(testProjects);
Information("Test projects serialized in JSON: " + testProjectsSerializedInJson);


private string GetTestResultFileName(FilePath filePath)
    => MakeAbsolute(Directory(testResultsPath + filePath.GetFilenameWithoutExtension() + ".testResults.trx")).FullPath;

private string GetCoverageResultsFileName(FilePath filePath)
    => MakeAbsolute(Directory(testCoverageReportsPath + filePath.GetFilenameWithoutExtension() + ".coverage.xml")).FullPath;

var setupOutputDirectory = Task("SetupOutputDirectory")
    .Does(() =>
    {
        EnsureDirectoryExists(outputPath);
        CleanDirectory(outputPath);

        EnsureDirectoryExists(publishPath);
        EnsureDirectoryExists(testResultsPath);
        EnsureDirectoryExists(testCoverageReportsPath);
    });

var branchInfo = Task("BranchInfo")
    .Does(() =>
    {
        var branch = GitBranchCurrent("./");
        Information($"Branch: {branch.FriendlyName}; commit {branch.Tip.Sha}");
    });

var cleanSolution = Task("CleanSolution")
    .Does(() =>
    {
        DotNetCoreClean(solutionName);
    });

var buildSolution = Task("BuildSolution")
    .IsDependentOn(cleanSolution)
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoDependencies = false,
            OutputDirectory = outputPath,
            MSBuildSettings = new DotNetCoreMSBuildSettings
            {
                DetailedSummary = true
            }
        };
        DotNetCoreBuild(solutionName, settings);
    });

var runTests = Task("RunTests")
    .DoesForEach(testProjects, project =>
    {
        Information(project);

        var openCoverSettings = new OpenCoverSettings
        {
            LogLevel = OpenCoverLogLevel.All,
            MergeOutput = true,
            SkipAutoProps = true,
            TargetDirectory = outputPath,
            OldStyle = true
        }
        .WithFilter("+[*]*")
        .WithFilter("-[*Test*]*");

        var dotnetCoreSettings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            Logger = "trx;LogFileName=" + GetTestResultFileName(project),
            ArgumentCustomization = args => args.Append($"/p:DebugType=full")
        };

        var coverageResultsFileName = new FilePath(GetCoverageResultsFileName(project));
        OpenCover(tool =>
        {
            Information(project);
            tool.DotNetCoreTest(project.FullPath, dotnetCoreSettings);
        },
        coverageResultsFileName,
        openCoverSettings);
    });

var publish = Task("Publish")
	.Does(() => {
		// self-contained parameter: https://github.com/dotnet/cli/issues/9852
		DotNetCorePublish("CakeExampleNetCore/CakeExampleNetCore.csproj",
        new DotNetCorePublishSettings
		{
			Configuration = configuration,
			OutputDirectory = publishPath,
			ArgumentCustomization = args => args.Append($"--no-restore --self-contained false")
		});
	});

var sonarBegin = Task("SonarBegin")
	.WithCriteria(enableSonarAnalysis)
	.Does(() => {
        var vsTestReportsPath = string.Join(",", testProjects.Select(x => GetTestResultFileName(x)));
        var openCoverReportsPath = string.Join(",", testProjects.Select(x => GetCoverageResultsFileName(x)));
        var sonarBeginSettings = new SonarBeginSettings
        {
            Name = "CakeExamplesNetCore",
            Key = sonarKey,
            Url = sonarUrl,
            Login = sonarLogin,
            Password = sonarPassword,
            Verbose = true,
            VsTestReportsPath = vsTestReportsPath,
            OpenCoverReportsPath = openCoverReportsPath,
            Exclusions = "CakeExampleNetCore/wwwroot/**"
        };

        SonarBegin(sonarBeginSettings);
    });

var sonarMsBuild = Task("MsBuild")
	.WithCriteria(enableSonarAnalysis)
	.Does(() => 
    {
        var msBuildSettingsWithRestore = new MSBuildSettings
        {
            Verbosity = Verbosity.Minimal,
            ToolVersion = MSBuildToolVersion.Default,
            Configuration = configuration,
            PlatformTarget = PlatformTarget.MSIL
        };
        
        msBuildSettingsWithRestore = msBuildSettingsWithRestore.WithTarget("restore");

        var msBuildSettings = new MSBuildSettings
        {
            Verbosity = Verbosity.Minimal,
            ToolVersion = MSBuildToolVersion.Default,
            Configuration = configuration,
            PlatformTarget = PlatformTarget.MSIL
        };

        // https://stackoverflow.com/questions/46773698/msbuild-15-nuget-restore-and-build
        MSBuild("./" + solutionName, msBuildSettingsWithRestore);
        MSBuild("./" + solutionName, msBuildSettings);
    });

var sonarEnd = Task("SonarEnd")
    .WithCriteria(enableSonarAnalysis)
    .Does(() => 
    {
        var sonarEndSettings = new SonarEndSettings
        {
            Login = sonarLogin,
            Password = sonarPassword
        };

        SonarEnd(sonarEndSettings);
    });

var sonarAnalysis = Task("Sonar")
    .WithCriteria(enableSonarAnalysis)
    .IsDependentOn(sonarBegin)
    .IsDependentOn(sonarMsBuild)
    .IsDependentOn(sonarEnd);

Task("Default")
    .IsDependentOn(branchInfo)
    .IsDependentOn(setupOutputDirectory)
    .IsDependentOn(buildSolution)
    .IsDependentOn(runTests)
    .IsDependentOn(publish)
    .IsDependentOn(sonarAnalysis);

RunTarget(target);