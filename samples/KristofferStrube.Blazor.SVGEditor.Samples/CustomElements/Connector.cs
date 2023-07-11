using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor.Samples.CustomElements;

public class Connector : Line
{
    public Connector(IElement element, SVGEditor svg) : base(element, svg)
    {
        UpdateLine();
    }

    public override Type Presenter => typeof(ConnectorEditor);

    public Node? From
    {
        get
        {
            var from = (Node?)SVG.Elements.FirstOrDefault(e => e is Node && e.Id == Element.GetAttribute("from"));
            _ = from?.RelatedConnectors.Add(this);
            return from;
        }
        set
        {
            if (From is { } from)
            {
                _ = from.RelatedConnectors.Remove(this);
            }
            if (value is null)
            {
                _ = Element.RemoveAttribute("from");
            }
            else
            {
                Element.SetAttribute("from", value.Id);
            }
            Changed?.Invoke(this);
        }
    }

    public Node? To
    {
        get
        {
            var to = (Node?)SVG.Elements.FirstOrDefault(e => e is Node && e.Id == Element.GetAttribute("to"));
            _ = to?.RelatedConnectors.Add(this);
            return to;
        }
        set
        {
            if (To is { } to)
            {
                _ = to.RelatedConnectors.Remove(this);
            }
            if (value is null)
            {
                _ = Element.RemoveAttribute("to");
            }
            else
            {
                Element.SetAttribute("to", value.Id);
            }
            Changed?.Invoke(this);
        }
    }

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Add:
                (X2, Y2) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                SetStart((X2, Y2));
                break;
            case EditMode.MoveAnchor or EditMode.Move or EditMode.None or EditMode.Scale:
                break;
            default:
                break;
        }
    }

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Add:
                if (SVG.SelectedShapes.FirstOrDefault(s => s is Node node
                    && node != From) is Node { } to)
                {
                    if (to.RelatedConnectors.Any(c => c.To == From || c.From == From))
                    {
                        Complete();
                    }
                    else
                    {
                        To = to;
                        SVG.EditMode = EditMode.None;
                        UpdateLine();
                    }
                }
                break;
            case EditMode.Move or EditMode.MoveAnchor:
                SVG.EditMode = EditMode.None;
                break;
            default:
                break;
        }
    }

    public override void Complete()
    {
        if (To is null)
        {
            SVG.RemoveElement(this);
            Changed?.Invoke(this);
        }
    }

    public static void AddNew(SVGEditor SVG, Node from)
    {
        IElement element = SVG.Document.CreateElement("LINE");
        element.SetAttribute("data-elementtype", "connector");

        Connector connector = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "black",
            StrokeWidth = "5",
            From = from
        };
        SVG.EditMode = EditMode.Add;

        (connector.X2, connector.Y2) = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
        connector.SetStart((connector.X2, connector.Y2));

        SVG.ClearSelectedShapes();
        SVG.SelectShape(connector);
        SVG.AddElement(connector);
    }

    public void SetStart((double x, double y) to)
    {
        if (From is null)
        {
            return;
        }

        if (From.Cx == SVG.LastRightClick.x && From.Cy == SVG.LastRightClick.y)
        {
            (X1, Y1) = (From.Cx, From.Cy);
        }
        else
        {
            double a = to.x - From.Cx;
            double b = to.y - From.Cy;
            double length = Math.Sqrt((a * a) + (b * b));
            (X1, Y1) = (From.Cx + (a / length * 50), From.Cy + (b / length * 50));
        }
    }

    public void UpdateLine()
    {
        if (From is null || To is null)
        {
            (X1, Y1) = (X2, Y2);
            return;
        }

        double a = From.Cx - To.Cx;
        double b = From.Cy - To.Cy;
        double length = Math.Sqrt((a * a) + (b * b));
        (X2, Y2) = (To.Cx + (a / length * 50), To.Cy + (b / length * 50));
        if (length < 100)
        {
            (X1, Y1) = (X2, Y2);
        }
        else
        {
            SetStart((X2, Y2));
        }
    }
}
