using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVGEditor : IAsyncDisposable
{
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    protected Lazy<Task<IJSObjectReference>> moduleTask = default!;
    public Box BBox { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        BBox = await GetBoundingBox(svgElementReference);
        InputRendered?.Invoke();
    }

    public async Task FocusAsync(ElementReference elementReference)
    {
        if (DisableSelecting)
        {
            return;
        }

        IJSObjectReference module = await moduleTask.Value;
        await module.InvokeVoidAsync("focus", elementReference);
    }

    public async Task UnFocusAsync(ElementReference elementReference)
    {
        if (DisableDeselecting)
        {
            return;
        }

        IJSObjectReference module = await moduleTask.Value;
        await module.InvokeVoidAsync("unFocus", elementReference);
    }

    public async Task<Box> GetBoundingBox(ElementReference elementReference)
    {
        IJSObjectReference module = await moduleTask.Value;
        return await module.InvokeAsync<Box>("BBox", elementReference);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            IJSObjectReference module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
