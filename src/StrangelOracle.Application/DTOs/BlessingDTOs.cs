using StrangelOracle.Domain.Enums;

namespace StrangelOracle.Application.DTOs;

/// <summary>
/// Response DTO for a blessing from a Strangel
/// </summary>
public sealed record BlessingResponse
{
    public Guid Id { get; init; }
    public string Strangel { get; init; } = string.Empty;
    public string StrangelTitle { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double Intensity { get; init; }
    public string IntensityDescription { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? SecondaryMessage { get; init; }
    public string? ReleasedEssence { get; init; }
    public DateTime BestowedAt { get; init; }
    public int DurationMinutes { get; init; }
    public bool IsActive { get; init; }
    public double RemainingStrength { get; init; }
}

/// <summary>
/// Request DTO for seeking a blessing
/// </summary>
public sealed record BlessingRequest
{
    /// <summary>
    /// The Strangel to approach
    /// </summary>
    public StrangelType Strangel { get; init; }
    
    /// <summary>
    /// Optional petition, confession, or question
    /// Not all Strangels require or accept this
    /// </summary>
    public string? Petition { get; init; }
}

/// <summary>
/// Information about a Strangel
/// </summary>
public sealed record StrangelInfo
{
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Aspect { get; init; } = string.Empty;
    public string Function { get; init; } = string.Empty;
    public string Disposition { get; init; } = string.Empty;
    public string[] Domains { get; init; } = Array.Empty<string>();
    public string[] Manifestations { get; init; } = Array.Empty<string>();
    public string RitualInstruction { get; init; } = string.Empty;
    public bool IsPresent { get; init; }
    public string CurrentMood { get; init; } = string.Empty;
}

/// <summary>
/// Presence information for all Strangels
/// </summary>
public sealed record OraclePresence
{
    public DateTime Timestamp { get; init; }
    public Dictionary<string, bool> StrangelPresence { get; init; } = new();
    public string AmbientMood { get; init; } = string.Empty;
}
