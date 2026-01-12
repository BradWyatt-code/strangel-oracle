
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StrangelOracle.Infrastructure.AI;

public static class SemanticKernelConfiguration
{
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var openAiKey = configuration["OpenAI:ApiKey"] 
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OpenAI API key not configured");
            
        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";
        
        services.AddSingleton<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();
            
            builder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: openAiKey
            );
            
            return builder.Build();
        });
        
        return services;
    }
}
