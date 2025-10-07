using Microsoft.Extensions.DependencyInjection;

namespace Ozge.Ocr.Services;

public static class OcrServiceCollectionExtensions
{
    public static IServiceCollection AddOzgeOcr(this IServiceCollection services)
    {
        services.AddSingleton<IContentIngestionService, OfflineContentIngestionService>();
        return services;
    }
}
