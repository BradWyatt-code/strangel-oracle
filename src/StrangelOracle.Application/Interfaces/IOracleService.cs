using StrangelOracle.Application.DTOs;
using StrangelOracle.Domain.Enums;

namespace StrangelOracle.Application.Interfaces;

/// <summary>
/// Main service for interacting with the Strangel Oracle
/// </summary>
public interface IOracleService
{
    /// <summary>
    /// Seek a blessing from a specific Strangel
    /// </summary>
    Task<BlessingResponse> SeekBlessingAsync(BlessingRequest request);
    
    /// <summary>
    /// Get information about a specific Strangel
    /// </summary>
    Task<StrangelInfo> GetStrangelInfoAsync(StrangelType type);
    
    /// <summary>
    /// Get information about all Strangels
    /// </summary>
    Task<IEnumerable<StrangelInfo>> GetAllStrangelsAsync();
    
    /// <summary>
    /// Check current presence of all Strangels
    /// </summary>
    Task<OraclePresence> GetPresenceAsync();
    
    /// <summary>
    /// Touch the Woman with Heart - her unique blessing interaction
    /// </summary>
    Task<BlessingResponse> TouchHeartAsync();
}
