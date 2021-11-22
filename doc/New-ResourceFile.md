---
external help file: ResourceGenerator.dll-Help.xml
Module Name: ResourceGenerator
online version: https://github.com/takakiwakuda/ResourceGenerator/blob/main/doc/New-ResourceFile.md
schema: 2.0.0
---

# New-ResourceFile

## SYNOPSIS

Creates resources for .NET application and assemblies.

## SYNTAX

### ResourceList (Default)

```powershell
New-ResourceFile [-ResourceList] <IDictionary> [-OutputDirectory] <String> [-Namespace] <String> [-TypeName] <String> [-ResourceType <ResourceType>] [-PublicClass] [-Force] [<CommonParameters>]
```

### Path

```powershell
New-ResourceFile [-Path] <String> [-OutputDirectory] <String> [-Namespace] <String> [-TypeName] <String> [-ResourceType <ResourceType>] [-PublicClass] [-Force] [<CommonParameters>]
```

## DESCRIPTION

The `New-ResourceFile` cmdlet creates resources for .NET application and assemblies.

## EXAMPLES

### Example 1

```powershell
PS C:\> New-ResourceFile -ResourceList @{ HelloWorld = 'Hello World!' } -OutputDirectory 'C:\Projects' -Namespace 'HelloWorld' -TypeName 'Resources'
```

This example creates `Resources.resx` and `Resources.Designer.cs` files in the specified directory `C:\Projects`.

### Example 2

```powershell
PS C:\> New-ResourceFile -Path 'C:\Projects\Resources.txt' -OutputDirectory 'C:\Projects' -Namespace 'HelloWorld' -TypeName 'Resources' -ResourceType All
```

This example creates `Resources.resx`, `Resources.Designer.cs`, and `Resources.resources` files in the specified directory `C:\Projects`.

## PARAMETERS

### -Force

Indicates that existing resource files are overwritten.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Overwrite

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Namespace

Specifies a namespace of a resource to be created.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputDirectory

Specifies a path to an output directory.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

Specifies a path to a file. The file must contain key-value pairs such as `Key=Pair`.

```yaml
Type: String
Parameter Sets: Path
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PublicClass

Indicates that the resource to be created is a public class.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResourceList

Specifies a dictionary object that inherits from the `System.Collections.IDictionary` class.

```yaml
Type: IDictionary
Parameter Sets: ResourceList
Aliases: Resources

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResourceType

Specifies a resource type to be created. By default, an XML resource file (.resx) and a CSharp source file (.Designer.cs) are created.

```yaml
Type: ResourceType
Parameter Sets: (All)
Aliases:
Accepted values: Binary, CSharp, Xml, Text, All

Required: False
Position: Named
Default value: Text
Accept pipeline input: False
Accept wildcard characters: False
```

### -TypeName

Specifies a type name of a resource to be created.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.IO.FileInfo

The `New-ResourceFile` returns `System.IO.FileInfo` representing resource files.

## NOTES

Basically, you might want to use Resource File Generator (Resgen.exe).

## RELATED LINKS
