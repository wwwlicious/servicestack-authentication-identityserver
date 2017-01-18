var target						  = Argument("target", "Default");
var configuration				  = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild				  = !AppVeyor.IsRunningOnAppVeyor;
var dotNetCorePackPath            = Directory("./src/IdentityServer4.Contrib.ServiceStack");
var sourcePath					  = Directory("./src");
var buildArtifacts				  = Directory("./artifacts/packages");

var nugetSources                  = new [] { "https://api.nuget.org/v3/index.json" };

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
	var projects = GetFiles("./**/project.json");

	foreach(var project in projects)
	{
        var settings = new DotNetCoreBuildSettings 
        {
            Configuration = configuration
        };
        DotNetCoreBuild(project.GetDirectory().FullPath, settings); 
    }
});

Task("Pack")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = buildArtifacts,
    };

    // add build suffix for CI builds
    if(!isLocalBuild)
    {
        settings.VersionSuffix = "build" + AppVeyor.Environment.Build.Number.ToString().PadLeft(5,'0');
    }
    DotNetCorePack(dotNetCorePackPath, settings);
});

Task("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreRestoreSettings { Sources = nugetSources };
    DotNetCoreRestore(sourcePath, settings);
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildArtifacts });
});

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Pack");

RunTarget(target);