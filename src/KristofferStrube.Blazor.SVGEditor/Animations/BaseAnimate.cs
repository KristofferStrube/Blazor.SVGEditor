using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor;

public abstract class BaseAnimate : ISVGElement
{
    public BaseAnimate(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;
        Values = StringToValues(Element.GetAttribute("values"));
        Dur = Element.GetAttribute("dur") is string s ? s.Replace("s", "").ParseAsDouble() : 0;
        AttributeName = Element.GetAttributeOrEmpty("attributename");
        ValuesAsString = Element.GetAttributeOrEmpty("values");
    }

    internal string _stateRepresentation;

    public IElement Element { get; init; }
    public SVG SVG { get; init; }
    public abstract Type Editor { get; }
    public string StateRepresentation => Playing.ToString() + SVG.EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y;
    public bool Playing { get; set; }
    public List<string> Values { get; set; }
    public int FrameCount => Values.Count;
    public double Dur { get; set; }
    public string AttributeName { get; set; }
    public string ValuesAsString { get; set; }
    public Action<ISVGElement> Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string StoredHtml { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void UpdateValues()
    {
        Element.SetAttribute("values", ValuesToString(Values));
    }

    private static List<string> StringToValues(string attribute)
    {
        if (attribute == null)
        {
            return new List<string>();
        }

        return attribute.Split(";").Select(e => e.Trim()).ToList();
    }

    private static string ValuesToString(List<string> values)
    {
        return string.Join(";", values);
    }

    public void UpdateHtml()
    {
        throw new NotImplementedException();
    }

    public void Complete()
    {
        throw new NotImplementedException();
    }

    public void Rerender()
    {
        _stateRepresentation = null;
    }
}
