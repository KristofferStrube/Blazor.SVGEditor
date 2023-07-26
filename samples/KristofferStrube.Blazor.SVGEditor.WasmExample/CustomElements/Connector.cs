using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor.WasmExample.CustomElements;

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
            var from = (Node?)SVG.Elements.FirstOrDefault(e => e is Node && e.Id == Element.GetAttribute("data-from"));
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
                _ = Element.RemoveAttribute("data-from");
            }
            else
            {
                Element.SetAttribute("data-from", value.Id);
                _ = value.RelatedConnectors.Add(this);
            }
            Changed?.Invoke(this);
        }
    }

    public Node? To
    {
        get
        {
            var to = (Node?)SVG.Elements.FirstOrDefault(e => e is Node && e.Id == Element.GetAttribute("data-to"));
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
                _ = Element.RemoveAttribute("data-to");
            }
            else
            {
                Element.SetAttribute("data-to", value.Id);
                _ = value.RelatedConnectors.Add(this);
            }
            Changed?.Invoke(this);
        }
    }

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        if (SVG.EditMode is EditMode.Add)
        {
            (X2, Y2) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            SetStart((X2, Y2));
        }
    }

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        if (SVG.EditMode is EditMode.Add
            && SVG.SelectedShapes.FirstOrDefault(s => s is Node node && node != From) is Node { } to)
        {
            if (to.RelatedConnectors.Any(c => c.To == From || c.From == From))
            {
                Complete();
            }
            else
            {
                To = to;
                SVG.EditMode = EditMode.None;
                SVG.ClearSelectedShapes();
                UpdateLine();
            }
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

        SVG.ClearSelectedShapes();
        SVG.SelectShape(connector);
        SVG.AddElement(connector);
    }

    public void SetStart((double x, double y) towards)
    {
        double differenceX = towards.x - From!.Cx;
        double differenceY = towards.y - From!.Cy;
        double distance = Math.Sqrt((differenceX * differenceX) + (differenceY * differenceY));

        if (distance > 0)
        {
            X1 = From!.Cx + (differenceX / distance * 50);
            Y1 = From!.Cy + (differenceY / distance * 50);
        }
    }

    public void UpdateLine()
    {
        if (From is null || To is null)
        {
            (X1, Y1) = (X2, Y2);
            return;
        }

        double differenceX = To.Cx - From.Cx;
        double differenceY = To.Cy - From.Cy;
        double distance = Math.Sqrt((differenceX * differenceX) + (differenceY * differenceY));

        if (distance < 100)
        {
            (X1, Y1) = (X2, Y2);
        }
        else
        {
            SetStart((To.Cx, To.Cy));
            X2 = To.Cx - (differenceX / distance * 50);
            Y2 = To.Cy - (differenceY / distance * 50);
        }
    }
}
