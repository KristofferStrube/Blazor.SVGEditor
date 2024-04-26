using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Text : Shape
{
    private readonly Dictionary<string, string> styleAttributes;

    public Text(IElement element, SVGEditor svg) : base(element, svg)
    {
        string? style = element.GetAttribute("style");
        styleAttributes = style is null
            ? (Dictionary<string, string>)[]
            : style
                .Split(";")
                .Select(style => style.Split(":"))
                .Where(pair => pair.Length is 2)
                .ToDictionary(pair => pair[0], pair => pair[1]);

        double fontSize = double.TryParse(styleAttributes.GetValueOrDefault("font-size"), out double size) ? size : 12;
        double fontHeightEstimate = fontSize * 0.7;

        BoundingBox = new()
        {
            X = element.GetAttributeOrZero("x"),
            Y = element.GetAttributeOrZero("y") - fontHeightEstimate,
            Width = element.TextContent.Length * fontHeightEstimate,
            Height = fontHeightEstimate
        };
    }

    public double X
    {
        get => Element.GetAttributeOrZero("x");
        set { Element.SetAttribute("x", value.AsString()); Changed?.Invoke(this); }
    }
    public double Y
    {
        get => Element.GetAttributeOrZero("y");
        set { Element.SetAttribute("y", value.AsString()); Changed?.Invoke(this); }
    }

    public string CharacterData
    {
        get => Element.TextContent;
        set { Element.InnerHtml = value; Changed?.Invoke(this); }
    }

    public string FontSize
    {
        get => styleAttributes.GetValueOrDefault("font-size", "");
        set { styleAttributes["font-size"] = value; UpdateStyle(); }
    }

    public string FontWeight
    {
        get => styleAttributes.GetValueOrDefault("font-weight", "");
        set { styleAttributes["font-weight"] = value; UpdateStyle(); }
    }

    public string FontFamily
    {
        get => styleAttributes.GetValueOrDefault("font-family", "");
        set { styleAttributes["font-family"] = value; UpdateStyle(); }
    }

    public string Style => Element.GetAttributeOrEmpty("style");

    public void UpdateStyle()
    {
        Element.SetAttribute("style", string.Join(";", styleAttributes.Select(kv => $"{kv.Key}:{kv.Value}")));
        Changed?.Invoke(this);
    }

    public override Type Presenter => typeof(TextEditor);

    public override List<(double x, double y)> SelectionPoints => [(BoundingBox.X, BoundingBox.Y), (BoundingBox.X + BoundingBox.Width, BoundingBox.Y), (BoundingBox.X + BoundingBox.Width, BoundingBox.Y + BoundingBox.Height), (BoundingBox.X, BoundingBox.Y + BoundingBox.Height)];

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.Move:
                (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                X += diff.x;
                Y += diff.y;
                BoundingBox.X += diff.x;
                BoundingBox.Y += diff.y;
                break;
            case EditMode.None:
                break;
            case EditMode.Add:
                break;
            case EditMode.MoveAnchor:
                break;
            case EditMode.Scale:
                break;
            default:
                break;
        }
    }

    public override void HandlePointerOut(PointerEventArgs eventArgs)
    {
    }

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                SVG.EditMode = EditMode.None;
                break;
            default:
                break;
        }
    }

    public override string StateRepresentation => base.StateRepresentation + CharacterData;

    public override void UpdateHtml()
    {
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<{Element.LocalName}{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>" + Element.TextContent + (AnimationElements.Count > 0 ? "\n" : "") + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + $"</{Element.LocalName}>";
    }

    public static void AddNew(SVGEditor SVG)
    {
        IElement element = SVG.Document.CreateElement("TEXT");

        Text text = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "1",
            Fill = "lightgrey",
            CharacterData = "TEXT"
        };
        SVG.EditMode = EditMode.Add;

        (text.X, text.Y) = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));

        SVG.ClearSelectedShapes();
        SVG.SelectShape(text);
        SVG.AddElement(text);
    }

    public override void Complete()
    {
        throw new NotImplementedException();
    }

    public override void SnapToInteger()
    {
        X = (int)X;
        Y = (int)Y;
    }
}
