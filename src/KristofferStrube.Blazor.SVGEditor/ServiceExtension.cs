using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddSVGEditor(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddBlazorContextMenu();
        }
    }

    public class BoundingBox
    {
        public double x { get; set; }

        public double y { get; set; }

        public double width { get; set; }

        public double height { get; set; }
    }
}
