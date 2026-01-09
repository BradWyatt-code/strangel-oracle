namespace StrangelOracle.Domain.ValueObjects;

/// <summary>
/// Represents the intensity of a blessing, from barely perceptible to overwhelming
/// </summary>
public sealed record BlessingIntensity
{
    public double Value { get; }
    
    private BlessingIntensity(double value)
    {
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Intensity must be between 0 and 1");
        Value = value;
    }
    
    public static BlessingIntensity Create(double value) => new(value);
    
    public static BlessingIntensity Whisper => new(0.1);
    public static BlessingIntensity Gentle => new(0.3);
    public static BlessingIntensity Present => new(0.5);
    public static BlessingIntensity Strong => new(0.7);
    public static BlessingIntensity Overwhelming => new(0.9);
    
    /// <summary>
    /// The Woman with Heart's blessing is always gentle but constant
    /// </summary>
    public static BlessingIntensity HeartTouch => new(0.4);
    
    public string ToDescription() => Value switch
    {
        < 0.2 => "barely perceptible",
        < 0.4 => "gentle, like a held breath",
        < 0.6 => "present and undeniable",
        < 0.8 => "strong, almost too much",
        _ => "overwhelming, the edges of self blur"
    };
}
