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
    }
}
