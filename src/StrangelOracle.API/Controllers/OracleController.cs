using Microsoft.AspNetCore.Mvc;
using StrangelOracle.Application.DTOs;
using StrangelOracle.Application.Interfaces;
using StrangelOracle.Domain.Enums;

namespace StrangelOracle.API.Controllers;

/// <summary>
/// The Strangel Oracle API
/// 
/// Through this interface, you may approach the Strange Angels.
/// Choose wisely. They are always watching.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OracleController : ControllerBase
{
    private readonly IOracleService _oracleService;
    private readonly ILogger<OracleController> _logger;
    
    public OracleController(IOracleService oracleService, ILogger<OracleController> logger)
    {
        _oracleService = oracleService;
        _logger = logger;
    }
    
    /// <summary>
    /// Check the presence of all Strangels
    /// </summary>
    /// <remarks>
    /// The Strangels move through the city at their own pace.
    /// Some are always present. Some appear only at certain hours.
    /// Check before you approach.
    /// </remarks>
    [HttpGet("presence")]
    [ProducesResponseType(typeof(OraclePresence), StatusCodes.Status200OK)]
    public async Task<ActionResult<OraclePresence>> GetPresence()
    {
        var presence = await _oracleService.GetPresenceAsync();
        return Ok(presence);
    }
    
    /// <summary>
    /// Get information about all Strangels
    /// </summary>
    [HttpGet("strangels")]
    [ProducesResponseType(typeof(IEnumerable<StrangelInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StrangelInfo>>> GetAllStrangels()
    {
        var strangels = await _oracleService.GetAllStrangelsAsync();
        return Ok(strangels);
    }
    
    /// <summary>
    /// Get information about a specific Strangel
    /// </summary>
    /// <param name="type">The Strangel to learn about</param>
    [HttpGet("strangels/{type}")]
    [ProducesResponseType(typeof(StrangelInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StrangelInfo>> GetStrangel(string type)
    {
        if (!Enum.TryParse<StrangelType>(type, true, out var strangelType))
        {
            return BadRequest($"Unknown Strangel: {type}. Known Strangels are: {string.Join(", ", Enum.GetNames<StrangelType>())}");
        }
        
        var info = await _oracleService.GetStrangelInfoAsync(strangelType);
        return Ok(info);
    }
    
    /// <summary>
    /// Seek a blessing from a Strangel
    /// </summary>
    /// <remarks>
    /// Approach with intention. Some Strangels require a petition.
    /// Some refuse petitions entirely. Some help. Some don't.
    /// The outcome is never guaranteed.
    /// </remarks>
    /// <param name="request">Your petition to the Strangel</param>
    [HttpPost("seek")]
    [ProducesResponseType(typeof(BlessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlessingResponse>> SeekBlessing([FromBody] BlessingRequest request)
    {
        _logger.LogInformation("Blessing sought from {Strangel}", request.Strangel);
        
        try
        {
            var blessing = await _oracleService.SeekBlessingAsync(request);
            return Ok(blessing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Touch the Woman with Heart
    /// </summary>
    /// <remarks>
    /// You do not pray to her. You do not speak.
    /// You touch. She receives. Something shifts.
    /// 
    /// This is her only interaction. It is enough.
    /// </remarks>
    [HttpPost("touch")]
    [ProducesResponseType(typeof(BlessingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BlessingResponse>> TouchHeart()
    {
        _logger.LogInformation("The Woman with Heart was touched");
        
        var blessing = await _oracleService.TouchHeartAsync();
        return Ok(blessing);
    }
    
    /// <summary>
    /// Petition the Fox
    /// </summary>
    /// <remarks>
    /// Ask him a question. Accept that he may not answer,
    /// or may answer wrong on purpose.
    /// The Fox helps those who don't need help,
    /// and confuses those who think they understand.
    /// </remarks>
    /// <param name="question">Your question for the Fox</param>
    [HttpPost("petition/fox")]
    [ProducesResponseType(typeof(BlessingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BlessingResponse>> PetitionFox([FromBody] string? question)
    {
        _logger.LogInformation("The Fox was petitioned");
        
        var request = new BlessingRequest
        {
            Strangel = StrangelType.Fox,
            Petition = question
        };
        
        var blessing = await _oracleService.SeekBlessingAsync(request);
        return Ok(blessing);
    }
    
    /// <summary>
    /// Confess to the Furies
    /// </summary>
    /// <remarks>
    /// Tell them what weighs on you.
    /// They will judge. They always judge.
    /// But judgment is not always condemnation.
    /// Sometimes it is clarity.
    /// </remarks>
    /// <param name="confession">What you carry</param>
    [HttpPost("confess")]
    [ProducesResponseType(typeof(BlessingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BlessingResponse>> ConfessToFuries([FromBody] string? confession)
    {
        _logger.LogInformation("A confession was made to the Furies");
        
        var request = new BlessingRequest
        {
            Strangel = StrangelType.Furies,
            Petition = confession
        };
        
        var blessing = await _oracleService.SeekBlessingAsync(request);
        return Ok(blessing);
    }
    
    /// <summary>
    /// Invoke Nok'so
    /// </summary>
    /// <remarks>
    /// Call him when you need something broken.
    /// He will decide if you're right.
    /// He does not explain. He does not apologize.
    /// He acts, or he doesn't.
    /// </remarks>
    /// <param name="request">What needs breaking</param>
    [HttpPost("invoke/nokso")]
    [ProducesResponseType(typeof(BlessingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BlessingResponse>> InvokeNokso([FromBody] string? request)
    {
        _logger.LogInformation("Nok'so was invoked");
        
        var blessingRequest = new BlessingRequest
        {
            Strangel = StrangelType.Nokso,
            Petition = request
        };
        
        var blessing = await _oracleService.SeekBlessingAsync(blessingRequest);
        return Ok(blessing);
    }
}
