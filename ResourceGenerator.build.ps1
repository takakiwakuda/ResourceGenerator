[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = "Debug"
)

<#
.SYNOPSIS
    Run "dotnet build".
#>
task Build @{
    Inputs  = Get-ChildItem -Path *.cs, *.csproj
    Outputs = "bin\$Configuration\net48\ResourceGenerator.dll"
    Jobs    = {
        exec { dotnet build -c $Configuration }
    }
}

<#
.SYNOPSIS
    Remove bin, obj, and out directories and thier sub-items.
#>
task Clean {
    remove bin, obj, out
}

<#
.SYNOPSIS
    Compile PowerShell module.
#>
task PackUp {
    $manifest = "bin\$Configuration\net48\ResourceGenerator.psd1"
    $version = (Import-PowerShellDataFile -LiteralPath $manifest).ModuleVersion
    $output = "out\$Configuration\net48\ResourceGenerator\$version"

    if (Test-Path -LiteralPath $output) {
        Remove-Item -Path $output\* -Recurse
    }
    else {
        $null = New-Item -Path $output -ItemType Directory
    }

    Copy-Item -Path bin\$Configuration\net48\ResourceGenerator.*, en-US, ja-JP -Destination $output -Recurse
}

<#
.SYNOPSIS
    Generate resource files.
#>
task ResGen @{
    Inputs  = "Properties\Resources.txt"
    Outputs = "Properties\Resources.Designer.cs"
    Jobs    = {
        $ipmo = "Import-Module -Name '$PSScriptRoot\out\$Configuration\net48\ResourceGenerator'"
        $source = "$PSScriptRoot\Properties\Resources.txt"
        $output = "$PSScriptRoot\Properties"
        $command = "& { $ipmo; New-ResourceFile '$source' '$output' ResourceGenerator.Properties Resources }"

        exec { powershell -Command $command }
    }
}

<#
.SYNOPSIS
    Run tests for PowerShell module.
#>
task Test {
    $module = "$PSScriptRoot\out\$Configuration\net48\ResourceGenerator"
    $command = "& { Import-Module -Name '$module'; Invoke-Pester -Path '$PSScriptRoot\test' }"

    exec { powershell -Command $command }
}

<#
.SYNOPSIS
    Run Build.
#>
task . Build, PackUp

<#
.SYNOPSIS
    Run Build and Test.
#>
task BuildAndTest Build, PackUp, Test
