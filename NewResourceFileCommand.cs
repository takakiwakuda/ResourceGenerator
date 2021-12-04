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
using System.Text;
using Microsoft.CSharp;
using Microsoft.PowerShell.Commands;
using Resources = ResourceGenerator.Properties.Resources;

namespace ResourceGenerator
{
    /// <summary>
    /// Defines the value the resource type to be generated.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// The resource type is a binary file (.resources).
        /// </summary>
        Binary = 1,

        /// <summary>
        /// The resource type is a CSharp source file (.Designer.cs).
        /// </summary>
        CSharp = 2,

        /// <summary>
        /// The resource type is an XML resource file (.resx).
        /// </summary>
        Xml = 4,

        /// <summary>
        /// The resource type are text files that contains CSharp and XML resources.
        /// </summary>
        Text = CSharp | Xml,

        /// <summary>
        /// All types of resources.
        /// </summary>
        All = Binary | CSharp | Xml
    }

    /// <summary>
    /// The New-ResourceFile cmdlet creates resources for .NET applications and assemblies.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ResourceFile", DefaultParameterSetName = ResourceListSetName,
            HelpUri = "https://github.com/takakiwakuda/ResourceGenerator/blob/main/doc/New-ResourceFile.md")]
    [OutputType(typeof(FileInfo))]
    public sealed class NewResourceFileCommand : PSCmdlet
    {
        private const string PathSetName = "Path";
        private const string ResourceListSetName = "ResourceList";

        /// <summary>
        /// Gets or sets the ResourceList parameter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [Parameter(Mandatory = true, ParameterSetName = ResourceListSetName, Position = 0)]
        [ValidateNotNull]
        [Alias("Resources")]
        public IDictionary ResourceList { get; set; }

        /// <summary>
        /// Gets or sets the Path parameter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [Parameter(Mandatory = true, ParameterSetName = PathSetName, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the OutputDirectory parameter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the Namespace parameter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateNotNullOrEmpty]
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the TypeName parameter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.AllowNull]
        [Parameter(Mandatory = true, Position = 3)]
        [ValidateNotNullOrEmpty]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the ResourceType parameter.
        /// </summary>
        [Parameter]
        public ResourceType ResourceType { get; set; } = ResourceType.Text;

        /// <summary>
        /// Gets or sets the PublicClass parameter.
        /// </summary>
        [Parameter]
        public SwitchParameter PublicClass { get; set; }

        /// <summary>
        /// Gets or sets the Force parameter.
        /// </summary>
        [Parameter]
        [Alias("Overwrite")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// ProcessRecord() override.
        /// Creates resources for .NET applications and assemblies.
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
            List<string> fileNames = new();

            if (ResourceType.HasFlag(ResourceType.Xml))
            {
                string resxFileName = System.IO.Path.Combine(OutputDirectory, TypeName + ".resx");
                if (CanGenerateFile(resxFileName))
                {
                    WriteVerbose(string.Format(Resources.GeneratingFile, resxFileName));
                    GenerateResXResourceFile(resxFileName);
                }
                fileNames.Add(resxFileName);
            }

            if (ResourceType.HasFlag(ResourceType.CSharp))
            {
                string csharpFileName = System.IO.Path.Combine(OutputDirectory, TypeName + ".Designer.cs");
                if (CanGenerateFile(csharpFileName))
                {
                    WriteVerbose(string.Format(Resources.GeneratingFile, csharpFileName));
                    GenerateCSharpSourceFile(csharpFileName);
                }
                fileNames.Add(csharpFileName);
            }

            if (ResourceType.HasFlag(ResourceType.Binary))
            {
                string binaryFileName = System.IO.Path.Combine(OutputDirectory, TypeName + ".resources");
                if (CanGenerateFile(binaryFileName))
                {
                    WriteVerbose(string.Format(Resources.GeneratingFile, binaryFileName));
                    GenerateBinaryResourceFile(binaryFileName);
                }
                fileNames.Add(binaryFileName);
            }

            WriteObject(SessionState.InvokeProvider.Item.Get(fileNames.ToArray(), false, true), true);
        }

        /// <summary>
        /// Checks if a file able to be generated with the specified path.
        /// </summary>
        /// <param name="path">The file to test.</param>
        /// <returns><see langword="true"/> if a file able to be generated; otherwise, <see langword="false"/>.</returns>
        private bool CanGenerateFile(string path)
        {
            if (Force.IsPresent || !File.Exists(path))
            {
                return true;
            }

            WriteWarning(string.Format(Resources.FileAlreadyExists, path));
            return false;
        }

        /// <summary>
        /// Creates a dictionary from the specified file.
        /// </summary>
        /// <remarks>Only UTF-8 encoding is supported.</remarks>
        /// <param name="path">The file to create a dictionary.</param>
        /// <returns>The dictionary created from the <paramref name="path"/> parameter.</returns>
        private IDictionary CreateDictionaryFromFile(string path)
        {
            char[] separator = new char[] { '=' };
            Dictionary<string, string> dict = new();

            try
            {
                using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader reader = new(stream, new UTF8Encoding(false));

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

                    dict.Add(text[0].Trim(), text[1].Trim());
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                ErrorRecord errorRecord = new(
                    ex,
                    "DirectoryNotFound",
                    ErrorCategory.ObjectNotFound,
                    path
                );
                ThrowTerminatingError(errorRecord);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
            {
                ErrorRecord errorRecord = new(
                    ex,
                    "PermissionDenied",
                    ErrorCategory.PermissionDenied,
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
        /// Generates resources in a binary file (.resources) to the specified path.
        /// </summary>
        /// <param name="path">The file to generate.</param>
        private void GenerateBinaryResourceFile(string path)
        {
            using FileStream stream = new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using ResourceWriter writer = new(stream);

            IDictionaryEnumerator enumerator = ResourceList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                writer.AddResource((string)enumerator.Key, enumerator.Value);
            }
        }

        /// <summary>
        /// Generates resources in a C# source file from the RsourceList to the specified path.
        /// </summary>
        /// <remarks>Only UTF-8 encoding is supported.</remarks>
        /// <param name="csharpFileName">The file to generate.</param>
        private void GenerateCSharpSourceFile(string csharpFileName)
        {
            using FileStream stream = new(csharpFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new(stream, new UTF8Encoding(false));
            using CSharpCodeProvider provider = new();

            CodeCompileUnit compileUnit = StronglyTypedResourceBuilder.Create(
                ResourceList,
                TypeName,
                Namespace,
                provider,
                !PublicClass.IsPresent,
                out string[] unmatchable
            );

            foreach (string warningMessage in unmatchable)
            {
                WriteWarning(warningMessage);
            }

            provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
        }

        /// <summary>
        /// Generates resources in a C# source file from the specified XML resource file (.resx) to the specified path.
        /// </summary>
        /// <remarks>Only UTF-8 encoding is supported.</remarks>
        /// <param name="resxFileName">The XML resource file (.resx) to be read.</param>
        /// <param name="csharpFileName">The file to generate.</param>
        private void GenerateCSharpSourceFile(string resxFileName, string csharpFileName)
        {
            using FileStream stream = new(csharpFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new(stream, new UTF8Encoding(false));
            using CSharpCodeProvider provider = new();

            CodeCompileUnit compileUnit = StronglyTypedResourceBuilder.Create(
                resxFileName,
                TypeName,
                Namespace,
                provider,
                !PublicClass.IsPresent,
                out string[] unmatchable
            );

            foreach (string warningMessage in unmatchable)
            {
                WriteWarning(warningMessage);
            }

            provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
        }

        /// <summary>
        /// Generates resources in an XML resource file (.resx) to the specified path.
        /// </summary>
        /// <remarks>Only UTF-8 encoding is supported.</remarks>
        /// <param name="path">The file to generate.</param>
        private void GenerateResXResourceFile(string path)
        {
            using FileStream stream = new(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using StreamWriter streamWriter = new(stream, new UTF8Encoding(false));
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
        /// <param name="path">The path to resolve.</param>
        /// <returns>The fully qualified path of the <paramref name="path"/> parameter.</returns>
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
            catch (System.Management.Automation.DriveNotFoundException ex)
            {
                ErrorRecord errorRecord = new(ex.ErrorRecord, ex);
                ThrowTerminatingError(errorRecord);
            }

            path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out ProviderInfo provider, out _);

            if (!provider.Name.Equals(FileSystemProvider.ProviderName, StringComparison.Ordinal))
            {
                ErrorRecord errorRecord = new(
                    new InvalidOperationException(string.Format(Resources.NotFileSystemProvider, provider)),
                    "NotFileSystemProvider",
                    ErrorCategory.InvalidArgument,
                    path
                );
                ThrowTerminatingError(errorRecord);
            }

            return path;
        }
    }
}
