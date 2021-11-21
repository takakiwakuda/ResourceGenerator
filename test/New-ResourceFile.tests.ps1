#Requires -Module ResourceGenerator

using namespace System.IO
using namespace System.Management.Automation

Set-StrictMode -Version 3.0

Describe "New-ResourceFile" {
    AfterEach {
        Get-ChildItem -LiteralPath "TestDrive:" -File | Remove-Item
    }

    Context "ResourceList parameter" -Tag "RL" {
        It "Throws an exception if the ResourceList parameter is an empty dictionary" {
            $params = @{
                ResourceList    = @{ }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "EmptyResourceList,ResourceGenerator.NewResourceFileCommand"
        }

        It "Should create resources" {
            $params = @{
                ResourceList    = @{
                    Cat  = "Nuko"
                    Nine = 9
                    True = $true
                }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 3
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            $files[2].Name | Should -BeExactly "Resources.resources"
        }

        It "Should create resources with the PublicClass parameter" {
            $params = @{
                ResourceList    = @{
                    Dog     = "Wanko"
                    False   = $false
                    Numbers = @(0x000F, 0x00FF, 0x0FFF, 0xFFFF)
                }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                PublicClass     = $true
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 3
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            $files[2].Name | Should -BeExactly "Resources.resources"
            Select-String -Pattern "^    public class Resources {$" -LiteralPath $files[1] -Quiet | Should -BeTrue
        }

        It "Should create an XML resource file (.resx)" {
            $params = @{
                ResourceList    = @{
                    Rabbit = "Unagi"
                    Eight  = 8
                }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "Xml"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.resx"
            "TestDrive:\Resources.Designer.cs" | Should -Not -Exist
            "TestDrive:\Resources.resources" | Should -Not -Exist
        }

        It "Should create a CSharp source file (.Designer.cs)" {
            $params = @{
                ResourceList    = @{
                    Eel      = "Usagi"
                    Thousand = 1000
                }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "CSharp"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.Designer.cs"
            "TestDrive:\Resources.resx" | Should -Not -Exist
            "TestDrive:\Resources.resources" | Should -Not -Exist
        }

        It "Should create a binary source file (.resources)" {
            $params = @{
                ResourceList    = @{
                    Frog    = "Kitaku"
                    Million = 1E+6
                }
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "Binary"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.resources"
            "TestDrive:\Resources.resx" | Should -Not -Exist
            "TestDrive:\Resources.Designer.cs" | Should -Not -Exist
        }
    }

    Context "Path parameter" -Tag "P" {
        It "Throws an exception if the path in the Path parameter does not exist" {
            $params = @{
                Path            = "TestDrive:\NonExistent.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "PathNotFound,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the root of the path in the Path parameter does not exist" {
            $params = @{
                Path            = "NonExistent:"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "DriveNotFound,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the path in the Path parameter is not a FileSystemProvider" {
            $params = @{
                Path            = "TestRegistry:"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "NotFileSystemProvider,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the path in the Path parameter is a directory" {
            $params = @{
                Path            = "TestDrive:"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "DirectoryNotFound,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the path in the Path parameter is a directory" {
            $params = @{
                Path            = $PSScriptRoot
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "PermissionDenied,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the line read is an invalid key-value pair" {
            $params = @{
                Path            = "$PSScriptRoot\IncorrectResources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ErrorAction     = [ActionPreference]::Stop
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "InvalidKeyValuePair,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if there are no valid key-value pairs" {
            $params = @{
                Path            = "$PSScriptRoot\EmptyResources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "InvalidFormatFile,ResourceGenerator.NewResourceFileCommand"
        }

        It "Should create resources" {
            $params = @{
                Path            = "$PSScriptRoot\Resources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 3
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            $files[2].Name | Should -BeExactly "Resources.resources"
        }

        It "Should create resources with the PublicClass parameter" {
            $params = @{
                Path            = "$PSScriptRoot\Resources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                PublicClass     = $true
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 3
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            Select-String -Pattern "^    public class Resources {$" -LiteralPath $files[1] -Quiet | Should -BeTrue
            $files[2].Name | Should -BeExactly "Resources.resources"
        }

        It "Should create an XML resource file (.resx)" {
            $params = @{
                Path            = "$PSScriptRoot\Resources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "Xml"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.resx"
            "TestDrive:\Resources.Designer.cs" | Should -Not -Exist
            "TestDrive:\Resources.resources" | Should -Not -Exist
        }

        It "Should create a CSharp source file (.Designer.cs)" {
            $params = @{
                Path            = "$PSScriptRoot\Resources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "CSharp"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.Designer.cs"
            "TestDrive:\Resources.resx" | Should -Not -Exist
            "TestDrive:\Resources.resources" | Should -Not -Exist
        }

        It "Should create a binary source file (.resources)" {
            $params = @{
                Path            = "$PSScriptRoot\Resources.txt"
                OutputDirectory = "TestDrive:"
                Namespace       = "Test.Properties"
                TypeName        = "Resources"
                ResourceType    = "Binary"
            }

            $file = New-ResourceFile @params
            $file.Count | Should -Be 1
            $file | Should -BeOfType ([FileInfo])
            $file.Name | Should -BeExactly "Resources.resources"
            "TestDrive:\Resources.resx" | Should -Not -Exist
            "TestDrive:\Resources.Designer.cs" | Should -Not -Exist
        }
    }
}
