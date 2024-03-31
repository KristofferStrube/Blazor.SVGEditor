using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Circle : Shape
{
    public Circle(IElement element, SVGEditor svg) : base(element, svg) { }

    public override Type Presenter => typeof(CircleEditor);

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
    public double R
    {
        get => Element.GetAttributeOrZero("r");
        set { Element.SetAttribute("r", value.AsString()); Changed?.Invoke(this); }
    }

    public override List<(double x, double y)> SelectionPoints => [(Cx + R, Cy + R), (Cx + R, Cy - R), (Cx - R, Cy + R), (Cx - R, Cy - R)];

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.Add:
                R = Math.Sqrt(Math.Pow(Cx - x, 2) + Math.Pow(Cy - y, 2));
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
                        R = Math.Abs(x - Cx);
                        break;
                    case 2:
                    case 3:
                        R = Math.Abs(y - Cy);
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
            case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                SVG.EditMode = EditMode.None;
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
        IElement element = SVG.Document.CreateElement("CIRCLE");

        Circle circle = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "1",
            Fill = "lightgrey"
        };
        SVG.EditMode = EditMode.Add;

        (double x, double y) startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
        (circle.Cx, circle.Cy) = startPos;

        SVG.ClearSelectedShapes();
        SVG.SelectShape(circle);
        SVG.AddElement(circle);
    }

    public override void Complete()
    {
    }

    public override void SnapToInteger()
    {
        Cx = (int)Cx;
        Cy = (int)Cy;
        R = (int)R;
    }
}
