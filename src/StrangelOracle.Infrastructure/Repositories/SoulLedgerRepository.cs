using Microsoft.EntityFrameworkCore;
using StrangelOracle.Domain.Entities;
using StrangelOracle.Infrastructure.Data;

namespace StrangelOracle.Infrastructure.Repositories;

public interface ISoulLedgerRepository
{
    Task<SoulLedgerEntry> RecordAsync(SoulLedgerEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SoulLedgerEntry>> GetBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SoulLedgerEntry>> GetByStrangelAsync(StrangelType strangel, int limit = 50, CancellationToken cancellationToken = default);
    Task<SoulLedgerSummary> GetSessionSummaryAsync(string sessionId, CancellationToken cancellationToken = default);
}

public sealed record SoulLedgerSummary(
    int TotalBlessings,
    int TotalDenials,
    int TotalJudgments,
    int TotalDisruptions,
    int TotalTouches,
    Dictionary<StrangelType, int> EncountersByStrangel,
    DateTime? FirstEncounter,
    DateTime? LastEncounter
);

public sealed class SoulLedgerRepository : ISoulLedgerRepository
{
    private readonly StrangelDbContext _context;
    
    public SoulLedgerRepository(StrangelDbContext context)
    {
        _context = context;
    }
    
    public async Task<SoulLedgerEntry> RecordAsync(
        SoulLedgerEntry entry, 
        CancellationToken cancellationToken = default)
    {
        _context.SoulLedger.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return entry;
    }
    
    public async Task<IReadOnlyList<SoulLedgerEntry>> GetBySessionAsync(
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.SoulLedger
            .Where(e => e.SessionId == sessionId)
            .OrderByDescending(e => e.BestowedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<SoulLedgerEntry>> GetByStrangelAsync(
        StrangelType strangel, 
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.SoulLedger
            .Where(e => e.Strangel == strangel)
            .OrderByDescending(e => e.BestowedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<SoulLedgerSummary> GetSessionSummaryAsync(
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        var entries = await _context.SoulLedger
            .Where(e => e.SessionId == sessionId)
            .ToListAsync(cancellationToken);
            
        if (!entries.Any())
        {
            return new SoulLedgerSummary(0, 0, 0, 0, 0, new(), null, null);
        }
        
        var encountersByStrangel = entries
            .GroupBy(e => e.Strangel)
            .ToDictionary(g => g.Key, g => g.Count());
            
        return new SoulLedgerSummary(
            TotalBlessings: entries.Count(e => e.Outcome == BlessingOutcome.Blessed),
            TotalDenials: entries.Count(e => e.Outcome == BlessingOutcome.Denied),
            TotalJudgments: entries.Count(e => e.Outcome == BlessingOutcome.Judged),
            TotalDisruptions: entries.Count(e => e.Outcome == BlessingOutcome.Disrupted),
            TotalTouches: entries.Count(e => e.Outcome == BlessingOutcome.Touched),
            EncountersByStrangel: encountersByStrangel,
            FirstEncounter: entries.Min(e => e.BestowedAt),
            LastEncounter: entries.Max(e => e.BestowedAt)
        );
    }
}
