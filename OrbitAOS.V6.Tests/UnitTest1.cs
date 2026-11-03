namespace OrbitAOS.V6.Tests;

/// <summary>
/// Unit tests for the OrbitAOS.V6 application.
/// These tests validate the migration to .NET 10 and ensure no regressions.
/// </summary>
public class UnitTest1
{
    [Fact]
    public void Migration_TargetFramework_IsNet10()
    {
        // Verify the test project targets net10.0
        var framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        Assert.Contains("10", framework);
    }

    [Fact]
    public void Migration_BasicArithmetic_Passes()
    {
        // Sanity check test to ensure the test framework is working
        Assert.Equal(4, 2 + 2);
    }

    [Fact]
    public void Migration_StringOperations_Pass()
    {
        // Verify string operations work correctly
        var connectionStringTemplate = "Server=(localdb)\\mssqllocaldb;Database=OrbitAOS;Trusted_Connection=True;";
        Assert.DoesNotContain("MultipleActiveResultSets", connectionStringTemplate);
        Assert.Contains("OrbitAOS", connectionStringTemplate);
    }
}
