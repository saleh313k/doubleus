using System.Xml.Linq;

namespace DoubleUs.ArchitectureTests;

/// <summary>
/// Enforces the dependency rule from docs/architecture/SYSTEM_ARCHITECTURE.md by reading the real
/// project files (compiled-assembly inspection would miss references that are not yet used in code).
/// Domain -> nothing; Application -> Domain; Infrastructure/Integrations/AI -> Application only;
/// API and Worker are composition roots and may reference any of the above but never each other.
/// </summary>
public class ProjectReferenceRulesTests
{
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        ["DoubleUs.Domain"] = [],
        ["DoubleUs.Application"] = ["DoubleUs.Domain"],
        ["DoubleUs.Infrastructure"] = ["DoubleUs.Application"],
        ["DoubleUs.Integrations"] = ["DoubleUs.Application"],
        ["DoubleUs.AI"] = ["DoubleUs.Application"],
        ["DoubleUs.API"] = ["DoubleUs.Application", "DoubleUs.Infrastructure", "DoubleUs.Integrations", "DoubleUs.AI"],
        ["DoubleUs.Worker"] = ["DoubleUs.Application", "DoubleUs.Infrastructure", "DoubleUs.Integrations", "DoubleUs.AI"],
    };

    public static TheoryData<string> Projects => new(Allowed.Keys);

    [Theory]
    [MemberData(nameof(Projects))]
    public void Project_references_only_allowed_projects(string project)
    {
        var actual = ReadProjectReferences(project);

        var unexpected = actual.Except(Allowed[project]).ToArray();
        Assert.True(
            unexpected.Length == 0,
            $"{project} must not reference: {string.Join(", ", unexpected)}");
    }

    [Fact]
    public void Every_source_project_is_covered_by_the_rules()
    {
        var onDisk = Directory.GetDirectories(Path.Combine(RepoRoot(), "src"))
            .Select(d => Path.GetFileName(d)!)
            .OrderBy(n => n, StringComparer.Ordinal);

        Assert.Equal(Allowed.Keys.OrderBy(n => n, StringComparer.Ordinal), onDisk);
    }

    [Fact]
    public void No_source_project_declares_package_versions_inline()
    {
        foreach (var csproj in Directory.GetFiles(Path.Combine(RepoRoot(), "src"), "*.csproj", SearchOption.AllDirectories))
        {
            var withVersion = XDocument.Load(csproj)
                .Descendants("PackageReference")
                .Where(e => e.Attribute("Version") is not null)
                .Select(e => (string?)e.Attribute("Include"));

            Assert.Empty(withVersion);
        }
    }

    private static string[] ReadProjectReferences(string project)
    {
        var csproj = Path.Combine(RepoRoot(), "src", project, project + ".csproj");
        return XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string)e.Attribute("Include")!).Replace('\\', '/')))
            .ToArray();
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "DoubleUs.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("DoubleUs.sln not found above the test output directory.");
    }
}
