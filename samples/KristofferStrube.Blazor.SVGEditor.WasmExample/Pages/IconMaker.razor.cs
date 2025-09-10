using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor.WasmExample.Pages;
public partial class IconMaker
{
    protected SVGEditor sVGEditor = default!;
    protected string Input = "";
    protected int width = 16;

    protected void InputHasBeenRendered()
    {
        double newScale = 1 / (width / sVGEditor.BBox.Height);
        if (sVGEditor.Scale != newScale)
        {
            sVGEditor.Scale = newScale;
            StateHasChanged();
        }
    }

    public async Task Copy()
    {
        string SVG = $"""
                    <svg class="bi" viewBox="0 0 16 16">
                        {Input}
                    </svg>
                    """;

        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", SVG);
    }
}
