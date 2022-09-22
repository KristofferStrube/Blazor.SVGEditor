using AngleSharp;
using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using System.Text;

namespace KristofferStrube.Blazor.SVGEditor;

public abstract class BaseAnimate : ISVGElement
{
    public BaseAnimate(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;
        if (Element.HasAttribute("values"))
        {
            Values = StringToValues(Element.GetAttribute("values"));
        }
        else
        {
            var valuesSB = new StringBuilder();
            if (Element.HasAttribute("from"))
            {
                valuesSB.Append(Element.GetAttribute("from"));
            }
            if (Element.HasAttribute("to"))
            {
                valuesSB.Append(Element.GetAttribute("to"));
            }
            Values = StringToValues(valuesSB.ToString());
        }
    }

    internal string _stateRepresentation;

    public string Id { get; set; }
    public IElement Element { get; init; }
    public SVG SVG { get; init; }
    public ISVGElement Parent { get; set; }
    public abstract Type Presenter { get; }
    public abstract Type MenuItem { get; }
    public string StateRepresentation => Playing.ToString() + Dur + AttributeName + ValuesAsString + CurrentFrame.ToString();
    public bool Playing { get; set; }
    public List<string> Values { get; set; }
    public int FrameCount => Values.Count;
    public int? CurrentFrame { get; set; }
    public double Begin
    {
        get
        {
            return Element.GetAttribute("begin") is string s ? s.Replace("s", "").ParseAsDouble() : 0;
        }
        set
        {
            Element.SetAttribute("begin", $"{value.AsString()}s");
        }
    }
    public double Dur
    {
        get
        {
            return Element.GetAttribute("dur") is string s ? s.Replace("s", "").ParseAsDouble() : 0;
        }
        set
        {
            Element.SetAttribute("dur", $"{value.AsString()}s");
        }
    }
    public string AttributeName
    {
        get
        {
            return Element.GetAttributeOrEmpty("attributename");
        }
        init
        {
            Element.SetAttribute("attributename", value);
        }
    }
    public string ValuesAsString => Element.GetAttributeOrEmpty("values");
    public Action<ISVGElement> Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string StoredHtml { get; set; }

    public void UpdateValues()
    {
        if (Element.HasAttribute("values"))
        {
            Element.SetAttribute("values", ValuesToString(Values));
        }
        else if (Element.HasAttribute("from") || Element.HasAttribute("to"))
        {
            int i = 0;
            if (Element.HasAttribute("from"))
            {
                Element.SetAttribute("from", Values[i]);
                i++;
            }
            if (Element.HasAttribute("to"))
            {
                Element.SetAttribute("to", Values[i]);
            }
        }
        else
        {
            Element.SetAttribute("values", ValuesToString(Values));
        }
        Parent.UpdateHtml();
    }

    public abstract bool IsEditing(string property);

    public abstract void AddFrame();

    public static List<string> StringToValues(string attribute)
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
        StoredHtml = Element.ToHtml();
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
