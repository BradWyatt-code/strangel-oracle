using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Domain.ValueObjects;

namespace StrangelOracle.Infrastructure.Services;

/// <summary>
/// Nok'so's engine - the falcon spirit
/// 
/// Sometimes protector, always disruptor.
/// He breaks what needs breaking.
/// 
/// Phase 2: Interruption system that can override other Strangel responses
/// </summary>
public sealed class NoksoEngine : IStrangelEngine
{
    public StrangelType StrangelType => StrangelType.Nokso;
    
    private static readonly string[] Disruptions =
    {
        "Something shatters that needed shattering. The pieces will make more sense than the whole.",
        "The falcon strikes. What you were holding falls. Your hands are free now.",
        "A pattern breaks. You've been walking in circles. Now there's a gap in the wall.",
        "He takes something from you. You won't miss it. You needed to lose it.",
        "The comfortable arrangement collapses. Good. It was suffocating you."
    };
    
    private static readonly string[] Protections =
    {
        "He circles overhead. Something that was coming toward you veers away.",
        "A door closes before you reach it. There was nothing good behind it.",
        "You stumble. Because you stumble, you miss what would have hit you.",
        "The falcon's shadow falls across your path. You stop. The danger passes.",
        "He screams once, sharp. The thing that was hunting you flinches. Leaves."
    };
    
    private static readonly string[] Refusals =
    {
        "The falcon watches but does not move. This is your work, not his.",
        "He could break this for you. He won't. You need to break it yourself.",
        "Nok'so turns away. What you asked for is not what you need."
    };
    
    public Task<Blessing> GenerateBlessingAsync(string? petition = null)
    {
        var random = new Random();
        
        // Nok'so decides: disrupt, protect, or refuse
        var actionRoll = random.NextDouble();
        
        if (actionRoll < 0.4)
        {
            // Disruption
            return Task.FromResult(Blessing.Create(
                source: StrangelType.Nokso,
                type: BlessingType.Disruption,
                intensity: BlessingIntensity.Strong,
                message: Disruptions[random.Next(Disruptions.Length)],
                secondaryMessage: "The falcon's eyes hold no apology. Only necessity."
            ));
        }
        else if (actionRoll < 0.75)
        {
            // Protection
            return Task.FromResult(Blessing.Create(
                source: StrangelType.Nokso,
                type: BlessingType.Blessing,
                intensity: BlessingIntensity.Present,
                message: Protections[random.Next(Protections.Length)],
                secondaryMessage: "He does not stay. He never stays. But he was there when it mattered."
            ));
        }
        else
        {
            // Refusal
            return Task.FromResult(Blessing.Create(
                source: StrangelType.Nokso,
                type: BlessingType.Silence,
                intensity: BlessingIntensity.Whisper,
                message: Refusals[random.Next(Refusals.Length)]
            ));
        }
    }
    
    public Task<bool> IsPresent()
    {
        // Nok'so appears suddenly, unpredictably
        var random = new Random();
        return Task.FromResult(random.NextDouble() > 0.3);
    }
    
    public Task<string> GetCurrentDisposition()
    {
        var random = new Random();
        var dispositions = new[]
        {
            "Sharp. Sudden. Watching from high places.",
            "Circling. Something has his attention.",
            "Still. The falcon considers whether to strike.",
            "Gone. But the memory of his presence lingers."
        };
        return Task.FromResult(dispositions[random.Next(dispositions.Length)]);
    }
}
