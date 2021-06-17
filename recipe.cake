#load nuget:?package=Cake.Recipe&version=2.2.1

var standardNotificationMessage = "Version {0} of {1} has just been released, it will be available here https://www.nuget.org/packages/{1}, once package indexing is complete.";

Environment.SetVariableNames();

BuildParameters.SetParameters(
  context: Context,
  buildSystem: BuildSystem,
  sourceDirectoryPath: "./src",
  title: "JavaVersionSwitcher",
  masterBranchName: "main",
  repositoryOwner: "nils-org",
  shouldRunDotNetCorePack: true,
  preferredBuildProviderType: BuildProviderType.GitHubActions,
  twitterMessage: standardNotificationMessage,
  shouldRunCoveralls: false, // no tests, currently
  shouldRunCodecov: false,
  shouldRunIntegrationTests: false);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();