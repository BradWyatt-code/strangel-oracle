using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.ValueObjects;

namespace StrangelOracle.Domain.Entities;

/// <summary>
/// A blessing, judgment, or response from a Strangel
/// </summary>
public sealed class Blessing
{
    public Guid Id { get; private set; }
    public StrangelType Source { get; private set; }
    public BlessingType Type { get; private set; }
    public BlessingIntensity Intensity { get; private set; }
    public string Message { get; private set; }
    public string? SecondaryMessage { get; private set; }
    public EmotionalResidue? Released { get; private set; }
    public DateTime BestowedAt { get; private set; }
    public TimeSpan Duration { get; private set; }
    
    private Blessing() { }
    
    public static Blessing Create(
        StrangelType source,
        BlessingType type,
        BlessingIntensity intensity,
        string message,
        string? secondaryMessage = null,
        EmotionalResidue? released = null)
    {
        return new Blessing
        {
            Id = Guid.NewGuid(),
            Source = source,
            Type = type,
            Intensity = intensity,
            Message = message,
            SecondaryMessage = secondaryMessage,
            Released = released,
            BestowedAt = DateTime.UtcNow,
            Duration = CalculateDuration(intensity)
        };
    }
    
    private static TimeSpan CalculateDuration(BlessingIntensity intensity)
    {
        // Blessings last longer the more intense they are
        var minutes = intensity.Value * 60 + 5; // 5-65 minutes
        return TimeSpan.FromMinutes(minutes);
    }
    
    public bool IsActive => DateTime.UtcNow < BestowedAt + Duration;
    
    public double RemainingStrength
    {
        get
        {
            if (!IsActive) return 0;
            var elapsed = DateTime.UtcNow - BestowedAt;
            var remaining = 1 - (elapsed.TotalSeconds / Duration.TotalSeconds);
            return remaining * Intensity.Value;
        }
    }
}
