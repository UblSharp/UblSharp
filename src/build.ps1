param (
    [Parameter(Mandatory=$false)]
    [switch] $NuGet
)

# includes
. "$PSScriptRoot\.build\build-functions.ps1"

# Build script configuration

$configuration = "Release"
$projects = @(".\UblSharp", ".\UblSharp.Validation")

$sgen = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\sgen.exe" 

# End configuration

Push-Location $(Split-Path $Script:MyInvocation.MyCommand.Path)

if(Test-Path .\..\artifacts) { Remove-Item .\..\artifacts -Force -Recurse }
New-Item -ItemType directory -Path .\..\artifacts | Out-Null

exec { & dotnet restore }

# Build projects
foreach($project in $projects) {
    exec { & dotnet build $project -c Release }
}

# Generate XmlSerializers assemblies
$sgenfw = @("net20", "net35", "net40", "net45")
foreach ($fw in $sgenfw) {
    exec { & $sgen /assembly:.\UblSharp\bin\$configuration\$fw\UblSharp.dll /verbose /force }
}

# build tests first
exec { & dotnet build .\UblSharp.Tests -c Release }

# manually copy sgen assemblies to test bin directory
# hack: hard-coded platform name
Copy-Item -Path ".\UblSharp\bin\$configuration\net45\UblSharp.XmlSerializers.dll" -Destination ".\UblSharp.Tests\bin\$configuration\net46\win7-x64" -Force

# Build/Run tests
exec { & dotnet test .\UblSharp.Tests -c Release }

# Create packages  
foreach($project in $projects) {
    $version = (ConvertFrom-Json -InputObject (Get-Content $project\project.json -Raw)).version

    # Do not add ci version number when building package for nuget.org
    if ($NuGet -eq $false -And $env:APPVEYOR_BUILD_NUMBER -ne $NULL){
        $version = "$version-ci$env:APPVEYOR_BUILD_NUMBER"
    }

    [xml]$nuspec = Get-Content "$project\package.nuspec" -Encoding UTF8
    $nuspec.package.metadata.version = $version
    $nuspec.Save((Resolve-Path "$project\package.nuspec"))

    Write-Host "Updated version in $project\package.nuspec to $version"

    exec { & nuget pack "$project\package.nuspec" -version $version -symbols -outputdirectory .\..\artifacts -properties Configuration=Release }
}

Pop-Location
