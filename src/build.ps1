param (
    [Parameter(Mandatory=$false)]
    [switch] $NuGet
)
<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

# Build script configuration

$configuration = "Release"
$projects = @(".\UblSharp", ".\UblSharp.Validation")

$sgen = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\sgen.exe" 

# End configuration

Push-Location $(Split-Path $Script:MyInvocation.MyCommand.Path)

if(Test-Path .\..\artifacts) { Remove-Item .\..\artifacts -Force -Recurse }
New-Item -ItemType directory -Path .\..\artifacts

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
    [xml]$nuspec = Get-Content "$project\package.nuspec"
    $version = $nuspec.package.metadata.version

    # Do not add ci version number when building package for nuget.org
    if ($NuGet -eq $false -And $env:APPVEYOR_BUILD_NUMBER -ne $NULL){
        $version = "$version-ci$env:APPVEYOR_BUILD_NUMBER"
    }

    exec { & nuget pack "$project\package.nuspec" -version $version -symbols -outputdirectory .\..\artifacts -properties Configuration=Release }
}

Pop-Location
