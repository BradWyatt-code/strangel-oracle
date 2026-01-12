using StrangelOracle.Domain.Enums;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using StrangelOracle.Application.Prompts;

namespace StrangelOracle.Infrastructure.AI;

public interface IStrangelAI
{
    Task<StrangelResponse> ConsultAsync(
        StrangelType strangel, 
        string? petition = null,
        CancellationToken cancellationToken = default);
}

public sealed record StrangelResponse(
    string Message,
    BlessingOutcome Outcome,
    double Intensity
);

public enum BlessingOutcome
{
    Blessed,
    Denied,
    Judged,
    Disrupted,
    Touched
}

public sealed class StrangelAIService : IStrangelAI
{
    private readonly Kernel _kernel;
    private readonly Random _random = new();
    
    public StrangelAIService(Kernel kernel)
    {
        _kernel = kernel;
    }
    
    public async Task<StrangelResponse> ConsultAsync(
        StrangelType strangel,
        string? petition = null,
        CancellationToken cancellationToken = default)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        
        var systemPrompt = StrangelPrompts.GetPrompt(strangel);
        var userMessage = BuildUserMessage(strangel, petition);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userMessage);
        
        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = GetTemperature(strangel),
                ["max_tokens"] = 150
            }
        };
        
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            settings,
            cancellationToken: cancellationToken);
            
        var message = response.Content ?? "...";
        var outcome = DetermineOutcome(strangel, message);
        var intensity = CalculateIntensity(strangel, message);
        
        return new StrangelResponse(message, outcome, intensity);
    }
    
    private string BuildUserMessage(StrangelType strangel, string? petition)
    {
        return strangel switch
        {
            StrangelType.WomanWithHeart => petition ?? "I touch your image. I seek your blessing.",
            
            StrangelType.Fox => string.IsNullOrWhiteSpace(petition)
                ? "I petition you, Fox. I seek your aid."
                : $"I petition you, Fox: {petition}",
                
            StrangelType.Furies => string.IsNullOrWhiteSpace(petition)
                ? "I stand before you for judgment."
                : $"I confess: {petition}",
                
            StrangelType.Nokso => petition ?? "I invoke you, Nok'so.",
            
            _ => petition ?? "I seek communion."
        };
    }
    
    private double GetTemperature(StrangelType strangel)
    {
        // Each Strangel has different predictability
        return strangel switch
        {
            StrangelType.WomanWithHeart => 0.7,  // Gentle variation
            StrangelType.Fox => 1.2,              // Highly unpredictable
            StrangelType.Furies => 0.5,           // More consistent judgment
            StrangelType.Nokso => 0.9,            // Sharp but varied
            _ => 0.8
        };
    }
    
    private BlessingOutcome DetermineOutcome(StrangelType strangel, string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        return strangel switch
        {
            StrangelType.WomanWithHeart => BlessingOutcome.Touched,
            
            StrangelType.Fox => lowerMessage.Contains("ha") || 
                               lowerMessage.Contains("no") || 
                               lowerMessage.Contains("not today")
                ? BlessingOutcome.Denied
                : BlessingOutcome.Blessed,
                
            StrangelType.Furies => BlessingOutcome.Judged,
            
            StrangelType.Nokso => BlessingOutcome.Disrupted,
            
            _ => BlessingOutcome.Blessed
        };
    }
    
    private double CalculateIntensity(StrangelType strangel, string message)
    {
        // Base intensity varies by Strangel
        var baseIntensity = strangel switch
        {
            StrangelType.WomanWithHeart => 0.4,  // Gentle
            StrangelType.Fox => 0.6,              // Variable
            StrangelType.Furies => 0.8,           // Heavy
            StrangelType.Nokso => 0.9,            // Sharp
            _ => 0.5
        };
        
        // Silence is more intense for Woman with Heart
        if (strangel == StrangelType.WomanWithHeart && message == "...")
        {
            return 0.9; // Profound silence
        }
        
        // Adjust based on message characteristics
        var lengthModifier = Math.Min(message.Length / 100.0, 0.2);
        var variation = (_random.NextDouble() - 0.5) * 0.2;
        
        return Math.Clamp(baseIntensity + lengthModifier + variation, 0.1, 1.0);
    }
}
