namespace StrangelOracle.Domain.ValueObjects;

/// <summary>
/// The emotional surplus that accumulates in cities and souls
/// The Woman with Heart absorbs and redistributes this
/// </summary>
public sealed record EmotionalResidue
{
    public string Essence { get; }
    public double Weight { get; }
    public DateTime AccumulatedAt { get; }
    
    private static readonly string[] Essences = 
    {
        "grief left in waiting rooms",
        "love that outlived its welcome",
        "hope abandoned at thresholds",
        "faith shed like old skin",
        "tenderness no one claimed",
        "sincerity punished into silence",
        "devotion that exceeded its object",
        "belief outgrown but not forgotten",
        "joy too fragile to keep",
        "longing that found no home"
    };
    
    private EmotionalResidue(string essence, double weight, DateTime accumulatedAt)
    {
        Essence = essence;
        Weight = weight;
        AccumulatedAt = accumulatedAt;
    }
    
    public static EmotionalResidue Generate()
    {
        var random = new Random();
        var essence = Essences[random.Next(Essences.Length)];
        var weight = random.NextDouble() * 0.5 + 0.3; // Between 0.3 and 0.8
        return new EmotionalResidue(essence, weight, DateTime.UtcNow);
    }
    
    public static EmotionalResidue FromEssence(string essence) =>
        new(essence, 0.5, DateTime.UtcNow);
}
