using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.GradientEditors;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;
using static System.Formats.Asn1.AsnWriter;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor;

public class Defs : ISVGElement
{
    private readonly Dictionary<string, Type> GradientTypes = new()
    {
        { "LINEARGRADIENT", typeof(LinearGradient) }
    };

    public Defs(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;
        Children = Element.Children.Select(child =>
        {
            ISVGElement sVGElement;
            if (SVG.SupportedTypes.ContainsKey(child.TagName))
            {
                sVGElement = (Shape)Activator.CreateInstance(SVG.SupportedTypes[child.TagName], child, SVG);
            }
            else if (GradientTypes.ContainsKey(child.TagName))
            {
                sVGElement = (ISVGElement)Activator.CreateInstance(GradientTypes[child.TagName], child, SVG);
            }
            else
            {
                throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            }
            sVGElement.Changed = UpdateInput;
            return sVGElement;
        }).ToList();
    }

    public void UpdateInput(ISVGElement child)
    {
        child.UpdateHtml();
        Changed.Invoke(this);
    }
    public string Id { get; set; }

    public IElement Element { get; init; }

    public SVG SVG { get; init; }

    public List<ISVGElement> Children { get; init; } = new();

    public Type Presenter => typeof(DefsPresenter);

    public string StateRepresentation => throw new NotImplementedException();

    public Action<ISVGElement> Changed { get; set; }
    public string StoredHtml { get; set; }

    public void Complete()
    {
        throw new NotImplementedException();
    }

    public void Rerender()
    {
        throw new NotImplementedException();
    }

    public void UpdateHtml()
    {
        Children.ForEach(c => c.UpdateHtml());
        StoredHtml = $"<defs{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>\n" + string.Join("", Children.Select(e => e.StoredHtml + "\n")) + "</defs>";
    }
}
