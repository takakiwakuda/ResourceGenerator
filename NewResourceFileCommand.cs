using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Resources;
using System.Resources.Tools;
using System.Security;
using Microsoft.CSharp;
using Resources = ResourceGenerator.Properties.Resources;

namespace ResourceGenerator;

/// <summary>
/// New-ResourceFile cmdlet creates .resx and .Designer.cs files.
/// </summary>
[Cmdlet(VerbsCommon.New, "ResourceFile", DefaultParameterSetName = ResourceListSetName,
HelpUri = "https://github.com/takakiwakuda/ResourceGenerator/blob/main/doc/New-ResourceFile.md")]
[OutputType(typeof(FileInfo))]
public sealed class NewResourceFileCommand : PSCmdlet
{
    private const string PathSetName = "Path";
    private const string ResourceListSetName = "ResourceList";

    /// <summary>
    /// ResourceList parameter
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = ResourceListSetName, Position = 0)]
    [ValidateNotNull()]
    [Alias("Resources")]
    public IDictionary ResourceList { get; set; }

    /// <summary>
    /// Path parameter
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = PathSetName, Position = 0)]
    [ValidateNotNullOrEmpty()]
    public string Path { get; set; }

    /// <summary>
    /// OutputDirectory parameter
    /// </summary>
    [Parameter(Mandatory = true, Position = 1)]
    [ValidateNotNullOrEmpty()]
    public string OutputDirectory { get; set; }

    /// <summary>
    /// Namespace parameter
    /// </summary>
    [Parameter(Mandatory = true, Position = 2)]
    [ValidateNotNullOrEmpty()]
    public string Namespace { get; set; }

    /// <summary>
    /// TypeName parameter
    /// </summary>
    [Parameter(Mandatory = true, Position = 3)]
    [ValidateNotNullOrEmpty()]
    public string TypeName { get; set; }

    /// <summary>
    /// GlobalClass parameter
    /// </summary>
    [Parameter()]
    public SwitchParameter GlobalClass { get; set; }

    /// <summary>
    /// ProcessRecord() override.
    /// </summary>
    protected override void ProcessRecord()
    {
        switch (ParameterSetName)
        {
            case PathSetName:
                Path = ResolveFilePath(Path);
                ResourceList = CreateDictionaryFromFile(Path);
                break;

            default:
                if (ResourceList.Count == 0)
                {
                    ErrorRecord errorRecord = new(
                        new InvalidOperationException(Resources.EmptyResourceList),
                        "EmptyResourceList",
                        ErrorCategory.InvalidArgument,
                        ResourceList
                    );

                    ThrowTerminatingError(errorRecord);
                }
                break;
        }

        OutputDirectory = ResolveFilePath(OutputDirectory);

        string resxFileName = System.IO.Path.Combine(OutputDirectory, TypeName + ".resx");
        GenerateResXResource(resxFileName);

        string csharpFileName = System.IO.Path.Combine(OutputDirectory, TypeName + ".Designer.cs");
        GenerateResXDesigner(resxFileName, csharpFileName);

        WriteObject(SessionState.InvokeProvider.Item.Get(new string[] { resxFileName, csharpFileName }, false, true), true);
    }

    /// <summary>
    /// Creates a dictionary from a specified text file.
    /// </summary>
    /// <param name="path">Path to a text file to create a dictionary</param>
    /// <returns>The dictionary created from the read text file</returns>
    private IDictionary CreateDictionaryFromFile(string path)
    {
        char[] separator = new char[] { '=' };
        Dictionary<string, string> dict = new();

        try
        {
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);

            while (!reader.EndOfStream)
            {
                string readLine = reader.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(readLine))
                {
                    continue;
                }

                string[] text = readLine.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                if (text.Length == 1)
                {
                    ErrorRecord errorRecord = new(
                        new IOException(string.Format(Resources.InvalidKeyValuePair, readLine)),
                        "InvalidKeyValuePair",
                        ErrorCategory.ParserError,
                        readLine
                    );

                    WriteError(errorRecord);
                    continue;
                }

                dict.Add(text[0], text[1]);
            }
        }
        catch (Exception e) when (e is DirectoryNotFoundException || e is UnauthorizedAccessException)
        {
            ErrorRecord errorRecord = new(
                e,
                "FileOpenFailure",
                ErrorCategory.OpenError,
                path
            );

            ThrowTerminatingError(errorRecord);
        }

        if (dict.Count == 0)
        {
            ErrorRecord errorRecord = new(
                new InvalidOperationException(string.Format(Resources.InvalidFormatFile, path)),
                "InvalidFormatFile",
                ErrorCategory.InvalidArgument,
                path
            );

            ThrowTerminatingError(errorRecord);
        }

        return dict;
    }

    /// <summary>
    /// Creates a <see cref="StreamWriter" /> from a specified file.
    /// </summary>
    /// <param name="path">Path to open</param>
    /// <returns>The <see cref="StreamWriter" /> representing path</returns>
    private StreamWriter CreateStreamWriter(string path)
    {
        StreamWriter writer = null;

        try
        {
            writer = new StreamWriter(path);
        }
        catch (Exception e) when (e is UnauthorizedAccessException || e is SecurityException)
        {
            ErrorRecord errorRecord = new(
                e,
                "PermissionDenied",
                ErrorCategory.PermissionDenied,
                path
            );

            ThrowTerminatingError(errorRecord);
        }

        return writer;
    }

    /// <summary>
    /// Generates a .Designer.cs file from a specified ResX file.
    /// </summary>
    /// <param name="resxFileName">Path to the ResX file name</param>
    /// <param name="csharpFileName">Path to the file name generate</param>
    private void GenerateResXDesigner(string resxFileName, string csharpFileName)
    {
        using StreamWriter writer = CreateStreamWriter(csharpFileName);
        using CSharpCodeProvider provider = new();
        CodeCompileUnit compileUnit = StronglyTypedResourceBuilder.Create(
            resxFileName,
            TypeName,
            Namespace,
            provider,
            !GlobalClass.IsPresent,
            out string[] unmatchable
        );

        foreach (string error in unmatchable)
        {
            WriteWarning(error);
        }

        provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
    }

    /// <summary>
    /// Generates ResX resources from a specified dictionary.
    /// </summary>
    /// <param name="path">Path to the file name to generate</param>
    private void GenerateResXResource(string path)
    {
        using StreamWriter streamWriter = CreateStreamWriter(path);
        using ResXResourceWriter resourceWriter = new(streamWriter);

        IDictionaryEnumerator enumerator = ResourceList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            resourceWriter.AddResource((string)enumerator.Key, enumerator.Value);
        }
    }

    /// <summary>
    /// Resolves the specified path to a fully qualified path.
    /// </summary>
    /// <param name="path">Path to resolve.</param>
    /// <returns>The fully qualified path of path</returns>
    private string ResolveFilePath(string path)
    {
        try
        {
            if (!SessionState.InvokeProvider.Item.Exists(path, false, true))
            {
                ErrorRecord errorRecord = new(
                    new ItemNotFoundException(string.Format(Resources.PathNotFound, path)),
                    "PathNotFound",
                    ErrorCategory.ObjectNotFound,
                    path
                );

                ThrowTerminatingError(errorRecord);
            }
        }
        catch (System.Management.Automation.DriveNotFoundException e)
        {
            ErrorRecord errorRecord = new(
                e,
                "DriveNotFound",
                ErrorCategory.ObjectNotFound,
                path
            );

            ThrowTerminatingError(errorRecord);
        }

        path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out ProviderInfo provider, out _);
        if (!provider.Name.Equals("FileSystem", StringComparison.InvariantCulture))
        {
            ErrorRecord errorRecord = new(
                new InvalidOperationException(string.Format(Resources.NotFileSystemProvider, path)),
                "NotFileSystemProvider",
                ErrorCategory.InvalidArgument,
                path
            );

            ThrowTerminatingError(errorRecord);
        }

        return path;
    }
}
