using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Domain.ValueObjects;

namespace StrangelOracle.Infrastructure.Services;

/// <summary>
/// The Woman with Heart's blessing engine
/// 
/// She does not speak. She does not choose. She receives and releases.
/// Her blessing is passive, constant, and cannot be refused.
/// </summary>
public sealed class WomanWithHeartEngine : IStrangelEngine
{
    public StrangelType StrangelType => StrangelType.WomanWithHeart;
    
    // The messages she gives are not words - they are sensations
    private static readonly string[] BlessingMessages = 
    {
        "Something loosens in your chest. You didn't know it was tight.",
        "For a moment, grief organizes itself. It will scatter again, but not yet.",
        "You breathe deeper. You hadn't noticed the shallowness.",
        "Love feels possible again. Not easy. But possible.",
        "A warmth spreads where coldness had settled without your permission.",
        "You remember being held. The memory doesn't hurt this time.",
        "The weight you carry shifts. It's still there, but distributed differently.",
        "Permission arrives from nowhere: to grieve, to rest, to continue.",
        "Something you abandoned long ago is returned, lighter than you left it.",
        "The edges of your exhaustion soften. You can bear another hour."
    };
    
    private static readonly string[] SecondaryMessages =
    {
        "She is lighter now. For a moment.",
        "What she released was yours once. You left it somewhere.",
        "This is not healing. This is load-bearing.",
        "She will need to do this again. She always does.",
        "The surplus flows from her to you. It has to go somewhere.",
        "You were never meant to carry this alone. Neither was she.",
        "Her heart pulses once, visible even through the photograph.",
        "Somewhere, a waiting room feels briefly less like purgatory.",
        "This is what she was made for. It does not make it easier."
    };
    
    private static readonly string[] Dispositions =
    {
        "Quietly radiant. The burden is manageable today.",
        "Slightly dimmer than usual. She has absorbed much.",
        "Present and steady. The heart pulses at a resting rate.",
        "Near overflow. Her blessing will be stronger but cost her more.",
        "Inexhaustibly burdened, as always. As always, she continues."
    };
    
    public Task<Blessing> GenerateBlessingAsync(string? petition = null)
    {
        // The Woman with Heart doesn't read petitions
        // She blesses because she must, not because you asked
        
        var random = new Random();
        var message = BlessingMessages[random.Next(BlessingMessages.Length)];
        var secondary = SecondaryMessages[random.Next(SecondaryMessages.Length)];
        
        // She always releases something when touched
        var released = EmotionalResidue.Generate();
        
        var blessing = Blessing.Create(
            source: StrangelType.WomanWithHeart,
            type: BlessingType.Blessing, // She only blesses. She cannot judge or refuse.
            intensity: CalculateIntensity(),
            message: message,
            secondaryMessage: secondary,
            released: released
        );
        
        return Task.FromResult(blessing);
    }
    
    public Task<bool> IsPresent()
    {
        // She is always present. That is her nature and her burden.
        // But she is strongest at certain times.
        var hour = DateTime.Now.Hour;
        
        // Strongest at thresholds: late night, golden hour
        var strongHours = new[] { 3, 4, 5, 17, 18, 19, 23, 0 };
        
        return Task.FromResult(true); // Always present
    }
    
    public Task<string> GetCurrentDisposition()
    {
        var random = new Random();
        
        // Her disposition varies with the weight she carries
        var hour = DateTime.Now.Hour;
        
        // She's more burdened after peak emotional hours
        var heavyHours = new[] { 2, 3, 4, 14, 15, 22, 23 };
        
        if (heavyHours.Contains(hour))
        {
            return Task.FromResult("Near overflow. Her blessing will be stronger but cost her more.");
        }
        
        return Task.FromResult(Dispositions[random.Next(Dispositions.Length)]);
    }
    
    private static BlessingIntensity CalculateIntensity()
    {
        var random = new Random();
        var hour = DateTime.Now.Hour;
        
        // Base intensity for touch blessing
        var baseIntensity = 0.4;
        
        // Golden hour bonus (5-7 PM)
        if (hour >= 17 && hour <= 19)
        {
            baseIntensity += 0.15;
        }
        
        // Deep night bonus (2-5 AM)
        if (hour >= 2 && hour <= 5)
        {
            baseIntensity += 0.2;
        }
        
        // Add some variance
        var variance = (random.NextDouble() - 0.5) * 0.2;
        var finalIntensity = Math.Clamp(baseIntensity + variance, 0.2, 0.8);
        
        return BlessingIntensity.Create(finalIntensity);
    }
}
