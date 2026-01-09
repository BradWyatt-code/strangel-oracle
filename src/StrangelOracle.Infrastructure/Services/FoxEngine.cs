using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Domain.ValueObjects;

namespace StrangelOracle.Infrastructure.Services;

/// <summary>
/// The Fox's blessing engine (Murat Askarov, the possessed)
/// 
/// Unpredictable. Sometimes helps, sometimes doesn't.
/// His responses are riddles, his blessings are gambles.
/// 
/// Phase 2: Full implementation with entropy-weighted responses
/// </summary>
public sealed class FoxEngine : IStrangelEngine
{
    public StrangelType StrangelType => StrangelType.Fox;
    
    private static readonly string[] Responses =
    {
        "In my country, foxes walk like men. Here, men run like foxes. So who's the animal, you think?",
        "You want blessing? Blessing is for those who don't ask. You asked. This changes things.",
        "I see what you carry. It's heavy, yes? But is it yours? Maybe you picked up someone else's bag.",
        "The answer is yes. Or no. Depends on what you do next. I only see the crossroads, not the choice.",
        "My passenger laughs. This is good sign. Or very bad. Hard to tell with foxes.",
        "You came to me because you want permission. I don't give permission. I give... possibilities.",
        "Three paths: one safe, one fast, one true. I won't tell you which is which. More fun this way."
    };
    
    private static readonly string[] Refusals =
    {
        "Not today. The fox sleeps. Come back when you have something interesting.",
        "You smell like certainty. The fox doesn't like certainty. Try again with more doubt.",
        "I could help. I won't. Not because I can't. Because you need to find this yourself."
    };
    
    public Task<Blessing> GenerateBlessingAsync(string? petition = null)
    {
        var random = new Random();
        
        // The Fox is unpredictable - 70% help, 30% refuse
        var willHelp = random.NextDouble() > 0.3;
        
        if (!willHelp)
        {
            return Task.FromResult(Blessing.Create(
                source: StrangelType.Fox,
                type: BlessingType.Silence,
                intensity: BlessingIntensity.Whisper,
                message: Refusals[random.Next(Refusals.Length)]
            ));
        }
        
        // Decide between blessing and trick
        var isTrick = random.NextDouble() < 0.2;
        
        return Task.FromResult(Blessing.Create(
            source: StrangelType.Fox,
            type: isTrick ? BlessingType.Trick : BlessingType.Blessing,
            intensity: BlessingIntensity.Create(random.NextDouble() * 0.5 + 0.3),
            message: Responses[random.Next(Responses.Length)],
            secondaryMessage: isTrick ? "The fox's eyes flash amber. Was that help or mischief? You won't know until later." : null
        ));
    }
    
    public Task<bool> IsPresent()
    {
        // The Fox moves at night
        var hour = DateTime.Now.Hour;
        return Task.FromResult(hour >= 20 || hour <= 5);
    }
    
    public Task<string> GetCurrentDisposition()
    {
        var random = new Random();
        var dispositions = new[]
        {
            "Curious. Watching. The passenger stirs.",
            "Playful and dangerous. A good night for riddles.",
            "Quiet. The fox considers whether to engage.",
            "Alert. Something has caught his attention."
        };
        return Task.FromResult(dispositions[random.Next(dispositions.Length)]);
    }
}
