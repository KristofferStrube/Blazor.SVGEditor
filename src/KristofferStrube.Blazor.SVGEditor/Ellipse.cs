using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Ellipse : Shape
    {
        public Ellipse(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
        }

        public override Type Editor => typeof(EllipseEditor);

        public double cx
        {
            get { return (Element.GetAttribute("cx") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("cx", value.AsString()); Changed.Invoke(this); }
        }
        public double cy
        {
            get { return (Element.GetAttribute("cy") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("cy", value.AsString()); Changed.Invoke(this); }
        }
        public double rx
        {
            get { return (Element.GetAttribute("rx") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("rx", value.AsString()); Changed.Invoke(this); }
        }
        public double ry
        {
            get { return (Element.GetAttribute("ry") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("ry", value.AsString()); Changed.Invoke(this); }
        }

        private double r { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Add:
                    if (r == 0)
                    {
                        rx = Math.Sqrt(Math.Pow(cx - pos.x, 2) + Math.Pow(cy - pos.y, 2));
                        ry = Math.Sqrt(Math.Pow(cx - pos.x, 2) + Math.Pow(cy - pos.y, 2));
                    }
                    else
                    {
                        if (Math.Abs(pos.x - cx) < Math.Abs(pos.y - cy))
                        {
                            rx = r;
                            ry = Math.Abs(pos.y - cy);
                        }
                        else
                        {
                            rx = Math.Abs(pos.x - cx);
                            ry = r;
                        }
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (pos.x, y: pos.y);
                    cx += diff.x;
                    cy += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    switch (CurrentAnchor)
                    {
                        case 0:
                        case 1:
                            rx = Math.Abs(pos.x - cx);
                            break;
                        case 2:
                        case 3:
                            ry = Math.Abs(pos.y - cy);
                            break;
                    }
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Move or EditMode.MoveAnchor:
                    EditMode = EditMode.None;
                    break;
                case EditMode.Add:
                    if (r == 0)
                    {
                        r = rx;
                    }
                    else
                    {
                        EditMode = EditMode.None;
                    }
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public static void AddNew(SVG SVG)
        {
            var element = SVG.Document.CreateElement("ELLIPSE");

            var ellipse = new Ellipse(element, SVG);
            ellipse.Changed = SVG.UpdateInput;
            ellipse.Stroke = "black";
            ellipse.StrokeWidth = "1";
            ellipse.Fill = "lightgrey";
            ellipse.EditMode = EditMode.Add;

            var startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
            (ellipse.cx, ellipse.cy) = startPos;

            SVG.CurrentShape = ellipse;
            SVG.AddElement(ellipse);
        }

        public override void Complete()
        {
            SVG.Remove(this);
        }
    }
}
