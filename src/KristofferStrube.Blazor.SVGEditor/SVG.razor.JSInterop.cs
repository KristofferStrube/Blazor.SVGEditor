using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected IJSObjectReference JSModule { get; set; }

        public BoundingBox BBox { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
            BBox = await GetBBox(SVGElementReference);
        }

        public async Task Focus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("focus", elementReference);
        }

        public async Task UnFocus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("unFocus", elementReference);
        }

        public async Task<BoundingBox> GetBBox(ElementReference elementReference)
        {
            return await JSModule.InvokeAsync<BoundingBox>("BBox", elementReference);
        }
    }
}
