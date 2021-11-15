#Requires -Module ResourceGenerator

using namespace System.IO
using namespace System.Management.Automation

Set-StrictMode -Version 3.0

Describe "ResourceGenerator" {
    BeforeAll {
        $TestDrive = (Resolve-Path -LiteralPath TestDrive:).ProviderPath
        $FooFile = Get-Item -LiteralPath $PSScriptRoot\Foo.txt
        $EmptyFile = New-Item -Path $TestDrive\Empty.txt -ItemType File
    }

    Context "New-ResourceFile.ResourceList" {
        It "Throws an exception if the path in Path does not exist" {
            $params = @{
                ResourceList    = @{ }
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "EmptyResourceList,ResourceGenerator.NewResourceFileCommand"
        }

        It "Should create resource files" {
            $params = @{
                ResourceList    = @{
                    Foo = "Foo"
                    Bar = "Bar"
                }
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 2
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
        }

        It "Should create resource files with PublicClass" {
            $params = @{
                ResourceList    = @{
                    Foo = "Foo"
                    Bar = "Bar"
                }
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
                PublicClass     = $true
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 2
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            Select-String -Pattern "^    public class Resources {$" -LiteralPath $files[1] -Quiet | Should -BeTrue
        }
    }

    Context "New-ResourceFile.Path" {
        It "Throws an exception if the path in Path does not exist" {
            $params = @{
                Path            = "$TestDrive\Foo.txt"
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "PathNotFound,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the root of the path in Path does not exist" {
            $params = @{
                Path            = "Foo:"
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "DriveNotFound,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the path in Path is not a file system provider" {
            $params = @{
                Path            = "TestRegistry:"
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "NotFileSystemProvider,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the path in Path is a directory" {
            $params = @{
                Path            = "$TestDrive"
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "FileOpenFailure,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if the line read is a valid key/value pair" {
            $params = @{
                Path            = $FooFile
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
                ErrorAction     = [ActionPreference]::Stop
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "InvalidKeyValuePair,ResourceGenerator.NewResourceFileCommand"
        }

        It "Throws an exception if there are no valid key/value pairs in the file" {
            $params = @{
                Path            = $EmptyFile
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
            }

            { New-ResourceFile @params } |
            Should -Throw -ErrorId "InvalidFormatFile,ResourceGenerator.NewResourceFileCommand"
        }

        It "Should create resource files" {
            $params = @{
                Path            = $FooFile
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
                ErrorAction     = [ActionPreference]::Ignore
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 2
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
        }

        It "Should create resource files with PublicClass" {
            $params = @{
                Path            = $FooFile
                OutputDirectory = $TestDrive
                Namespace       = "Foo.Properties"
                TypeName        = "Resources"
                PublicClass     = $true
                ErrorAction     = [ActionPreference]::Ignore
            }

            $files = New-ResourceFile @params
            $files.Count | Should -Be 2
            $files | Should -BeOfType ([FileInfo])
            $files[0].Name | Should -BeExactly "Resources.resx"
            $files[1].Name | Should -BeExactly "Resources.Designer.cs"
            Select-String -Pattern "^    public class Resources {$" -LiteralPath $files[1] -Quiet | Should -BeTrue
        }
    }
}
