using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor;

public interface ISVGElement
{
    public string? Id { get; set; }
    public IElement Element { get; init; }
    public Type Presenter { get; }
    public string StateRepresentation { get; }
    public Action<ISVGElement>? Changed { get; set; }
    public void UpdateHtml();
    public string StoredHtml { get; set; }
    public void Complete();
    public void BeforeBeingRemoved();
    public void Rerender();
}
