#load "../common.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", "Verbose");

var buildSpec = new BuildSpec {
    Libs = new ISolutionBuilder [] {
        new DefaultSolutionBuilder {
            AlwaysUseMSBuild = true,
            BuildsOn = BuildPlatforms.Windows | BuildPlatforms.Mac,
            SolutionPath = "./source/SkiaSharp.Extended.sln",
            Configuration = configuration,
            OutputFiles = new [] { 
                new OutputFileCopy {
                    FromFile = "./source/SkiaSharp.Extended/bin/Release/SkiaSharp.Extended.dll",
                    ToDirectory = "./output/portable"
                },
                new OutputFileCopy {
                    FromFile = "./source/SkiaSharp.Extended.NetStandard/bin/Release/SkiaSharp.Extended.dll",
                    ToDirectory = "./output/netstandard"
                },
            },
            PostBuildAction = () => {
                SignAssembly("./source/SkiaSharp.Extended/bin/Release/SkiaSharp.Extended.dll", "../keys/mono.snk");
                SignAssembly("./source/SkiaSharp.Extended.NetStandard/bin/Release/SkiaSharp.Extended.dll", "../keys/mono.snk");
            },
        },
    },

    // Samples = new ISolutionBuilder [] {
    //     new DefaultSolutionBuilder { SolutionPath = "./samples/SkiaSharpDemo.sln" },
    // },

    NuGets = new [] {
        new NuGetInfo { NuSpec = "./nuget/SkiaSharp.Extended.nuspec"},
    },
};

Task("tests")
    .IsDependentOn("libs")
    .Does(() =>
{
    // build the tests
    NuGetRestore("./tests/SkiaSharp.Extended.Tests.sln");
    MSBuild("./tests/SkiaSharp.Extended.Tests.sln", settings => settings.SetConfiguration(configuration));

    // run the tests
    NUnit3("./tests/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        Results = "./output/TestResult.xml",
        ResultFormat = "nunit2",
    });
});

Task("Default")
    .IsDependentOn("libs")
    .IsDependentOn("nuget")
    .IsDependentOn("tests");

SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget(target);
