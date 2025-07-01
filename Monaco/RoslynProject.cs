using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

public class AssemblyMetadataHelper
{

    private HttpClient _httpClient = new HttpClient();


    public AssemblyMetadataHelper(string uri)
    {
        _httpClient.BaseAddress = new Uri(uri);
    }
    public async Task<MetadataReference?> GetAssemblyMetadataReference(Assembly assembly)
    {
        MetadataReference? ret = null;
        var assemblyName = assembly.GetName().Name ?? "";
        var assemblyUrl = $"./_framework/{assemblyName}.dll";
        try
        {
            var tmp = await _httpClient.GetAsync(assemblyUrl);
            if (tmp.IsSuccessStatusCode)
            {
                var bytes = await tmp.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"Fetching assembly: {assemblyName}");
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
}


public class RoslynProject
{
    private List<Assembly> Assemblies = new List<Assembly>();
    public static List<MetadataReference> MetadataReferences = new List<MetadataReference>();
    private string Uri { get; init; }
    public RoslynProject(string uri)
    {
        Uri = uri;

        // Assemblies we reference for metadata
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
        Assemblies.Add(Assembly.Load("System.ComponentModel.EventBasedAsync"));
        Assemblies.Add(Assembly.Load("System.Collections.Concurrent"));
        Assemblies.Add(Assembly.Load("System.Memory"));
        //Assemblies.Add(Assembly.Load("System.IO.Compression"));
        //Assemblies.Add(Assembly.Load("System.IO.Compression.ZipFile"));
        //Assemblies.Add(typeof(Console).Assembly);

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
        Assemblies.Add(typeof(TCAdmin.SDK.Constants.BuiltInRoles).Assembly);
        Assemblies.Add(typeof(TCAdmin.SDK.Utility).Assembly);
        Assemblies.Add(typeof(TCAdmin.SDK.GameHosting.GameHostingModule).Assembly);
        Assemblies.Add(typeof(TCAdmin.Web.Shared.Api.Attributes.ApiRouteAttribute).Assembly);
        Assemblies.Add(typeof(TCAdmin.Scripting.ScriptEngineManager).Assembly);
        Assemblies.Add(typeof(XDocument).Assembly);
    }

    public async Task Init()
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        Workspace = new AdhocWorkspace(host);

        if (MetadataReferences.Count == 0)
        {
            var mh = new AssemblyMetadataHelper(Uri);

            var initializedAssemblies = new List<string>();
            foreach (var a in Assemblies)
            {
                try
                {
                    var metadataReference = await mh.GetAssemblyMetadataReference(a);
                    if (metadataReference == null)
                    {
                        Console.WriteLine($"Did not get metadata ref {a.FullName}");
                        continue;
                    }
                    if (!initializedAssemblies.Contains(a.FullName))
                    {
                        MetadataReferences.Add(metadataReference);
                        initializedAssemblies.Add(a.FullName);
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
