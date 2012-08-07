#I "./packages/FAKE.1.64.7/tools"
#r "FakeLib.dll"

open Fake
open System.IO

// properties
let projectName = "LAgent"
let version = if isLocalBuild then "1.0." + System.DateTime.UtcNow.ToString("yMMdd") else buildVersion
let projectSummary = "An agent framework in F#"
let projectDescription = projectSummary
let authors = ["Luca Bolognese"; "panesofglass"]
let mail = "ryan.riley@panesofglass.org"
let homepage = "http://github.com/panesofglass/LAgent"
let license = "http://github.com/panesofglass/LAgent/raw/master/LICENSE.txt"

// directories
let buildDir = "./build/"
let packagesDir = "./packages/"
let testDir = "./test/"
let deployDir = "./deploy/"
let docsDir = "./docs/"

let targetPlatformDir = getTargetPlatformDir "4.0.30319"

let nugetDir = "./nuget/"
let nugetLibDir = nugetDir @@ "lib/net40"
let nugetDocsDir = nugetDir @@ "docs"

// params
let target = getBuildParamOrDefault "target" "All"

// tools
let fakePath = "./packages/FAKE.1.64.7/tools"
let nugetPath = "./.nuget/nuget.exe"
let nunitPath = "./packages/NUnit.Runners.2.6.0.12051/tools"

// files
let appReferences =
    !+ "./src/**/*.fsproj"
        |> Scan

let testReferences =
    !+ "./tests/**/*.fsproj"
      |> Scan

let filesToZip =
    !+ (buildDir + "/**/*.*")
        -- "*.zip"
        |> Scan

// targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir; docsDir]
)

Target "BuildApp" (fun _ ->
    AssemblyInfo (fun p ->
        {p with 
            CodeLanguage = FSharp
            AssemblyVersion = version
            AssemblyTitle = projectSummary
            AssemblyDescription = projectDescription
            Guid = "95693707-bd9e-49ea-a67c-5ac802b4674b"
            OutputFileName = "./src/AssemblyInfo.fs" })

    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    MSBuildDebug testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    !+ (testDir + "/*.Tests.dll")
        |> Scan
        |> NUnit (fun p ->
            {p with
                ToolPath = nunitPath
                DisableShadowCopy = true
                OutputFile = testDir + "TestResults.xml" })
)

Target "GenerateDocumentation" (fun _ ->
    !+ (buildDir + "*.dll")
        |> Scan
        |> Docu (fun p ->
            {p with
                ToolPath = fakePath + "/docu.exe"
                TemplatesPath = "./lib/templates"
                OutputPath = docsDir })
)

Target "CopyLicense" (fun _ ->
    [ "LICENSE.txt" ] |> CopyTo buildDir
)

Target "ZipDocumentation" (fun _ ->
    !+ (docsDir + "/**/*.*")
        |> Scan
        |> Zip docsDir (deployDir + sprintf "Documentation-%s.zip" version)
)

Target "BuildNuGet" (fun _ ->
    CleanDirs [nugetDir; nugetLibDir; nugetDocsDir]

    XCopy (docsDir |> FullName) nugetDocsDir
    [ buildDir + "LAgent.dll"
      buildDir + "LAgent.pdb" ]
        |> CopyTo nugetLibDir

    NuGet (fun p -> 
        {p with               
            Authors = authors
            Project = projectName
            Description = projectDescription
            Version = version
            OutputPath = nugetDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            ToolPath = nugetPath
            Publish = hasBuildParam "nugetkey" })
        "LAgent.nuspec"

    [nugetDir + sprintf "LAgent.%s.nupkg" version]
        |> CopyTo deployDir
)

Target "Deploy" (fun _ ->
    !+ (buildDir + "/**/*.*")
        -- "*.zip"
        |> Scan
        |> Zip buildDir (deployDir + sprintf "%s-%s.zip" projectName version)
)

Target "All" DoNothing

// Build order
"Clean"
  ==> "BuildApp" <=> "CopyLicense"
  ==> "BuildNuGet"
  ==> "Deploy"

"All" <== ["Deploy"]

// Start build
Run target

