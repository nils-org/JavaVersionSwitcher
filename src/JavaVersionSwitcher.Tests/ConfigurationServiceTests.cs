using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JavaVersionSwitcher.Tests.Fixtures;
using Shouldly;
using Xunit;

namespace JavaVersionSwitcher.Tests;

public class ConfigurationServiceTests
{
    [Fact]
    public async Task SetConfiguration_throws_on_wrong_provider()
    {
        // arrange
        using var fixture = new ConfigurationServiceFixture();
        const string providerName = "non-existent-provider";

        // act
        // ReSharper disable once AccessToDisposedClosure
        async Task Act() => await fixture.Service.SetConfiguration(providerName, null, null);

        // assert
        (await Should.ThrowAsync<KeyNotFoundException>((Func<Task>)Act))
            .Message
            .ShouldSatisfyAllConditions(
                m => m.ShouldStartWith("No ConfigurationProvider"),
                m => m.ShouldContain(providerName));
    }
        
    [Fact]
    public async Task SetConfiguration_throws_on_wrong_setting()
    {
        // arrange
        const string providerName = "provider";
        using var fixture = new ConfigurationServiceFixture();
        fixture.WithConfigurationProvider(providerName);
        const string setting = "non-existent-setting";

        // act'
        // ReSharper disable once AccessToDisposedClosure
        async Task Act() => await fixture.Service.SetConfiguration(providerName, setting, null);

        // assert
        (await Should.ThrowAsync<KeyNotFoundException>((Func<Task>)Act))
            .Message
            .ShouldSatisfyAllConditions(
                m => m.ShouldStartWith("No Configuration with the name"),
                m => m.ShouldContain(setting));
    }
        
    [Fact]
    public async Task SetConfiguration_writes_value_to_xml()
    {
        // arrange
        const string providerName = "pName";
        const string settingsName = "settingsName";
        const string value = "a value";
        using var fixture = new ConfigurationServiceFixture();
        fixture.WithConfigurationProvider(providerName, settingsName);

        // act'
        await fixture.Service.SetConfiguration(providerName, settingsName, value);

        // assert
        var xml = fixture.ReadXml(providerName, settingsName);
        xml.Value.ShouldBe(value);
    }
        
    [Fact]
    public async Task GetConfiguration_returns_empty_for_not_set_setting()
    {
        // arrange
        const string providerName = "pName";
        const string settingsName = "settingsName";
        using var fixture = new ConfigurationServiceFixture();
        fixture.WithConfigurationProvider(providerName, settingsName);

        // act'
        var actual = await fixture.Service.GetConfiguration(providerName, settingsName);

        // assert
        actual.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task GetConfiguration_returns_the_value_from_xml()
    {
        // arrange
        const string providerName = "pName";
        const string settingsName = "settingsName";
        const string expected = "some value";
        using var fixture = new ConfigurationServiceFixture();
        fixture.WithConfigurationProvider(providerName, settingsName);
        fixture.EnsureSetting(providerName, settingsName, expected);

        // act'
        var actual = await fixture.Service.GetConfiguration(providerName, settingsName);

        // assert
        actual.ShouldBe(expected);
    }
        
    [Fact]
    public async Task SetConfiguration_removes_empty_settings()
    {
        // arrange
        const string providerName = "pName";
        const string settingsName = "settingsName";
        using var fixture = new ConfigurationServiceFixture();
        fixture.WithConfigurationProvider(providerName, settingsName);
        fixture.EnsureSetting(providerName, settingsName, "some value");

        // act'
        await fixture.Service.SetConfiguration(providerName, settingsName, null);

        // assert
        var xml = fixture.ReadXml();
        xml.Parent.ShouldBeNull("this should be the root node.");
        xml.Elements().Count().ShouldBe(0);
    }
}