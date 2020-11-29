$modulePath = Join-Path ([IO.Path]::GetTempPath()) "pwsh-module";
if (-not (Test-Path $modulePath)) { New-Item $modulePath -ItemType Directory | Out-Null; }

$project = Join-Path (Split-Path $PSScriptRoot -Parent | Split-Path -Parent) "src\*.Powershell\*.*proj" | Resolve-Path;
&dotnet publish $project --output $modulePath;

$module = Join-Path $modulePath "*.psd1" | Get-Item;
Import-Module $module.FullName -Force;

#help Add-UserSecret -Full;

Push-Location $PSScriptRoot;

# ==========

Describe "Set-Secret"{
	$sourceFile = Join-Path ([IO.Path]::GetTempPath()) "cecrets-powershell-test.json";
	if (Test-Path $sourceFile) { Remove-Item $sourceFile -Force; }

	Set-Secret -k "a" -v "this is a value" -Path $sourceFile;

	It "Can set json value" {
		$sourceFile | Should Exist;
	}
}

Pop-Location;
Remove-Module $module.FullName -Force -ErrorAction Ignore;