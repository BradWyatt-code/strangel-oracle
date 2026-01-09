using StrangelOracle.Domain.Entities;
using StrangelOracle.Domain.Enums;

namespace StrangelOracle.Domain.Interfaces;

/// <summary>
/// Engine for generating Strangel responses
/// Each Strangel type has its own implementation with distinct behavior
/// </summary>
public interface IStrangelEngine
{
    /// <summary>
    /// The type of Strangel this engine serves
    /// </summary>
    StrangelType StrangelType { get; }
    
    /// <summary>
    /// Generate a blessing/response from this Strangel
    /// </summary>
    /// <param name="petition">Optional petition or confession from the supplicant</param>
    /// <returns>The Strangel's response</returns>
    Task<Blessing> GenerateBlessingAsync(string? petition = null);
    
    /// <summary>
    /// Determine if this Strangel is currently present/available
    /// Some Strangels are more active at certain times
    /// </summary>
    Task<bool> IsPresent();
    
    /// <summary>
    /// Get the current "mood" or disposition of the Strangel
    /// Affects the nature of blessings given
    /// </summary>
    Task<string> GetCurrentDisposition();
}
