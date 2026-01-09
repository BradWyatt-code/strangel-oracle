namespace StrangelOracle.Domain.Enums;

/// <summary>
/// The nature of a Strangel's response
/// </summary>
public enum BlessingType
{
    /// <summary>
    /// A positive blessing - comfort, luck, protection
    /// </summary>
    Blessing,
    
    /// <summary>
    /// A judgment - neither purely good nor bad, but true
    /// </summary>
    Judgment,
    
    /// <summary>
    /// A refusal or withholding - the Strangel declines to respond
    /// </summary>
    Silence,
    
    /// <summary>
    /// A disruption - something is unsettled, broken open
    /// </summary>
    Disruption,
    
    /// <summary>
    /// A trick - the Fox's specialty, meaning unclear
    /// </summary>
    Trick
}
