using System.Runtime;
var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean("CPCSharp.sln", new DotNetCoreCleanSettings
    {
        Configuration = configuration,
    });
    CleanDirectory($"./NativeLibs");
    StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj clean");
});

Task("BuildNative")
    .Does(() =>
{
    EnsureDirectoryExists("NativeLibs");
    StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj build");
    CopyFile("NativePSGs/Mac/MacPSG/build/Release/libMacPSG.dylib", "NativeLibs/libMacPSG.dylib");
});

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Build")
    .IsDependentOn("BuildNative")
    .Does(() =>
{
    DotNetCoreBuild("CPCSharp.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("CPCSharp.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
