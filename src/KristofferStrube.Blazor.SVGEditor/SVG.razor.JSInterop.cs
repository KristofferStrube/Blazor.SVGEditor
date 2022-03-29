using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVG
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    private IJSObjectReference _jSModule;
    public Box BBox { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _jSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
        BBox = await GetBoundingBox(SVGElementReference);
    }

    public async Task Focus(ElementReference elementReference)
    {
        await _jSModule.InvokeVoidAsync("focus", elementReference);
    }

    public async Task UnFocus(ElementReference elementReference)
    {
        await _jSModule.InvokeVoidAsync("unFocus", elementReference);
    }

    public async Task<Box> GetBoundingBox(ElementReference elementReference)
    {
        return await _jSModule.InvokeAsync<Box>("BBox", elementReference);
    }
}
