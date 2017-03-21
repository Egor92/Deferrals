$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\src\Egor92.Deferrals\bin\Release\Egor92.Deferrals.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\NuGet\Egor92.Deferrals.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\NuGet\Egor92.Deferrals.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\NuGet\Egor92.Deferrals.compiled.nuspec -outputdirectory $root\NuGet

& $root\NuGet\NuGet.exe setApiKey Enbbe1MRpwxTk+l70bA1DfoJq62BO4S01RRKFDL4Fx83I/f3W4D4MQJTRXrNwkFj

& $root\NuGet\NuGet.exe push $root\NuGet\Egor92.Deferrals.$version.nupkg