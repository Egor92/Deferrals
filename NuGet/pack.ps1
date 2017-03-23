$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\src\Egor92.Deferrals\bin\Release\Egor92.Deferrals.dll").GetName().Version

Write-Host "Setting .nuspec version tag to $version"

$content = (Get-Content $root\NuGet\Egor92.Deferrals.nuspec) 
$content = $content -replace '\$version\$',$version

$content | Out-File $root\NuGet\Egor92.Deferrals.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\NuGet\Egor92.Deferrals.compiled.nuspec -outputdirectory $root\Artifacts