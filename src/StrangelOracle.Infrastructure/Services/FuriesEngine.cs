using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Domain.ValueObjects;

namespace StrangelOracle.Infrastructure.Services;

/// <summary>
/// The Furies' judgment engine (Alecto, Megaera, Tisiphone)
/// 
/// They appear as seagulls now. They judge all the same.
/// Three voices, one verdict.
/// 
/// Phase 2: Sentiment analysis for moral weight calculation
/// </summary>
public sealed class FuriesEngine : IStrangelEngine
{
    public StrangelType StrangelType => StrangelType.Furies;
    
    private static readonly string[] AlectoJudgments =
    {
        "ALECTO speaks: Your anger is justified. But justification is not absolution.",
        "ALECTO speaks: You buried your rage. We can smell where you buried it.",
        "ALECTO speaks: The fury you feel is borrowed. Return it to its source."
    };
    
    private static readonly string[] MegaeraJudgments =
    {
        "MEGAERA speaks: You wanted what wasn't yours. The wanting itself was the sin.",
        "MEGAERA speaks: Envy is a mirror. What you covet in others lives dormant in you.",
        "MEGAERA speaks: You compare yourself to shadows. Stop measuring against ghosts."
    };
    
    private static readonly string[] TisiphoneJudgments =
    {
        "TISIPHONE speaks: What you did cannot be undone. But its weight can be carried differently.",
        "TISIPHONE speaks: The debt exists. We are not here to collect, only to remind.",
        "TISIPHONE speaks: Guilt without action is self-indulgence. What will you do?"
    };
    
    private static readonly string[] UnitedJudgments =
    {
        "WE SPEAK AS ONE: You came seeking judgment. Here it is: you are not as guilty as you fear, nor as innocent as you hope.",
        "WE SPEAK AS ONE: The scales balance differently than you expected. Live with the true weight.",
        "WE SPEAK AS ONE: Your conscience brought you here. We have nothing to add to what it already knows."
    };
    
    public Task<Blessing> GenerateBlessingAsync(string? petition = null)
    {
        var random = new Random();
        
        // Choose which Fury speaks, or if they speak together
        var speakerRoll = random.NextDouble();
        
        string message;
        if (speakerRoll < 0.25)
            message = AlectoJudgments[random.Next(AlectoJudgments.Length)];
        else if (speakerRoll < 0.5)
            message = MegaeraJudgments[random.Next(MegaeraJudgments.Length)];
        else if (speakerRoll < 0.75)
            message = TisiphoneJudgments[random.Next(TisiphoneJudgments.Length)];
        else
            message = UnitedJudgments[random.Next(UnitedJudgments.Length)];
        
        return Task.FromResult(Blessing.Create(
            source: StrangelType.Furies,
            type: BlessingType.Judgment,
            intensity: BlessingIntensity.Strong,
            message: message,
            secondaryMessage: "The seagulls wheel overhead. They will be watching."
        ));
    }
    
    public Task<bool> IsPresent()
    {
        // The Furies are always watching
        return Task.FromResult(true);
    }
    
    public Task<string> GetCurrentDisposition()
    {
        var random = new Random();
        var dispositions = new[]
        {
            "Circling. Watching. The verdict forms slowly.",
            "Restless. Something has drawn their attention.",
            "Still. The judgment has already been made. You just don't know it yet.",
            "Thunder in the skull. They are close."
        };
        return Task.FromResult(dispositions[random.Next(dispositions.Length)]);
    }
}
