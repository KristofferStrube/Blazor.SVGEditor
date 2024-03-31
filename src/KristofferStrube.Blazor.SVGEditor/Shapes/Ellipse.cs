using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Ellipse : Shape
{
    public Ellipse(IElement element, SVGEditor svg) : base(element, svg) { }

    public override Type Presenter => typeof(EllipseEditor);

    public double Cx
    {
        get => Element.GetAttributeOrZero("cx");
        set { Element.SetAttribute("cx", value.AsString()); Changed?.Invoke(this); }
    }
    public double Cy
    {
        get => Element.GetAttributeOrZero("cy");
        set { Element.SetAttribute("cy", value.AsString()); Changed?.Invoke(this); }
    }
    public double Rx
    {
        get => Element.GetAttributeOrZero("rx");
        set { Element.SetAttribute("rx", value.AsString()); Changed?.Invoke(this); }
    }
    public double Ry
    {
        get => Element.GetAttributeOrZero("ry");
        set { Element.SetAttribute("ry", value.AsString()); Changed?.Invoke(this); }
    }

    public override List<(double x, double y)> SelectionPoints => [(Cx + Rx, Cy + Ry), (Cx + Rx, Cy - Ry), (Cx - Rx, Cy + Ry), (Cx - Rx, Cy - Ry)];

    private double _r;

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.Add:
                if (_r == 0)
                {
                    Rx = Math.Sqrt(Math.Pow(Cx - x, 2) + Math.Pow(Cy - y, 2));
                    Ry = Math.Sqrt(Math.Pow(Cx - x, 2) + Math.Pow(Cy - y, 2));
                }
                else
                {
                    if (Math.Abs(x - Cx) < Math.Abs(y - Cy))
                    {
                        Rx = _r;
                        Ry = Math.Abs(y - Cy);
                    }
                    else
                    {
                        Rx = Math.Abs(x - Cx);
                        Ry = _r;
                    }
                }
                break;
            case EditMode.Move:
                (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                Cx += diff.x;
                Cy += diff.y;
                break;
            case EditMode.MoveAnchor:
                if (SVG.CurrentAnchor == null)
                {
                    SVG.CurrentAnchor = 0;
                }
                switch (SVG.CurrentAnchor)
                {
                    case 0:
                    case 1:
                        Rx = Math.Abs(x - Cx);
                        break;
                    case 2:
                    case 3:
                        Ry = Math.Abs(y - Cy);
                        break;
                    default:
                        break;
                }
                break;
            case EditMode.None:
                break;
            case EditMode.Scale:
                break;
            default:
                break;
        }
    }

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Move or EditMode.MoveAnchor:
                SVG.EditMode = EditMode.None;
                break;
            case EditMode.Add:
                if (_r == 0)
                {
                    _r = Rx;
                }
                else
                {
                    SVG.EditMode = EditMode.None;
                }
                break;
            default:
                break;
        }
    }

    public override void HandlePointerOut(PointerEventArgs eventArgs)
    {
    }

    public static void AddNew(SVGEditor SVG)
    {
        IElement element = SVG.Document.CreateElement("ELLIPSE");

        Ellipse ellipse = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "1",
            Fill = "lightgrey"
        };
        SVG.EditMode = EditMode.Add;

        (double x, double y) startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
        (ellipse.Cx, ellipse.Cy) = startPos;

        SVG.ClearSelectedShapes();
        SVG.SelectShape(ellipse);
        SVG.AddElement(ellipse);
    }

    public override void Complete()
    {
    }

    public override void SnapToInteger()
    {
        Cx = (int)Cx;
        Cy = (int)Cy;
        Rx = (int)Rx;
        Ry = (int)Ry;
    }
}
