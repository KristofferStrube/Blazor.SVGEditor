using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected IJSObjectReference JSModule { get; set; }

        public Box BBox { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
            BBox = await GetBoundingBox(SVGElementReference);
        }

        public async Task Focus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("focus", elementReference);
        }

        public async Task UnFocus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("unFocus", elementReference);
        }

        public async Task<Box> GetBoundingBox(ElementReference elementReference)
        {
            return await JSModule.InvokeAsync<Box>("BBox", elementReference);
        }
    }
}
