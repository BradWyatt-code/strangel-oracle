using FluentAssertions;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Infrastructure.Services;
using Xunit;

namespace StrangelOracle.Tests;

public class WomanWithHeartEngineTests
{
    private readonly WomanWithHeartEngine _engine;
    
    public WomanWithHeartEngineTests()
    {
        _engine = new WomanWithHeartEngine();
    }
    
    [Fact]
    public void StrangelType_ShouldBeWomanWithHeart()
    {
        _engine.StrangelType.Should().Be(StrangelType.WomanWithHeart);
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldAlwaysReturnBlessing()
    {
        // The Woman with Heart only blesses - she cannot judge or refuse
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.Type.Should().Be(BlessingType.Blessing);
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldAlwaysReleaseEmotionalResidue()
    {
        // She always releases something when touched
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.Released.Should().NotBeNull();
        blessing.Released!.Essence.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldIgnorePetition()
    {
        // She doesn't read petitions - she blesses because she must
        var withPetition = await _engine.GenerateBlessingAsync("Please help me");
        var withoutPetition = await _engine.GenerateBlessingAsync();
        
        // Both should return valid blessings
        withPetition.Type.Should().Be(BlessingType.Blessing);
        withoutPetition.Type.Should().Be(BlessingType.Blessing);
    }
    
    [Fact]
    public async Task IsPresent_ShouldAlwaysReturnTrue()
    {
        // She is always present. That is her nature and her burden.
        var isPresent = await _engine.IsPresent();
        
        isPresent.Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_IntensityShouldBeInValidRange()
    {
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.Intensity.Value.Should().BeGreaterOrEqualTo(0.2);
        blessing.Intensity.Value.Should().BeLessOrEqualTo(0.8);
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldHaveMessage()
    {
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.Message.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldHaveSecondaryMessage()
    {
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.SecondaryMessage.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetCurrentDisposition_ShouldReturnNonEmptyString()
    {
        var disposition = await _engine.GetCurrentDisposition();
        
        disposition.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_BlessingShouldBeActive()
    {
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public async Task GenerateBlessingAsync_ShouldHavePositiveDuration()
    {
        var blessing = await _engine.GenerateBlessingAsync();
        
        blessing.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
