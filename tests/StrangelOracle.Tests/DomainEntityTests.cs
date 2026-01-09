using FluentAssertions;
using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.ValueObjects;
using Xunit;

namespace StrangelOracle.Tests;

public class DomainEntityTests
{
    [Fact]
    public void BlessingIntensity_ShouldThrowForInvalidValues()
    {
        var action = () => BlessingIntensity.Create(-0.1);
        action.Should().Throw<ArgumentOutOfRangeException>();
        
        var action2 = () => BlessingIntensity.Create(1.1);
        action2.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void BlessingIntensity_ShouldAcceptValidValues()
    {
        var intensity = BlessingIntensity.Create(0.5);
        intensity.Value.Should().Be(0.5);
    }
    
    [Fact]
    public void BlessingIntensity_ToDescription_ShouldReturnAppropriateDescription()
    {
        BlessingIntensity.Whisper.ToDescription().Should().Contain("barely");
        BlessingIntensity.Gentle.ToDescription().Should().Contain("gentle");
        BlessingIntensity.Present.ToDescription().Should().Contain("present");
        BlessingIntensity.Strong.ToDescription().Should().Contain("strong");
        BlessingIntensity.Overwhelming.ToDescription().Should().Contain("overwhelming");
    }
    
    [Fact]
    public void EmotionalResidue_Generate_ShouldCreateValidResidue()
    {
        var residue = EmotionalResidue.Generate();
        
        residue.Essence.Should().NotBeNullOrEmpty();
        residue.Weight.Should().BeGreaterOrEqualTo(0.3);
        residue.Weight.Should().BeLessOrEqualTo(0.8);
        residue.AccumulatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public void Blessing_Create_ShouldGenerateUniqueId()
    {
        var blessing1 = Blessing.Create(
            StrangelType.WomanWithHeart,
            BlessingType.Blessing,
            BlessingIntensity.Gentle,
            "Test message");
            
        var blessing2 = Blessing.Create(
            StrangelType.WomanWithHeart,
            BlessingType.Blessing,
            BlessingIntensity.Gentle,
            "Test message");
            
        blessing1.Id.Should().NotBe(blessing2.Id);
    }
    
    [Fact]
    public void Blessing_IsActive_ShouldBeTrueWhenNew()
    {
        var blessing = Blessing.Create(
            StrangelType.WomanWithHeart,
            BlessingType.Blessing,
            BlessingIntensity.Gentle,
            "Test message");
            
        blessing.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void Blessing_RemainingStrength_ShouldDecreaseOverTime()
    {
        var blessing = Blessing.Create(
            StrangelType.WomanWithHeart,
            BlessingType.Blessing,
            BlessingIntensity.Present,
            "Test message");
            
        var initialStrength = blessing.RemainingStrength;
        
        // Strength should be close to intensity when new
        initialStrength.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void Strangel_GetByType_ShouldReturnCorrectStrangel()
    {
        var womanWithHeart = Strangel.GetByType(StrangelType.WomanWithHeart);
        womanWithHeart.Name.Should().Be("The Woman with Heart");
        
        var fox = Strangel.GetByType(StrangelType.Fox);
        fox.Name.Should().Be("The Fox");
        
        var furies = Strangel.GetByType(StrangelType.Furies);
        furies.Name.Should().Be("The Furies");
        
        var nokso = Strangel.GetByType(StrangelType.Nokso);
        nokso.Name.Should().Be("Nok'so");
    }
    
    [Fact]
    public void Strangel_ShouldHaveRequiredProperties()
    {
        foreach (var type in Enum.GetValues<StrangelType>())
        {
            var strangel = Strangel.GetByType(type);
            
            strangel.Name.Should().NotBeNullOrEmpty();
            strangel.Title.Should().NotBeNullOrEmpty();
            strangel.Aspect.Should().NotBeNullOrEmpty();
            strangel.Function.Should().NotBeNullOrEmpty();
            strangel.Disposition.Should().NotBeNullOrEmpty();
            strangel.Domains.Should().NotBeEmpty();
            strangel.Manifestations.Should().NotBeEmpty();
            strangel.RitualInstruction.Should().NotBeNullOrEmpty();
        }
    }
}
