using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.GradientEditors;
using static System.Formats.Asn1.AsnWriter;

namespace KristofferStrube.Blazor.SVGEditor;

public class LinearGradient : ISVGElement
{
    public LinearGradient(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;

        GradientUnits = element.GetAttributeOrEmpty("gradientUnits") switch
        {
            "userSpaceOnUse" => GradientUnits.UserSpaceOnUse,
            _ => GradientUnits.ObjectBoundingBox
        };
        X1 = element.GetAttributeOrZero("x1");
        Y1 = element.GetAttributeOrZero("y1");
        X2 = element.GetAttributeOrZero("x2");
        Y2 = element.GetAttributeOrZero("y2");
        SpreadMethod = element.GetAttributeOrEmpty("spreadMethod") switch
        {
            "reflect" => SpreadMethod.Reflect,
            "repeat" => SpreadMethod.Repeat,
            _ => SpreadMethod.Pad
        };
        Stops = Element.Children
            .Where(child => child.TagName == "STOP")
            .Select(child => new Stop(child, svg))
            .ToList();
        AnimationElements = Element.Children
            .Where(child => child.TagName == "ANIMATE" && child.HasAttribute("attributename"))
            .Select(child =>
            {
                string attributeName = child.GetAttribute("attributename");
                if (Shape.animateTypes.ContainsKey(attributeName))
                {
                    BaseAnimate animation = (BaseAnimate)Activator.CreateInstance(Shape.animateTypes[attributeName], child, SVG);
                    animation.Parent = this;
                    return animation;
                }
                else
                {
                    return new AnimateDefault(child, SVG);
                }
            }
            )
            .ToList();
    }
    public string Id { get; set; }

    public IElement Element { get; init; }

    public SVG SVG { get; init; }

    public Type Editor => typeof(LinearGradientEditor);

    public string StateRepresentation => throw new NotImplementedException();

    public GradientUnits GradientUnits { get; set; }

    public double X1 { get; set; }

    public double Y1 { get; set; }

    public double X2 { get; set; }

    public double Y2 { get; set; }

    public SpreadMethod SpreadMethod { get; set; }

    public List<Stop> Stops { get; set; }

    public List<BaseAnimate> AnimationElements { get; set; }

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
        Stops.ForEach(s => s.UpdateHtml());
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<linearGradient{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>\n" + string.Join("", Stops.Select(e => e.StoredHtml + "\n")) + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + "</linearGradient>";
    }
}
