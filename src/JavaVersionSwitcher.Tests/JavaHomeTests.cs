using System;
using System.Threading.Tasks;
using JavaVersionSwitcher.Adapters;
using JavaVersionSwitcher.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace JavaVersionSwitcher.Tests;

public class JavaHomeTests
{
    private readonly JavaHomeAdapter _adapter = new JavaHomeAdapter(new Mock<ILogger>().Object);

    [Fact]
    public async Task Can_Set_per_process()
    {
        var expected = Guid.NewGuid().ToString("d");
        await _adapter.SetValue(expected, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.Process);

        actual.ShouldBe(expected);
    }
        
    [Fact]
    public async Task Set_per_process_does_not_modify_user()
    {
        var existing = await _adapter.GetValue(EnvironmentScope.User);
        var modified = existing + Guid.NewGuid().ToString("d");
        await _adapter.SetValue(modified, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.User);

        actual.ShouldNotBe(modified);
    }
        
    [Fact]
    public async Task Set_per_process_does_not_modify_machine()
    {
        var existing = await _adapter.GetValue(EnvironmentScope.Machine);
        var modified = existing + Guid.NewGuid().ToString("d");
        await _adapter.SetValue(modified, EnvironmentScope.Process);

        var actual = await _adapter.GetValue(EnvironmentScope.Machine);

        actual.ShouldNotBe(modified);
    }
}