#addin "MagicChunks"

var target						  = Argument("target", "Default");
var configuration				  = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild				  = !AppVeyor.IsRunningOnAppVeyor;
var projects					  = new []
{
	"./src/IdentityServer4.Contrib.ServiceStack/IdentityServer4.Contrib.ServiceStack.csproj",
	"./src/ServiceStack.Authentication.IdentityServer/ServiceStack.Authentication.IdentityServer.csproj"
};
var sourcePath					  = Directory("./src");
var buildArtifacts				  = Directory("./Artifacts");

var nugetSources                  = new [] { "https://api.nuget.org/v3/index.json" };

Task("Build")
    .IsDependentOn("Clean")
	.IsDependentOn("Version")
    .IsDependentOn("Restore")
    .Does(() =>
{
	foreach(var project in projects)
	{
        var settings = new DotNetCoreBuildSettings 
        {
            Configuration = configuration
        };
        DotNetCoreBuild(project, settings); 
    }
});

Task("Version")
	.Does(() => 
{
	if (AppVeyor.IsRunningOnAppVeyor)
	{
		// TransformConfig()
		// var projects = GetFiles("./**/project.json");
		// foreach(var project in projects)
		// {
		// 	var projectJson = ParseJsonFromFile(project);

		// 	string version = (string)projectJson["version"];
		// 	projectJson["version"] = version.Substring(0, version.LastIndexOf('.')) +  "." + AppVeyor.Environment.Build.Number.ToString();

		// 	SerializeJsonToFile(project, projectJson);
		// }
	}
});

Task("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreRestoreSettings { Sources = nugetSources };
    DotNetCoreRestore(Directory("."), settings);
});

Task("Pack")
	.IsDependentOn("Build")
	.Does(() => 
{
	var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = buildArtifacts,
    };
	
	foreach(var project in projects)
	{
		DotNetCorePack(Directory(project), settings);
	}

	if (!isLocalBuild)
	{
		var artifacts = GetFiles(buildArtifacts.Path + "/*.nupkg");
		foreach(var artifact in artifacts)
		{
			AppVeyor.UploadArtifact(artifact);
		}
	}
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