using Microsoft.Extensions.DependencyInjection;
using KristofferStrube.Blazor.SVGAnimation;

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
