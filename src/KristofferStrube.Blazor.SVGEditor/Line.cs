using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Line : Shape
    {
        public Line(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
        }

        public override Type Editor => typeof(LineEditor);

        public double x1
        {
            get { return (Element.GetAttribute("x1") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("x1", value.AsString()); Changed.Invoke(this); }
        }
        public double y1
        {
            get { return (Element.GetAttribute("y1") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("y1", value.AsString()); Changed.Invoke(this); }
        }
        public double x2
        {
            get { return (Element.GetAttribute("x2") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("x2", value.AsString()); Changed.Invoke(this); }
        }
        public double y2
        {
            get { return (Element.GetAttribute("y2") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("y2", value.AsString()); Changed.Invoke(this); }
        }

        private (double x, double y) AddPos { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Add:
                    (x2, y2) = pos;
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (x: pos.x, y: pos.y);
                    x1 += diff.x;
                    y1 += diff.y;
                    x2 += diff.x;
                    y2 += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    switch (CurrentAnchor)
                    {
                        case 0:
                            (x1, y1) = pos;
                            break;
                        case 1:
                            (x2, y2) = pos;
                            break;
                    }
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                    EditMode = EditMode.None;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public static void AddNew(SVG SVG)
        {
            var element = SVG.Document.CreateElement("LINE");

            var line = new Line(element, SVG);
            line.Changed = SVG.UpdateInput;
            line.Stroke = "black";
            line.StrokeWidth = "3";
            line.EditMode = EditMode.Add;

            var start = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
            (line.x1, line.y1) = start;
            (line.x2, line.y2) = start;

            SVG.CurrentShape = line;
            SVG.AddElement(line);
        }

        public override void Complete()
        {
            SVG.Remove(this);
        }
    }
}
