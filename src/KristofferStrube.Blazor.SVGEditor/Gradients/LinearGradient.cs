using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.GradientEditors;

namespace KristofferStrube.Blazor.SVGEditor;

public class LinearGradient : ISVGElement
{
    public LinearGradient(IElement element, SVGEditor svg)
    {
        Element = element;
        SVG = svg;

        if (Id is not null && Element.ParentElement?.TagName.ToUpper() == "DEFS")
        {
            SVG.Definitions[Id] = this;
        }
        Stops = Element.Children
            .Where(child => child.TagName.ToUpper() == "STOP")
            .Select(child => new Stop(child, this, svg))
            .ToList();
        AnimationElements = Element.Children
            .Where(child => child.TagName.ToUpper() == "ANIMATE" && child.HasAttribute("attributename"))
            .Select(child =>
            {
                string? attributeName = child.GetAttribute("attributename");
                if (attributeName is not null && SVG.AnimationTypes.FirstOrDefault(at => at.AnimationAttributeName == attributeName)?.AnimationType is Type animationType)
                {
                    var animation = (BaseAnimate)Activator.CreateInstance(animationType, child, this, SVG)!;
                    animation.Parent = this;
                    return animation;
                }
                else
                {
                    return new AnimateDefault(child, this, SVG);
                }
            }
            )
            .ToList();
    }
    public string? Id
    {
        get => Element.GetAttribute("id");
        set { Element.SetAttribute("id", value); Changed?.Invoke(this); }
    }

    public IElement Element { get; init; }

    public SVGEditor SVG { get; init; }

    public Type Presenter => typeof(GradientPresenter);

    public string StateRepresentation => $"{X1}{X2}{Y1}{Y2}{string.Join("", Stops.Select(s => s.StateRepresentation))}";

    internal string _stateRepresentation = string.Empty;

    public double X1
    {
        get
        {
            string? x1 = Element.GetAttribute("x1");
            return x1 is null ? 0 : x1.Trim().EndsWith("%") ? x1.Trim()[..^1].ParseAsDouble() / 100 : x1.Trim().ParseAsDouble();
        }
        set { Element.SetAttribute("x1", (value * 100).AsString() + "%"); Changed?.Invoke(this); }
    }
    public double Y1
    {
        get
        {
            string? y1 = Element.GetAttribute("y1");
            return y1 is null ? 0 : y1.Trim().EndsWith("%") ? y1.Trim()[..^1].ParseAsDouble() / 100 : y1.Trim().ParseAsDouble();
        }
        set { Element.SetAttribute("y1", (value * 100).AsString() + "%"); Changed?.Invoke(this); }
    }
    public double X2
    {
        get
        {
            string? x2 = Element.GetAttribute("x2");
            return x2 is null ? 1 : x2.Trim().EndsWith("%") ? x2.Trim()[..^1].ParseAsDouble() / 100 : x2.Trim().ParseAsDouble();
        }
        set { Element.SetAttribute("x2", (value * 100).AsString() + "%"); Changed?.Invoke(this); }
    }
    public double Y2
    {
        get
        {
            string? y2 = Element.GetAttribute("y2");
            return y2 is null ? 0 : y2.Trim().EndsWith("%") ? y2.Trim()[..^1].ParseAsDouble() / 100 : y2.Trim().ParseAsDouble();
        }
        set { Element.SetAttribute("y2", (value * 100).AsString() + "%"); Changed?.Invoke(this); }
    }

    public GradientUnits GradientUnits
    {
        get => Element.GetAttributeOrEmpty("gradientUnits") switch
        {
            "userSpaceOnUse" => GradientUnits.UserSpaceOnUse,
            _ => GradientUnits.ObjectBoundingBox
        };
        set => Element.SetAttribute("gradientUnits", value switch
        {
            GradientUnits.UserSpaceOnUse => "userSpaceOnUser",
            GradientUnits.ObjectBoundingBox => "objectBoundingBox",
            _ => throw new NotSupportedException($"Value {value} not supported as {nameof(GradientUnits)}")
        });
    }
    public SpreadMethod SpreadMethod
    {
        get => Element.GetAttributeOrEmpty("spreadMethod") switch
        {
            "reflect" => SpreadMethod.Reflect,
            "repeat" => SpreadMethod.Repeat,
            _ => SpreadMethod.Pad
        };
        set => Element.SetAttribute("spreadMethod", value switch
        {
            SpreadMethod.Reflect => "reflect",
            SpreadMethod.Repeat => "repeat",
            SpreadMethod.Pad => "pad",
            _ => "pad"
        });
    }

    public List<Stop> Stops { get; set; }

    public int? CurrentStop { get; set; }

    public List<BaseAnimate> AnimationElements { get; set; }

    public Action<ISVGElement>? Changed { get; set; }

    public string StoredHtml { get; set; } = string.Empty;

    public Shape? EditingShape { get; set; }

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

    public void AddNewStop(Stop? tempStop = null, string color = "grey")
    {
        IElement element = SVG.Document.CreateElement("STOP");

        double offset = tempStop is null
            ? Stops.Count is 0 ? 1.0 / 3 : (Stops.Last().Offset / 2) + 0.5
            : tempStop == Stops.Last() ? (Stops.Last().Offset / 2) + 0.5 : (tempStop.Offset + Stops[Stops.IndexOf(tempStop) + 1].Offset) / 2;
        Stop stop = new(element, this, SVG)
        {
            Offset = offset,
            StopColor = color,
        };

        Stop? stopBefore = Stops.Where(s => s.Offset <= offset).MaxBy(s => s.Offset);
        if (stopBefore is null)
        {
            Stops.Add(stop);
            _ = Element.AppendElement(element);
        }
        else
        {
            Stops.Insert(Stops.IndexOf(stopBefore) + 1, stop);
            stopBefore.Element.InsertAfter(element);
        }

        SVG.EditMode = EditMode.None;
        Changed?.Invoke(this);
    }

    public static void AddNew(SVGEditor svg, string id, Shape? shape = null)
    {
        string firstStopColor = "grey";
        if (shape is not null)
        {
            firstStopColor = shape.Fill;
            shape.Fill = id.ToUrl();
        }

        IElement element = svg.Document.CreateElement("LINEARGRADIENT");

        LinearGradient linearGradient = new(element, svg);

        svg.AddDefinition(linearGradient);
        linearGradient.Id = string.IsNullOrEmpty(id) ? Random.Shared.Next(999).ToString() : id;
        svg.Definitions[linearGradient.Id] = linearGradient;

        if (shape is not null)
        {
            linearGradient.AddNewStop(color: firstStopColor);
        }
    }

    public void BeforeBeingRemoved()
    {
    }
}
