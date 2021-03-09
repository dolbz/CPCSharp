using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.IO;
using System;

const string macIconFile = "CPCSharp.Avalonia/macicon.icns";

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

var currentWorkingDir = System.IO.Directory.GetCurrentDirectory();

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

    if (!FileExists(macIconFile)) {
        // Compile all the individual icon pngs into a MacOS icns file

        var generateIconResult = StartProcess("iconutil", new ProcessSettings {
                Arguments = "-c icns macicon.iconset",
                WorkingDirectory = System.IO.Path.Combine(currentWorkingDir, "CPCSharp.Avalonia")
            });

        if (generateIconResult != 0) {
            throw new Exception("Error generating iconset");
        }
    }
});

Task("PublishMac")
    .IsDependentOn("Build")
    .Does(() => {
        //dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true -p:Configuration=Release
        DotNetCoreMSBuild(new DotNetCoreMSBuildSettings {
            WorkingDirectory = System.IO.Path.Combine(currentWorkingDir, "CPCSharp.Avalonia"),
            ArgumentCustomization = args => args
            .Append("-t:BundleApp")
            .Append("-p:RuntimeIdentifier=osx-x64")
            .Append("-p:UseAppHost=true")
            .Append("-p:Configuration=" + configuration)
            .Append("-p:Platform=MacOS")
        });
    });

Task("PublishWindows")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCorePublish("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCorePublishSettings {
            IncludeNativeLibrariesForSelfExtract=true,
            PublishTrimmed=true,
            SelfContained=true,
            PublishSingleFile=true,
            Runtime="win-x64",
            Configuration = configuration
        });
        // dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true
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
