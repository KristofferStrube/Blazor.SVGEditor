﻿using AngleSharp.Dom;
using AngleSharp.Text;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using System.Xml.Linq;

namespace KristofferStrube.Blazor.SVGEditor;

public class Stop : ISVGElement
{
    public Stop(IElement element, LinearGradient parent, SVG svg)
    {
        Element = element;
        Parent = parent;
        SVG = svg;
        AnimationElements = new();
        Key = Guid.NewGuid();
    }

    public Guid Key { get; set; }

    public string Id { get; set; }
    public IElement Element { get; init; }
    public LinearGradient Parent { get; init; }
    public SVG SVG { get; init; }

    public Type Presenter => throw new NotImplementedException();

    public string StateRepresentation => $"{Offset}{StopColor}{StopOpacity}";

    public double Offset
    {
        get
        {
            var offset = Element.GetAttribute("offset");
            if (offset is null)
            {
                return 0;
            }
            else if (offset.Trim().EndsWith("%"))
            {
                return offset.Trim()[..^1].ParseAsDouble() / 100;
            }
            else
            {
                return offset.Trim().ParseAsDouble();
            }
        }
        set
        {
            Element.SetAttribute("offset", (value*100).AsString() + "%");
            Changed.Invoke(this);
        }
    }

    public string StopColor
    {
        get => Element.GetAttributeOrEmpty("stop-color");
        set { Element.SetAttribute("stop-color", value); Changed.Invoke(this); }
    }

    public double StopOpacity
    {
        get => Element.GetAttributeOrOne("stop-opacity");
        set { if (value != 1) Element.SetAttribute("stop-opacity", value.AsString()); Changed.Invoke(this); }
    }

    public List<BaseAnimate> AnimationElements { get; set; }

    public Action<ISVGElement> Changed
    {
        get => Parent.Changed; set
        {
            Parent.Changed = value;
        }
    }
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
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<stop{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>" + (AnimationElements.Count > 0 ? "\n" : "") + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + "</stop>";
    }
}