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
        // Build universal binary directly with xcodebuild
        Information("Building libMacPSG as universal binary (x86_64 + arm64)...");

        // Clean first
        StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj clean");

        // Build for both architectures at once
        var psgBuildResult = StartProcess("xcodebuild", "-project NativePSGs/Mac/MacPSG/MacPSG.xcodeproj build -configuration Release -arch x86_64 -arch arm64 CODE_SIGN_IDENTITY=\"Developer ID Application: Nathan Randle (AJ9VCT4GE7)\" ONLY_ACTIVE_ARCH=NO");
        if (psgBuildResult != 0) {
            throw new Exception("Error building Mac PSG universal binary");
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
        // Publish for x64 (Intel)
        Information("Publishing for osx-x64 (Intel)...");
        DotNetCorePublish("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCorePublishSettings {
            Configuration = configuration,
            Runtime = "osx-x64",
            SelfContained = true,
            ArgumentCustomization = args => args
                .Append("-p:DefineConstants=MACOS")
                .Append("-p:Platform=MacOS")
                .Append($"-p:BuildVersion={EnvironmentVariable("INPUT_VERSION")}")
        });

        // Publish for ARM64 (Apple Silicon)
        Information("Publishing for osx-arm64 (Apple Silicon)...");
        DotNetCorePublish("CPCSharp.Avalonia/CPCSharp.Avalonia.csproj", new DotNetCorePublishSettings {
            Configuration = configuration,
            Runtime = "osx-arm64",
            SelfContained = true,
            ArgumentCustomization = args => args
                .Append("-p:DefineConstants=MACOS")
                .Append("-p:Platform=MacOS")
                .Append($"-p:BuildVersion={EnvironmentVariable("INPUT_VERSION")}")
        });

        // Manually create app bundle structure
        var appBundlePath = $"{BuildArtifactsPath}/CPC#.app";
        var contentsPath = $"{appBundlePath}/Contents";
        var macOSPath = $"{contentsPath}/MacOS";
        var resourcesPath = $"{contentsPath}/Resources";

        EnsureDirectoryExists(macOSPath);
        EnsureDirectoryExists(resourcesPath);

        // Copy x64 files as base (we'll merge executables later)
        Information("Copying base files from x64 build...");
        CopyDirectory("CPCSharp.Avalonia/bin/MacOS/Release/net10.0/osx-x64/publish", macOSPath);

        // Create universal binaries for executable and native dylibs
        Information("Creating universal binaries with lipo...");
        var x64Path = "CPCSharp.Avalonia/bin/MacOS/Release/net10.0/osx-x64/publish";
        var arm64Path = "CPCSharp.Avalonia/bin/MacOS/Release/net10.0/osx-arm64/publish";

        // List of files to make universal (executable + native dylibs)
        var filesToMerge = new[] {
            "CPCSharp.Avalonia",
            "libMacPSG.dylib",
            "libAvaloniaNative.dylib",
            "libHarfBuzzSharp.dylib",
            "libSkiaSharp.dylib",
            "libclrjit.dylib",
            "libcoreclr.dylib",
            "libclrgc.dylib",
            "libclrgcexp.dylib",
            "libhostfxr.dylib",
            "libhostpolicy.dylib",
            "libmscordaccore.dylib",
            "libmscordbi.dylib",
            "libSystem.Globalization.Native.dylib",
            "libSystem.IO.Compression.Native.dylib",
            "libSystem.Native.dylib",
            "libSystem.Net.Security.Native.dylib",
            "libSystem.Security.Cryptography.Native.Apple.dylib"
        };

        foreach (var file in filesToMerge) {
            var x64File = $"{x64Path}/{file}";
            var arm64File = $"{arm64Path}/{file}";
            var outputFile = $"{macOSPath}/{file}";

            if (FileExists(x64File) && FileExists(arm64File)) {
                Information($"Creating universal binary for {file}");
                var lipoResult = StartProcess("lipo", $"-create -output \"{outputFile}\" \"{x64File}\" \"{arm64File}\"");
                if (lipoResult != 0) {
                    Warning($"Failed to create universal binary for {file}, using x64 version");
                }
            }
        }

        // Copy icon to Resources
        CopyFile(macIconFile, $"{resourcesPath}/macicon.icns");

        // Create Info.plist
        var infoPlist = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
  <dict>
    <key>CFBundleName</key>
    <string>CPCSharp</string>
    <key>CFBundleDisplayName</key>
    <string>CPC#</string>
    <key>CFBundleIdentifier</key>
    <string>com.dolbz.CPCSharp</string>
    <key>CFBundleVersion</key>
    <string>{EnvironmentVariable("INPUT_VERSION") ?? "1.0.0"}</string>
    <key>CFBundlePackageType</key>
    <string>AAPL</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>CFBundleExecutable</key>
    <string>CPCSharp.Avalonia</string>
    <key>CFBundleIconFile</key>
    <string>macicon.icns</string>
    <key>CFBundleShortVersionString</key>
    <string>{EnvironmentVariable("INPUT_VERSION") ?? "1.0.0"}</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>NSHighResolutionCapable</key>
    <true />
  </dict>
</plist>";
        System.IO.File.WriteAllText($"{contentsPath}/Info.plist", infoPlist);
    });

Task("SignMac")
    .Does(() => {
        StartProcess("scripts/MacDistribution/signApp.sh", BuildArtifactsPath + "/CPC#.app");
    });

Task("NotarizeMac")
    .Does(() =>
    {
        StartProcess("scripts/MacDistribution/notarizeApp.sh", BuildArtifactsPath + "/CPC#.app");
    });

Task("PublishMac")
    .IsDependentOn("BundleMac")
    .IsDependentOn("SignMac");

Task("ExternalPublishMac")
    .IsDependentOn("PublishMac")
    .IsDependentOn("NotarizeMac");

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
              .Append($"-p:DefineConstants={DefineConstants}")
              .Append($"-p:BuildVersion={EnvironmentVariable("INPUT_VERSION")}")
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
        .Append($"-p:BuildVersion={EnvironmentVariable("INPUT_VERSION")}")
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
