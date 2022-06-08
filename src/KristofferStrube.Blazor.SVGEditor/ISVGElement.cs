using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public interface ISVGElement
{
    public IElement Element { get; init; }
    public Type Editor { get; }
    public string StateRepresentation { get; }
    public Action<ISVGElement> Changed { get; set; }
    public void UpdateHtml();
    public string StoredHtml { get; set; }
    public void Complete();
    public void Rerender();
}
