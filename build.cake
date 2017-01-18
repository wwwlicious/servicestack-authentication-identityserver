#addin "Cake.Json"

var target						  = Argument("target", "Default");
var configuration				  = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild				  = !AppVeyor.IsRunningOnAppVeyor;
var dotNetCorePackPath            = Directory("./src/IdentityServer4.Contrib.ServiceStack");
var sourcePath					  = Directory("./src");

var nugetSources                  = new [] { "https://api.nuget.org/v3/index.json" };

Task("Build")
    .IsDependentOn("Clean")
	.IsDependentOn("Version")
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

Task("Version")
	.Does(() => 
{
	if (!isLocalBuild)
	{
		var projects = GetFiles("./**/project.json");
		foreach(var project in projects)
		{
			var projectJson = ParseJsonFromFile(project);

			string version = (string)projectJson["version"];
			projectJson["version"] = version.Substring(0, version.LastIndexOf('.')) +  "." + AppVeyor.Environment.Build.Number.ToString();

			SerializeJsonToFile(project, projectJson);
		}
	}
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
  .IsDependentOn("Build");

RunTarget(target);