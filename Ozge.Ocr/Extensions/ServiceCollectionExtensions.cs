using Microsoft.Extensions.DependencyInjection;
using Ozge.Core.Contracts;

namespace Ozge.Ocr.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOzgeOcr(this IServiceCollection services)
    {
        services.AddSingleton<IOcrPipeline, OcrPipeline>();
        return services;
    }
}
