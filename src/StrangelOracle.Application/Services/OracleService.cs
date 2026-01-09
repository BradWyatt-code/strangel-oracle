using StrangelOracle.Application.DTOs;
using StrangelOracle.Application.Interfaces;
using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;
using StrangelOracle.Domain.Interfaces;

namespace StrangelOracle.Application.Services;

/// <summary>
/// Main service for the Strangel Oracle
/// </summary>
public sealed class OracleService : IOracleService
{
    private readonly IEnumerable<IStrangelEngine> _engines;
    
    public OracleService(IEnumerable<IStrangelEngine> engines)
    {
        _engines = engines;
    }
    
    public async Task<BlessingResponse> SeekBlessingAsync(BlessingRequest request)
    {
        var engine = _engines.FirstOrDefault(e => e.StrangelType == request.Strangel)
            ?? throw new InvalidOperationException($"No engine found for {request.Strangel}");
        
        var blessing = await engine.GenerateBlessingAsync(request.Petition);
        var strangel = Strangel.GetByType(request.Strangel);
        
        return MapToResponse(blessing, strangel);
    }
    
    public async Task<StrangelInfo> GetStrangelInfoAsync(StrangelType type)
    {
        var strangel = Strangel.GetByType(type);
        var engine = _engines.FirstOrDefault(e => e.StrangelType == type);
        
        var isPresent = engine != null && await engine.IsPresent();
        var mood = engine != null ? await engine.GetCurrentDisposition() : strangel.Disposition;
        
        return new StrangelInfo
        {
            Type = type.ToString(),
            Name = strangel.Name,
            Title = strangel.Title,
            Aspect = strangel.Aspect,
            Function = strangel.Function,
            Disposition = strangel.Disposition,
            Domains = strangel.Domains,
            Manifestations = strangel.Manifestations,
            RitualInstruction = strangel.RitualInstruction,
            IsPresent = isPresent,
            CurrentMood = mood
        };
    }
    
    public async Task<IEnumerable<StrangelInfo>> GetAllStrangelsAsync()
    {
        var types = Enum.GetValues<StrangelType>();
        var tasks = types.Select(GetStrangelInfoAsync);
        return await Task.WhenAll(tasks);
    }
    
    public async Task<OraclePresence> GetPresenceAsync()
    {
        var presence = new Dictionary<string, bool>();
        
        foreach (var type in Enum.GetValues<StrangelType>())
        {
            var engine = _engines.FirstOrDefault(e => e.StrangelType == type);
            presence[type.ToString()] = engine != null && await engine.IsPresent();
        }
        
        return new OraclePresence
        {
            Timestamp = DateTime.UtcNow,
            StrangelPresence = presence,
            AmbientMood = DetermineAmbientMood()
        };
    }
    
    public async Task<BlessingResponse> TouchHeartAsync()
    {
        return await SeekBlessingAsync(new BlessingRequest 
        { 
            Strangel = StrangelType.WomanWithHeart 
        });
    }
    
    private static BlessingResponse MapToResponse(Blessing blessing, Strangel strangel)
    {
        return new BlessingResponse
        {
            Id = blessing.Id,
            Strangel = strangel.Name,
            StrangelTitle = strangel.Title,
            Type = blessing.Type.ToString(),
            Intensity = blessing.Intensity.Value,
            IntensityDescription = blessing.Intensity.ToDescription(),
            Message = blessing.Message,
            SecondaryMessage = blessing.SecondaryMessage,
            ReleasedEssence = blessing.Released?.Essence,
            BestowedAt = blessing.BestowedAt,
            DurationMinutes = (int)blessing.Duration.TotalMinutes,
            IsActive = blessing.IsActive,
            RemainingStrength = blessing.RemainingStrength
        };
    }
    
    private static string DetermineAmbientMood()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            >= 0 and < 4 => "The city sleeps. The Strangels are closer.",
            >= 4 and < 7 => "The hour of the threshold. Anything can cross.",
            >= 7 and < 12 => "Daylight presses. They retreat to edges.",
            >= 12 and < 17 => "The afternoon haze. They watch from shadows.",
            >= 17 and < 20 => "The golden hour. The Woman with Heart is strongest.",
            >= 20 and < 23 => "Night falls. The Fox begins to move.",
            _ => "The Furies circle. Judgment hangs in the air."
        };
    }
}
