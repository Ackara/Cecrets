$modulePath = Join-Path ([IO.Path]::GetTempPath()) "pwsh-module";
if (-not (Test-Path $modulePath)) { New-Item $modulePath -ItemType Directory | Out-Null; }

$project = Join-Path (Split-Path $PSScriptRoot -Parent | Split-Path -Parent) "src\*.Powershell\*.*proj" | Resolve-Path;
&dotnet publish $project --output $modulePath;

$module = Join-Path $modulePath "*.psd1" | Get-Item;
Import-Module $module.FullName -Force;

#help Add-UserSecret -Full;

Push-Location $PSScriptRoot;

# ==========

Describe "Add-UserSecret"{
	Add-UserSecret ""

	It "foo"{
	}
}

Pop-Location;
Remove-Module $module.FullName -Force;