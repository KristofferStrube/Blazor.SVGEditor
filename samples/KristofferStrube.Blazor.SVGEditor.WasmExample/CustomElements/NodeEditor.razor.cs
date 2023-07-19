using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor.WasmExample.CustomElements;

public partial class NodeEditor
{
    public new async Task SelectAsync(MouseEventArgs eventArgs)
    {
        if (SVGElement.SVG.EditMode is EditMode.Add && SVGElement.SVG.SelectedShapes.Any(s => s is Connector))
        {
            SVGElement.SVG.SelectedShapes.Add(SVGElement);
        }
        else
        {
            await base.SelectAsync(eventArgs);
        }
    }
}