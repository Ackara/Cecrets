# SYNOPSIS: This is a psake task file.
FormatTaskName "$([string]::Concat([System.Linq.Enumerable]::Repeat('-', 70)))`r`n  {0}`r`n$([string]::Concat([System.Linq.Enumerable]::Repeat('-', 70)))";

Properties {
	$Dependencies = @(@{"Name"="Ncrement";"Version"="8.2.18"});

    # Arguments
	$Major = $false;
	$Minor = $false;
	$Filter = $null;
	$InPreview = $false;
	$Interactive = $true;
	$InProduction = $false;
	$Configuration = "Debug";
	$EnvironmentName = $null;

	# Files & Folders
	$MSBuildExe = "";
	$ToolsFolder = "";
	$SecretsFilePath = "";
	$SolutionFolder = (Split-Path $PSScriptRoot -Parent);
	$SolutionName =   (Split-Path $SolutionFolder -Leaf);
	$ArtifactsFolder = (Join-Path $SolutionFolder "artifacts");
	$ManifestFilePath = (Join-Path $PSScriptRoot  "manifest.json");
}

Task "Default" -depends @("compile", "test", "pack");

Task "Publish" -depends @("clean", "compile", "test", "pack", "push-nuget") `
-description "This task compiles, test then publish all packages to their respective destination.";

# ======================================================================

Task "Restore-Dependencies" -alias "restore" -description "This task generate and/or import all file and module dependencies." `
-action {
	# Import powershell module dependencies
	# ==================================================
	foreach ($module in $Dependencies)
	{
		$modulePath = Join-Path $ToolsFolder "$($module.Name)/*/*.psd1";
		if (-not (Test-Path $modulePath)) { Save-Module $module.Name -MaximumVersion $module.Version -Path $ToolsFolder; }
		Import-Module $modulePath -Force;
		Write-Host "  * imported the '$($module.Name)-$(Split-Path (Get-Item $modulePath).DirectoryName -Leaf)' powershell module.";
	}

	# Generating the build manifest file
	# ==================================================
	if (-not (Test-Path $ManifestFilePath))
	{
		New-NcrementManifest | ConvertTo-Json | Out-File $ManifestFilePath -Encoding utf8;
		Write-Host "  * added 'build/$(Split-Path $ManifestFilePath -Leaf)' to the solution.";
	}

	# Generating a secrets file template
	# ==================================================
	if (-not (Test-Path $SecretsFilePath))
	{
		"{}" | Out-File $SecretsFilePath -Encoding utf8;
		Write-Host "  * added '$(Split-Path $SecretsFilePath -Leaf)' to the solution.";
	}
}

Task "Package-Solution" -alias "pack" -description "This task generates all deployment packages." `
-depends @("restore") -action {
	if (Test-Path $ArtifactsFolder) { Remove-Item $ArtifactsFolder -Recurse -Force; }
	New-Item $ArtifactsFolder -ItemType Directory | Out-Null;
	$version = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName;

	# --- Nuget Package ---

	[string]$package = Join-Path $ArtifactsFolder "";
	[string]$tempPath = Join-Path $SolutionFolder "src/$SolutionName/obj/msbuild";
	$project = Join-Path $SolutionFolder "src/*.MSBuild/*.*proj" | Get-Item;
	Write-Separator "dotnet publish '$($project.BaseName)'";
	Exec { &dotnet publish $project.FullName --configuration $Configuration --output $tempPath; }

	$project = Join-Path $SolutionFolder "src/$SolutionName/*.*proj"  | Get-Item;
	Write-Separator "dotnet pack '$($project.BaseName)'";
	Exec { &dotnet pack $project.FullName --configuration $Configuration --output $ArtifactsFolder -p:"Version=$version"; }

	if (Test-Path $tempPath) { Remove-Item $tempPath -Recurse -Force; }

	[string]$nupkg = Join-Path $ArtifactsFolder "*.nupkg" | Resolve-Path;
	$tempPath = [IO.Path]::ChangeExtension($nupkg, ".zip");
	Copy-Item $nupkg -Destination $tempPath;
	Expand-Archive $tempPath -DestinationPath "$ArtifactsFolder\msbuild" -Force;

	if (Test-Path $tempPath) { Remove-Item $tempPath -Recurse -Force; }

	# --- Powershell Module ---

	[string]$package = Join-Path $ArtifactsFolder "powershell\$SolutionName";
	New-Item $package -ItemType Directory | Out-Null;

	$project = Join-Path $SolutionFolder "src\*.Powershell\*.*proj" | Get-Item;
	Write-Separator "dotnet build '$($project.BaseName)'";
	Exec { &dotnet publish $project.FullName --configuration $Configuration --output $package; }
}

#region ----- PUBLISHING -----------------------------------------------

Task "Publish-NuGet-Packages" -alias "push-nuget" -description "This task publish all nuget packages to a nuget repository." `
-precondition { return ($InProduction -or $InPreview ) -and (Test-Path $ArtifactsFolder -PathType Container) } `
-action {
    foreach ($nupkg in Get-ChildItem $ArtifactsFolder -Filter "*.nupkg")
    {
        Write-Separator "dotnet nuget push '$($nupkg.Name)'";
        Exec { &dotnet nuget push $nupkg.FullName --source "https://api.nuget.org/v3/index.json"; }
    }
}

Task "Publish-PowershellModule" -alias "push-ps" -description "This task publish all powershell modules." `
-precondition { return ($InProduction -or $InPreview ) -and (Test-Path $ArtifactsFolder -PathType Container) } `
-action {
	foreach ($module in (Join-Path $ArtifactsFolder "powershell/*" | Resolve-Path | Get-ChildItem -Filter "*.psd1"))
	{
		if (Test-ModuleManifest $module.FullName)
		{
			try
			{
				$secrets = Get-Secret "psGalleryKey" "POWERSHELL_GALLERY_KEY";
				Push-Location $module.DirectoryName;
				Publish-Module -Path $PWD -NuGetApiKey $secrets;
			}
			finally { Pop-Location; }
		}
	}
}

Task "Add-GitReleaseTag" -alias "tag" -description "This task tags the lastest commit with the version number." `
-precondition { return ($InProduction -or $InPreview ) } `
-depends @("restore") -action {
	$version = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName -Format "C";

	if (-not ((&git status | Out-String) -match 'nothing to commit'))
	{
		Exec { &git add .; }
		Write-Separator "git commit";
		Exec { &git commit -m "Increment version number to '$version'."; }
	}

	Write-Separator "git tag '$version'";
	Exec { &git tag --annotate "v$version" --message "Version $version"; }
}

#endregion

#region ----- COMPILATION ----------------------------------------------

Task "Clean" -description "This task removes all generated files and folders from the solution." `
-action {
	foreach ($itemsToRemove in @("artifacts", "TestResults", "*/*/bin/", "*/*/obj/", "*/*/node_modules/", "*/*/package-lock.json"))
	{
		$itemPath = Join-Path $SolutionFolder $itemsToRemove;
		if (Test-Path $itemPath)
		{
			Resolve-Path $itemPath `
				| Write-Value "  * removed '{0}'." -PassThru `
					| Remove-Item -Recurse -Force;
		}
	}
}

Task "Increment-Version-Number" -alias "version" -description "This task increments all of the projects version number." `
-depends @("restore") -action {
	$manifest = $ManifestFilePath | Step-NcrementVersionNumber -Major:$Major -Minor:$Minor -Patch | Edit-NcrementManifest $ManifestFilePath;
	$newVersion = $ManifestFilePath | Select-NcrementVersionNumber $EnvironmentName;

	foreach ($item in @("*/*/*.*proj", "src/*/*.vsixmanifest"))
	{
		$itemPath = Join-Path $SolutionFolder $item;
		if (Test-Path $itemPath)
		{
			Get-ChildItem $itemPath | Update-NcrementProjectFile $ManifestFilePath `
				| Write-Value "  * incremented '{0}' version number to '$newVersion'.";
		}
	}
}

Task "Build-Solution" -alias "compile" -description "This task compiles projects in the solution." `
-action {
	$solutionFile = Join-Path $SolutionFolder "*.sln" | Get-Item;
	Write-Separator "msbuild '$($solutionFile.Name)'";
	Exec { &$MSBuildExe $solutionFile.FullName -property:Configuration=$Configuration -restore ; }
}

Task "Run-Tests" -alias "test" -description "This task invoke all tests within the 'tests' folder." `
-action {
	foreach ($item in @("tests/*MSTest/*.*proj"))
	{
		[string]$projectPath = Join-Path $SolutionFolder $item;
		if (Test-Path $projectPath -PathType Leaf)
		{
			$projectPath = Resolve-Path $projectPath;
			Write-Separator "dotnet test '$(Split-Path $projectPath -Leaf)'";
			Exec { &dotnet test $projectPath --configuration $Configuration; }
		}
	}
}

#endregion

#region ----- FUNCTIONS ------------------------------------------------

function Write-Value
{
	Param(
		[Parameter(Mandatory)]
		[string]$FormatString,

		$Arg1, $Arg2,

		[Alias('c', "fg")]
		[System.ConsoleColor]$ForegroundColor = [System.ConsoleColor]::Gray,

		[Parameter(ValueFromPipeline)]
		$InputObject,

		[switch]$PassThru
	)

	PROCESS
	{
		Write-Host ([string]::Format($FormatString, $InputObject, $Arg1, $Arg2)) -ForegroundColor $ForegroundColor;
		if ($PassThru -and $InputObject) { return $InputObject }
	}
}

function Write-Separator([string]$Title = "", [int]$length = 70)
{
	$header = [string]::Concat([System.Linq.Enumerable]::Repeat('-', $length));
	if (-not [String]::IsNullOrEmpty($Title))
	{
		$header = $header.Insert(4, " $Title ");
		if ($header.Length -gt $length) { $header = $header.Substring(0, $length); }
	}
	Write-Host "`r`n$header`r`n" -ForegroundColor DarkGray;
}

function Get-Secret
{
	Param(
		[Parameter(Mandatory)]
		[string]$JPath,

		[Parameter(Mandatory)]
		[string]$EnvironmentVariable
	)

	$result = [Environment]::ExpandEnvironmentVariables("%$EnvironmentVariable%");
	if ([string]::IsNullOrEmpty($result) -or ($result -eq "%$EnvironmentVariable%"))
	{
		$result = Get-Content $SecretsFilePath | Out-String | ConvertFrom-Json;
		$properties = $JPath.Split(@('.', '/', ':'));
		foreach($prop in $properties)
		{
			$result = $result.$prop;
		}
	}
	return $result;
}

#endregion