using System;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using Shouldly;
using Xunit;

namespace JavaVersionSwitcher.Tests;

public class PathTests
{
    private readonly PathAdapter _adapter = new PathAdapter();

    [Fact]
    public async Task Can_Set_per_process()
    {
        var expected = new []{ Guid.NewGuid().ToString("d") };
        await _adapter.SetValue(expected, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.Process);

        actual.ShouldBe(expected);
    }
        
    [Fact]
    public async Task Set_per_process_does_not_modify_user()
    {
        var modified = (await _adapter.GetValue(EnvironmentScope.User)).ToList();
        modified.Add(Guid.NewGuid().ToString("d"));
        await _adapter.SetValue(modified, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.User);

        actual.Count().ShouldBe(modified.Count - 1);
    }
        
    [Fact]
    public async Task Set_per_process_does_not_modify_machine()
    {
        var modified = (await _adapter.GetValue(EnvironmentScope.Machine)).ToList();
        modified.Add(Guid.NewGuid().ToString("d"));
        await _adapter.SetValue(modified, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.Machine);

        actual.Count().ShouldBe(modified.Count - 1);
    }
}