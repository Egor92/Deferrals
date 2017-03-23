$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\src\Egor92.Deferrals\bin\Release\Egor92.Deferrals.dll").GetName().Version

Write-Host "Setting .nuspec version tag to $version"

$content = (Get-Content $root\NuGet\Egor92.Deferrals.nuspec) 
$content = $content -replace '\$version\$',$version

$content | Out-File $root\NuGet\Egor92.Deferrals.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\NuGet\Egor92.Deferrals.compiled.nuspec -outputdirectory $root\Packages

& $root\NuGet\NuGet.exe setApiKey Enbbe1MRpwxTk+l70bA1DfoJq62BO4S01RRKFDL4Fx83I/f3W4D4MQJTRXrNwkFj

& $root\NuGet\NuGet.exe push $root\Packages\Egor92.Deferrals.$version.nupkg