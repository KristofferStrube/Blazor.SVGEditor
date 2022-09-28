using AngleSharp.Dom;
using AngleSharp.Text;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.GradientEditors;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace KristofferStrube.Blazor.SVGEditor;

public class LinearGradient : ISVGElement
{
    public LinearGradient(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;

        if (Id is not null && Element.ParentElement?.TagName == "DEFS")
        {
            SVG.Definitions[Id] = this;
        }
        Stops = Element.Children
            .Where(child => child.TagName == "STOP")
            .Select(child => new Stop(child, this, svg))
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
    public string Id
    {
        get => Element.GetAttribute("id");
        set { Element.SetAttribute("id", value); Changed.Invoke(this); }
    }

    public IElement Element { get; init; }

    public SVG SVG { get; init; }

    public Type Presenter => typeof(LinearGradientPresenter);

    public string StateRepresentation => throw new NotImplementedException();

    public double X1
    {
        get
        {
            var x1 = Element.GetAttribute("x1");
            if (x1 is null)
            {
                return 0;
            }
            else if (x1.Trim().EndsWith("%"))
            {
                return double.Parse(x1.Trim()[..^1]) / 100;
            }
            else
            {
                return double.Parse(x1.Trim());
            }
        }
        set { Element.SetAttribute("x1", (value * 100).AsString() + "%"); Changed.Invoke(this); }
    }
    public double Y1
    {
        get
        {
            var y1 = Element.GetAttribute("y1");
            if (y1 is null)
            {
                return 0;
            }
            else if (y1.Trim().EndsWith("%"))
            {
                return double.Parse(y1.Trim()[..^1]) / 100;
            }
            else
            {
                return double.Parse(y1.Trim());
            }
        }
        set { Element.SetAttribute("y1", (value * 100).AsString() + "%"); Changed.Invoke(this); }
    }
    public double X2
    {
        get
        {
            var x2 = Element.GetAttribute("x2");
            if (x2 is null)
            {
                return 1;
            }
            else if (x2.Trim().EndsWith("%"))
            {
                return double.Parse(x2.Trim()[..^1]) / 100;
            }
            else
            {
                return double.Parse(x2.Trim());
            }
        }
        set { Element.SetAttribute("x2", (value * 100).AsString() + "%"); Changed.Invoke(this); }
    }
    public double Y2
    {
        get {
            var y2 = Element.GetAttribute("y2");
            if (y2 is null)
            {
                return 0;
            }
            else if (y2.Trim().EndsWith("%"))
            {
                return double.Parse(y2.Trim()[..^1]) / 100;
            }
            else
            {
                return double.Parse(y2.Trim());
            }
        }
        set { Element.SetAttribute("y2", (value * 100).AsString() + "%"); Changed.Invoke(this); }
    }

    public GradientUnits GradientUnits
    {
        get
        {
            return Element.GetAttributeOrEmpty("gradientUnits") switch
            {
                "userSpaceOnUse" => GradientUnits.UserSpaceOnUse,
                _ => GradientUnits.ObjectBoundingBox
            };
        }
        set
        {
            Element.SetAttribute("gradientUnits", value switch
            {
                GradientUnits.UserSpaceOnUse => "userSpaceOnUser",
                _ => "objectBoundingBox"
            });
        }
    }
    public SpreadMethod SpreadMethod
    {
        get
        {
            return Element.GetAttributeOrEmpty("spreadMethod") switch
            {
                "reflect" => SpreadMethod.Reflect,
                "repeat" => SpreadMethod.Repeat,
                _ => SpreadMethod.Pad
            };
        }
        set
        {
            Element.SetAttribute("spreadMethod", value switch
            {
                SpreadMethod.Reflect => "reflect",
                SpreadMethod.Repeat => "repeat",
                _ => "pad"
            });
        }
    }

    public List<Stop> Stops { get; set; }

    public int? CurrentStop { get; set; }

    public List<BaseAnimate> AnimationElements { get; set; }

    public Action<ISVGElement> Changed { get; set; }

    public string StoredHtml { get; set; }

    public bool IsEditing { get; set; }

    public Shape EditingShape { get; set; }

    public void Edit(Shape shape)
    {
        IsEditing = true;
        EditingShape = shape;
    }

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
