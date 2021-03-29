using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.IO;
using System;

const string macIconFile = "CPCSharp.Avalonia/macicon.icns";
const string BuildArtifactsPath = "BuildArtifacts";

string DefineConstants = "";

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

// Used to invoke the appropriate native build. Add others as native components are created
// Each native task determines internally whether it needs to do anything for the current platform
Task("BuildNative")
    .IsDependentOn("BuildNativeMac");

Task("BuildNativeMac")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    EnsureDirectoryExists("NativeLibs");

    if (!FileExists("NativeLibs/libMacPSG.dylib"))
    {
        var psgBuildResult = StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj build -configuration Release CODE_SIGN_IDENTITY=\"Developer ID Application: Nathan Randle (AJ9VCT4GE7)\"");
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

Task("BundleMac")
    .IsDependentOn("Build")
    .Does(() => {
        //dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true -p:Configuration=Release
        DotNetCoreMSBuild(new DotNetCoreMSBuildSettings {
            WorkingDirectory = System.IO.Path.Combine(currentWorkingDir, "CPCSharp.Avalonia"),
            ArgumentCustomization = args => args
            .Append("-t:BundleApp")
            .Append("-p:UseAppHost=true")
            .Append("-p:Configuration=" + configuration)
            .Append("-p:Platform=MacOS")
            .Append("-p:DefineConstants=MACOS")
        });
        EnsureDirectoryExists(BuildArtifactsPath);
        CopyDirectory("CPCSharp.Avalonia/bin/MacOS/Release/net5.0/publish/CPC#.app", BuildArtifactsPath + "/CPC#.app");
    });

Task("SignMac")
    .Does(() => {
        StartProcess("scripts/MacDistribution/signApp.sh", BuildArtifactsPath + "/CPC#.app");
    });

Task("PublishMac")
    .IsDependentOn("BundleMac")
    .IsDependentOn("SignMac");

Task("PublishWindows")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCorePublish("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCorePublishSettings {
            IncludeNativeLibrariesForSelfExtract=true,
            PublishTrimmed=true,
            SelfContained=true,
            PublishSingleFile=true,
            Runtime="win-x64",
            Configuration = configuration,
            ArgumentCustomization = args => args
              .Append("-p:DefineConstants=" + DefineConstants)
        });
        // dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true
    });

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("ResolveBuildConstants")
    .Does(() => {
        if (IsRunningOnMacOs()) {
            DefineConstants = "MACOS";
        }
        if (IsRunningOnWindows()) {
            DefineConstants = "WINDOWS";
        }
        Information("Defined build constants: {0}", DefineConstants);
    });

Task("Build")
    .IsDependentOn("ResolveBuildConstants")
    .IsDependentOn("BuildNative")
    .Does(() =>
{
    DotNetCoreRestore("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCoreRestoreSettings {
        ArgumentCustomization = args => args
        .Append("-p:DefineConstants=" + DefineConstants)
    });
    DotNetCoreBuild("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCoreBuildSettings
    {
        NoRestore=true,
        Configuration = configuration,
        ArgumentCustomization = args => args
        .Append("-p:DefineConstants=" + DefineConstants)
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
