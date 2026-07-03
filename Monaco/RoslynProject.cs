using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

public class AssemblyMetadataHelper
{

    private HttpClient _httpClient = new HttpClient();
    private JsonDocument _blazorBootDocument = null;

    public AssemblyMetadataHelper(string uri)
    {
        _httpClient.BaseAddress = new Uri(uri);
    }
    public async Task<MetadataReference?> GetAssemblyMetadataReference(Assembly assembly)
    {
        return await GetAssemblyMetadataReference(assembly.GetName().Name ?? "");
    }
    public async Task<MetadataReference?> GetAssemblyMetadataReference(string assembly)
    {
        if (_blazorBootDocument == null)
        {
            var bootJsonResponse = await _httpClient.GetAsync("./_framework/blazor.boot.json");
            var bootJsonString = await bootJsonResponse.Content.ReadAsStringAsync();
            _blazorBootDocument = JsonDocument.Parse(bootJsonString);
        }

        MetadataReference? ret = null;
        var assemblyName = assembly/*.GetName().Name ?? ""*/;
        var assemblyUrl = $"./_framework/{GetDeploymentName(assemblyName + ".dll")}";
        try
        {
            Console.WriteLine($"Fetching assembly: {assemblyName}");

            var tmp = await _httpClient.GetAsync(assemblyUrl);
            if (tmp.IsSuccessStatusCode)
            {
                byte[]? bytes = null;
                //if (assemblyUrl.EndsWith(".gz"))
                //{
                //    var stream = await tmp.Content.ReadAsStreamAsync();
                //    bytes = await DecompressGzInMemory(stream);
                //}
                //else if (assemblyUrl.EndsWith(".br"))
                //{
                //    var stream = await tmp.Content.ReadAsStreamAsync();
                //    bytes = await DecompressBrInMemory(stream);
                //}
                //else
                //{
                bytes = await tmp.Content.ReadAsByteArrayAsync();
                //}

                if (assemblyName == "System.Runtime")
                {
                    var docProviderFetch = await _httpClient.GetAsync($"./System.Runtime.xml");
                    var docProviderBytes = await docProviderFetch.Content.ReadAsByteArrayAsync();
                    var documentationProvider = XmlDocumentationProvider.CreateFromBytes(docProviderBytes);
                    ret = MetadataReference.CreateFromImage(bytes, documentation: documentationProvider);
                }
                else
                {
                    ret = MetadataReference.CreateFromImage(bytes);
                }
                ret = MetadataReference.CreateFromImage(bytes);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching metadata {e.Message}");
        }
        return ret;
    }

    //private async Task<byte[]> DecompressGzInMemory(Stream stream)
    //{
    //    // Ensure the input stream is at position 0 if it supports seeking.
    //    if (stream.CanSeek)
    //        stream.Position = 0;

    //    using var gzip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
    //    using var ms = new MemoryStream();

    //    await gzip.CopyToAsync(ms).ConfigureAwait(false);

    //    return ms.ToArray();
    //}

    //private async Task<byte[]> DecompressBrInMemory(Stream stream)
    //{
    //    // Ensure the input stream is at position 0 when possible
    //    if (stream.CanSeek)
    //        stream.Position = 0;

    //    using var decompressed = new MemoryStream();
    //    using var brotli = new BrotliStream(stream, CompressionMode.Decompress, leaveOpen: true);

    //    await brotli.CopyToAsync(decompressed).ConfigureAwait(false);

    //    return decompressed.ToArray();
    //}

    private string GetDeploymentName(string originalDllName)
    {
        var root = _blazorBootDocument.RootElement;
        if (root.TryGetProperty("resources", out var resources) &&
            resources.TryGetProperty("fingerprinting", out var fingerprinting))
        {
            foreach (var property in fingerprinting.EnumerateObject())
            {
                if (property.Value.GetString() == originalDllName)
                {
                    return property.Name;
                }
            }
        }
        return $"NOT-FOUND-{originalDllName}"; // Return null if not found
    }
}


public class RoslynProject
{
    private List<Assembly> Assemblies = new List<Assembly>();
    private List<string> AssemblyNames = new List<string>();

    public static List<MetadataReference> MetadataReferences = new List<MetadataReference>();
    private string Uri { get; init; }
    public RoslynProject(string uri)
    {
        Uri = uri;

        // Assemblies we reference for metadata (do not trim)
        //Assemblies.Add(Assembly.GetExecutingAssembly());
        //Assemblies.Add(Assembly.Load("Microsoft.CSharp"));
        Assemblies.Add(Assembly.Load("System.Runtime"));
        Assemblies.Add(Assembly.Load("System.Collections"));
        Assemblies.Add(Assembly.Load("netstandard"));
        Assemblies.Add(Assembly.Load("System"));
        Assemblies.Add(Assembly.Load("System.Net.WebClient"));
        Assemblies.Add(Assembly.Load("System.Private.Uri"));
        Assemblies.Add(Assembly.Load("System.Text.RegularExpressions"));
        Assemblies.Add(Assembly.Load("System.Text.Json"));
        Assemblies.Add(Assembly.Load("System.Net.WebHeaderCollection"));
        Assemblies.Add(Assembly.Load("System.Collections.Specialized"));
        Assemblies.Add(Assembly.Load("System.Diagnostics.Process"));
        //Assemblies.Add(Assembly.Load("System.ComponentModel.EventBasedAsync"));
        Assemblies.Add(Assembly.Load("System.Collections.Concurrent"));
        Assemblies.Add(Assembly.Load("System.Memory"));
        //Assemblies.Add(Assembly.Load("System.IO.Compression"));
        //Assemblies.Add(Assembly.Load("System.IO.Compression.ZipFile"));
        //Assemblies.Add(typeof(Console).Assembly);

        Assemblies.Add(typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly);
        Assemblies.Add(typeof(List<>).Assembly);
        Assemblies.Add(typeof(DescriptionAttribute).Assembly);
        Assemblies.Add(typeof(Task).Assembly);
        Assemblies.Add(typeof(Enumerable).Assembly);
        Assemblies.Add(typeof(DataSet).Assembly);
        Assemblies.Add(typeof(XmlDocument).Assembly);
        Assemblies.Add(typeof(INotifyPropertyChanged).Assembly);
        Assemblies.Add(typeof(HttpClient).Assembly);
        Assemblies.Add(typeof(System.IO.Compression.ZipArchive).Assembly);
        Assemblies.Add(typeof(System.IO.Compression.ZipFile).Assembly);
        Assemblies.Add(typeof(System.IO.Compression.ZipArchiveEntry).Assembly);
        Assemblies.Add(typeof(System.Formats.Tar.TarFile).Assembly);
        Assemblies.Add(typeof(System.IO.Compression.GZipStream).Assembly);
        Assemblies.Add(typeof(System.Linq.Expressions.Expression).Assembly);
        Assemblies.Add(typeof(Docker.DotNet.DockerClient).Assembly);
        Assemblies.Add(typeof(XDocument).Assembly);

        AssemblyNames.Add("TCAdmin.SDK.Constants");
        AssemblyNames.Add("TCAdmin.SDK");
        AssemblyNames.Add("TCAdmin.GameHosting.SDK");
        AssemblyNames.Add("TCAdmin.Docker.SDK");
        AssemblyNames.Add("TCAdmin.Web.Shared");
        AssemblyNames.Add("TCAdmin.Scripting");
        AssemblyNames.Add("TCAdmin.Scripting.SDK");
        AssemblyNames.Add("TCAdmin.Monitor.SDK");

    }
    public async Task Init()
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        Workspace = new AdhocWorkspace(host);

        if (MetadataReferences.Count == 0)
        {
            var mh = new AssemblyMetadataHelper(Uri);

            var initializedAssemblies = new List<string>();
            AssemblyNames.AddRange(Assemblies.Select(x => x.GetName().Name));
            foreach (var a in AssemblyNames)
            {
                try
                {
                    var metadataReference = await mh.GetAssemblyMetadataReference(a);
                    if (metadataReference == null)
                    {
                        Console.WriteLine($"Did not get metadata ref {a}");
                        continue;
                    }
                    if (!initializedAssemblies.Contains(a))
                    {
                        MetadataReferences.Add(metadataReference);
                        initializedAssemblies.Add(a);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not add rdrf {e.Message}");
                }
            }
        }


        var projectInfo = ProjectInfo
            .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "IntelliSage", "IntelliSage", LanguageNames.CSharp)
            .WithMetadataReferences(MetadataReferences)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
            .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));

        var project = Workspace.AddProject(projectInfo);

        UseOnlyOnceDocument = Workspace.AddDocument(project.Id, "Code.cs", SourceText.From(string.Empty));
        DocumentId = UseOnlyOnceDocument.Id;

    }

    public AdhocWorkspace Workspace { get; set; }

    public Document UseOnlyOnceDocument { get; set; }

    public DocumentId DocumentId { get; set; }
}
