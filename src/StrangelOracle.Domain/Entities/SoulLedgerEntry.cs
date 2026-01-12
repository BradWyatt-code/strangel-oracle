using StrangelOracle.Domain.Enums;

namespace StrangelOracle.Domain.Entities;

/// <summary>
/// A permanent record of a blessing or judgment bestowed upon a seeker.
/// The Soul Ledger accumulates across all interactions—nothing is forgotten.
/// </summary>
public sealed class SoulLedgerEntry
{
    public Guid Id { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    public StrangelType Strangel { get; private set; }
    public string? Petition { get; private set; }
    public string Response { get; private set; } = string.Empty;
    public BlessingOutcome Outcome { get; private set; }
    public double Intensity { get; private set; }
    public DateTime BestowedAt { get; private set; }
    
    // For EF Core
    private SoulLedgerEntry() { }
    
    public static SoulLedgerEntry Create(
        string sessionId,
        StrangelType strangel,
        string? petition,
        string response,
        BlessingOutcome outcome,
        double intensity)
    {
        return new SoulLedgerEntry
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Strangel = strangel,
            Petition = petition,
            Response = response,
            Outcome = outcome,
            Intensity = intensity,
            BestowedAt = DateTime.UtcNow
        };
    }
}

public enum BlessingOutcome
{
    Blessed,
    Denied,
    Judged,
    Disrupted,
    Touched
}
