using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddSVGEditor(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddBlazorContextMenu();
        }

        public static async Task Focus(this IJSRuntime jSRuntime, ElementReference elementReference)
        {
            var module = await jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
            await module.InvokeVoidAsync("focus", elementReference);
        }

        public static async Task<BoundingBox> BBox(this IJSRuntime jSRuntime, ElementReference elementReference)
        {
            var module = await jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
            return await module.InvokeAsync<BoundingBox>("BBox", elementReference);
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
