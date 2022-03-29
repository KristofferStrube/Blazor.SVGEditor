using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Rect : Shape
{
    public Rect(IElement element, SVG svg) : base(element, svg) { }

    public override Type Editor => typeof(RectEditor);

    public double X
    {
        get => Element.GetAttributeOrZero("x");
        set { Element.SetAttribute("x", value.AsString()); Changed.Invoke(this); }
    }
    public double Y
    {
        get => Element.GetAttributeOrZero("y");
        set { Element.SetAttribute("y", value.AsString()); Changed.Invoke(this); }
    }
    public double Width
    {
        get => Element.GetAttributeOrZero("width");
        set { Element.SetAttribute("width", value.AsString()); Changed.Invoke(this); }
    }
    public double Height
    {
        get => Element.GetAttributeOrZero("height");
        set { Element.SetAttribute("height", value.AsString()); Changed.Invoke(this); }
    }

    private (double x, double y)? AddPos { get; set; }

    public override List<(double x, double y)> SelectionPoints => new() { (X, Y), (X + Width, Y), (X + Width, Y + Height), (X, Y + Height) };

    public override void HandleMouseMove(MouseEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.Add:
                if (AddPos is null)
                {
                    AddPos = (X, Y);
                }
                if (x < AddPos.Value.x)
                {
                    X = x;
                    Width = AddPos.Value.x - x;
                }
                else
                {
                    X = AddPos.Value.x;
                    Width = x - AddPos.Value.x;
                }
                if (y < AddPos.Value.y)
                {
                    Y = y;
                    Height = AddPos.Value.y - y;
                }
                else
                {
                    Y = AddPos.Value.y;
                    Height = y - AddPos.Value.y;
                }
                break;
            case EditMode.Move:
                (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                X += diff.x;
                Y += diff.y;
                break;
            case EditMode.MoveAnchor:
                if (SVG.CurrentAnchor == null)
                {
                    SVG.CurrentAnchor = 0;
                }
                switch (SVG.CurrentAnchor)
                {
                    case 0:
                        Width -= x - X;
                        Height -= y - Y;
                        X = x;
                        Y = y;
                        break;
                    case 1:
                        Width = x - X;
                        Height -= y - Y;
                        Y = y;
                        break;
                    case 2:
                        Width = x - X;
                        Height = y - Y;
                        break;
                    case 3:
                        Width -= x - X;
                        Height = y - Y;
                        X = x;
                        break;
                }
                break;
        }
    }

    public override void HandleMouseUp(MouseEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                SVG.EditMode = EditMode.None;
                break;
        }
    }

    public override void HandleMouseOut(MouseEventArgs eventArgs)
    {
    }

    public static void AddNew(SVG SVG)
    {
        IElement element = SVG.Document.CreateElement("RECT");

        Rect rect = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "1",
            Fill = "lightgrey"
        };
        SVG.EditMode = EditMode.Add;

        (rect.X, rect.Y) = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));

        SVG.SelectedElements.Clear();
        SVG.SelectedElements.Add(rect);
        SVG.AddElement(rect);
    }

    public override void Complete()
    {
    }
}
