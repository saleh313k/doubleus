using System.Reflection;

namespace DoubleUs.UnitTests;

public class AssemblyMarkerTests
{
    public static TheoryData<string, Assembly> Markers => new()
    {
        { "Domain", Domain.AssemblyMarker.Assembly },
        { "Application", Application.AssemblyMarker.Assembly },
        { "Infrastructure", Infrastructure.AssemblyMarker.Assembly },
        { "Integrations", Integrations.AssemblyMarker.Assembly },
        { "AI", AI.AssemblyMarker.Assembly },
    };

    [Theory]
    [MemberData(nameof(Markers))]
    public void Each_module_assembly_has_the_expected_name(string module, Assembly assembly)
    {
        Assert.Equal($"DoubleUs.{module}", assembly.GetName().Name);
    }
}
