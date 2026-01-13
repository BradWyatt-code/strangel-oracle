using StrangelOracle.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using StrangelOracle.Application.Prompts;
using StrangelOracle.Domain.Entities;
using StrangelOracle.Infrastructure.AI;
using StrangelOracle.Infrastructure.Repositories;

namespace StrangelOracle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OracleController : ControllerBase
{
    private readonly IStrangelAI _strangelAI;
    private readonly ISoulLedgerRepository _soulLedger;
    private readonly ILogger<OracleController> _logger;
    
    public OracleController(
        IStrangelAI strangelAI,
        ISoulLedgerRepository soulLedger,
        ILogger<OracleController> logger)
    {
        _strangelAI = strangelAI;
        _soulLedger = soulLedger;
        _logger = logger;
    }
    
    /// <summary>
    /// Consult a Strangel and receive their response.
    /// The interaction is recorded in your Soul Ledger.
    /// </summary>
    [HttpPost("consult")]
    [ProducesResponseType(typeof(ConsultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsultResponse>> Consult(
        [FromBody] ConsultRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StrangelType>(request.Strangel, ignoreCase: true, out var strangel))
        {
            return BadRequest(new { error = "Unknown Strangel. Choose: WomanWithHeart, Fox, Furies, or Nokso" });
        }
        
        _logger.LogInformation("Seeker {SessionId} consults {Strangel}", 
            request.SessionId, strangel);
        
        // Consult the Strangel via AI
        var response = await _strangelAI.ConsultAsync(
            strangel, 
            request.Petition, 
            cancellationToken);
        
        // Record in Soul Ledger
        var entry = SoulLedgerEntry.Create(
            sessionId: request.SessionId,
            strangel: (Domain.Enums.StrangelType)(int)strangel,
            petition: request.Petition,
            response: response.Message,
            outcome: (Domain.Entities.BlessingOutcome)(int)response.Outcome,
            intensity: response.Intensity
        );
        
        await _soulLedger.RecordAsync(entry, cancellationToken);
        
        return Ok(new ConsultResponse(
            Strangel: strangel.ToString(),
            Message: response.Message,
            Outcome: response.Outcome.ToString(),
            Intensity: response.Intensity,
            RecordedAt: entry.BestowedAt
        ));
    }
    
    /// <summary>
    /// Touch the Woman with Heart. No words needed.
    /// </summary>
    [HttpPost("touch")]
    [ProducesResponseType(typeof(ConsultResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultResponse>> Touch(
        [FromBody] TouchRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _strangelAI.ConsultAsync(
            StrangelType.WomanWithHeart,
            cancellationToken: cancellationToken);
            
        var entry = SoulLedgerEntry.Create(
            sessionId: request.SessionId,
            strangel: Domain.Enums.StrangelType.WomanWithHeart,
            petition: null,
            response: response.Message,
            outcome: Domain.Entities.BlessingOutcome.Touched,
            intensity: response.Intensity
        );
        
        await _soulLedger.RecordAsync(entry, cancellationToken);
        
        return Ok(new ConsultResponse(
            Strangel: "WomanWithHeart",
            Message: response.Message,
            Outcome: "Touched",
            Intensity: response.Intensity,
            RecordedAt: entry.BestowedAt
        ));
    }
    
    /// <summary>
    /// Retrieve your Soul Ledger—the record of all your encounters.
    /// </summary>
    [HttpGet("ledger/{sessionId}")]
    [ProducesResponseType(typeof(LedgerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LedgerResponse>> GetLedger(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var entries = await _soulLedger.GetBySessionAsync(sessionId, cancellationToken);
        var summary = await _soulLedger.GetSessionSummaryAsync(sessionId, cancellationToken);
        
        return Ok(new LedgerResponse(
            SessionId: sessionId,
            Summary: new LedgerSummary(
                TotalEncounters: entries.Count,
                Blessed: summary.TotalBlessings,
                Denied: summary.TotalDenials,
                Judged: summary.TotalJudgments,
                Disrupted: summary.TotalDisruptions,
                Touched: summary.TotalTouches,
                FirstEncounter: summary.FirstEncounter,
                LastEncounter: summary.LastEncounter
            ),
            Entries: entries.Select(e => new LedgerEntry(
                Strangel: e.Strangel.ToString(),
                Petition: e.Petition,
                Response: e.Response,
                Outcome: e.Outcome.ToString(),
                Intensity: e.Intensity,
                BestowedAt: e.BestowedAt
            )).ToList()
        ));
    }
}

// Request/Response DTOs
public sealed record ConsultRequest(
    string SessionId,
    string Strangel,
    string? Petition = null
);

public sealed record TouchRequest(string SessionId);

public sealed record ConsultResponse(
    string Strangel,
    string Message,
    string Outcome,
    double Intensity,
    DateTime RecordedAt
);

public sealed record LedgerResponse(
    string SessionId,
    LedgerSummary Summary,
    List<LedgerEntry> Entries
);

public sealed record LedgerSummary(
    int TotalEncounters,
    int Blessed,
    int Denied,
    int Judged,
    int Disrupted,
    int Touched,
    DateTime? FirstEncounter,
    DateTime? LastEncounter
);

public sealed record LedgerEntry(
    string Strangel,
    string? Petition,
    string Response,
    string Outcome,
    double Intensity,
    DateTime BestowedAt
);
// Phase 2 rebuild Mon Jan 12 20:16:00 PST 2026
