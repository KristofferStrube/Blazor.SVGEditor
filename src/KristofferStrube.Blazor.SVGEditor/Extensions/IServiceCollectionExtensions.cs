using KristofferStrube.Blazor.SVGAnimation;
using Microsoft.Extensions.DependencyInjection;

namespace KristofferStrube.Blazor.SVGEditor.Extensions
{
    public static class IServiceCollectionExtensions

    {
        public static IServiceCollection AddSVGEditor(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddBlazorContextMenu()
                .AddSVGAnimationService();
        }
    }
}
