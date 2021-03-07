using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.IO;
using System;

const string macIconFile = "CPCSharp.Avalonia/cpcsharp.icns";

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
    if (FileExists(macIconFile)) {
        DeleteFile(macIconFile);
    }
});

Task("BuildNativeMac")
    .Does(() =>
{
    EnsureDirectoryExists("NativeLibs");

    if (!FileExists("NativeLibs/libMacPSG.dylib"))
    {
        var psgBuildResult = StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj build");
        if (psgBuildResult != 0) {
            throw new Exception("Error building Mac PSG");
        }
        CopyFile("NativePSGs/Mac/MacPSG/build/Release/libMacPSG.dylib", "NativeLibs/libMacPSG.dylib");
    }

    if (!FileExists("CPCSharp.Avalonia/cpcsharp.icns")) {
        // Compile all the individual icon pngs into a MacOS icns file
       var currentWorkingDir = System.IO.Directory.GetCurrentDirectory();

        var generateIconResult = StartProcess("iconutil", new ProcessSettings {
                Arguments = "-c icns cpcsharp.iconset",
                WorkingDirectory = System.IO.Path.Combine(currentWorkingDir, "CPCSharp.Avalonia")
            });

        if (generateIconResult != 0) {
            throw new Exception("Error generating iconset");
        }
    }
});

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Build")
    .IsDependentOn("BuildNativeMac")
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
