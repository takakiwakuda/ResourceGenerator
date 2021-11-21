[CmdletBinding()]
param (
    # Specifies the build configuration. The default is Debug.
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = "Debug"
)

#region Variables
$ModuleVersion = (Import-PowerShellDataFile -LiteralPath ResourceGenerator.psd1).ModuleVersion
$Module = "$PSScriptRoot\out\$Configuration\ResourceGenerator"
$OutputPath = "$Module\$ModuleVersion"
#endregion

<#
.SYNOPSIS
    Runs the build of the ResourceGenerator project.
#>
task Build @{
    Inputs  = Get-ChildItem -Path *.cs, *.csproj
    Outputs = "bin\$Configuration\ResourceGenerator.dll"
    Jobs    = {
        exec { dotnet build --nologo -c $Configuration }
    }
}

<#
.SYNOPSIS
    Runs the build of the ResourceGenerator module.
#>
task BuildModule @{
    Inputs  = "bin\$Configuration\ResourceGenerator.dll"
    Outputs = "$OutputPath\ResourceGenerator.dll"
    Jobs    = {
        if (Test-Path -LiteralPath $OutputPath -PathType Container) {
            Remove-Item -Path $OutputPath\* -Recurse
        }
        else {
            $null = New-Item -Path $OutputPath -ItemType Directory
        }

        $params = @{
            Path        = "bin\$Configuration\ResourceGenerator.*", "en-US", "ja-JP"
            Destination = $OutputPath
            Recurse     = $true
        }
        Copy-Item @params
    }
}

<#
.SYNOPSIS
    Removes bin, obj, and out directories and thier sub-items.
#>
task Clean {
    remove bin, obj, out
}

<#
.SYNOPSIS
    Generates resources for the ResourceGenerator project.
#>
task ResGen @{
    Inputs  = "Properties\Resources.txt"
    Outputs = "Properties\Resources.Designer.cs"
    Jobs    = {
        $ipmo = "Import-Module -Name '$Module'"
        $source = "$PSScriptRoot\Properties\Resources.txt"
        $output = "$PSScriptRoot\Properties"
        $command = "& { $ipmo; New-ResourceFile '$source' '$output' ResourceGenerator.Properties Resources -Force }"

        exec { powershell -NoProfile -Command $command }
    }
}

<#
.SYNOPSIS
    Runs the test of the ResourceGenerator module.
#>
task Test {
    $ipmo = "Import-Module -Name '$Module'"
    $command = "& { $ipmo; Invoke-Pester -Path '$PSScriptRoot\test' -Output Detailed }"

    exec { powershell -NoProfile -Command $command }
}

<#
.SYNOPSIS
    Runs build tasks.
#>
task . Build, BuildModule
