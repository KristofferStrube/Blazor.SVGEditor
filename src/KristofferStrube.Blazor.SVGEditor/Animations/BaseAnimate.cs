using AngleSharp;
using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using System.Text;

namespace KristofferStrube.Blazor.SVGEditor;

public abstract class BaseAnimate : ISVGElement
{
    public BaseAnimate(IElement element, ISVGElement parent, SVGEditor svg)
    {
        Element = element;
        Parent = parent;
        SVGEditor = svg;
        if (Element.HasAttribute("values"))
        {
            Values = StringToValues(Element.GetAttributeOrEmpty("values"));
        }
        else
        {
            var valuesSB = new StringBuilder();
            if (Element.HasAttribute("from"))
            {
                _ = valuesSB.Append(Element.GetAttribute("from"));
            }
            if (Element.HasAttribute("to"))
            {
                _ = valuesSB.Append(Element.GetAttribute("to"));
            }
            Values = StringToValues(valuesSB.ToString());
        }
    }

    internal string? _stateRepresentation;

    public string? Id { get; set; }
    public IElement Element { get; init; }
    public SVGEditor SVGEditor { get; init; }
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
        get => Element.GetAttribute("begin") is string s ? s.Replace("s", "").ParseAsDouble() : 0;
        set => Element.SetAttribute("begin", $"{value.AsString()}s");
    }
    public double Dur
    {
        get => Element.GetAttribute("dur") is string s ? s.Replace("s", "").ParseAsDouble() : 0;
        set => Element.SetAttribute("dur", $"{value.AsString()}s");
    }
    public string AttributeName
    {
        get => Element.GetAttributeOrEmpty("attributename");
        init => Element.SetAttribute("attributename", value);
    }
    public string ValuesAsString => Element.GetAttributeOrEmpty("values");
    public Action<ISVGElement>? Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string StoredHtml { get; set; } = string.Empty;

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
        Parent.Changed?.Invoke(Parent);
    }

    public abstract bool IsEditing(string property);

    public abstract void AddFrame();
    public abstract void RemoveFrame(int frame);

    public static List<string> StringToValues(string attribute)
    {
        return attribute == null ? [] : attribute.Split(";").Select(e => e.Trim()).ToList();
    }

    private static string ValuesToString(List<string> values)
    {
        return string.Join(";", values);
    }

    public void Remove()
    {
        SVGEditor.EditMode = EditMode.None;
        SVGEditor.ClearSelectedShapes();
        if (Parent is Shape parentShape)
        {
            _ = parentShape.AnimationElements.Remove(this);
        }
        SVGEditor.RemoveElement(this, Parent);
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

    public void BeforeBeingRemoved()
    {
    }
}
