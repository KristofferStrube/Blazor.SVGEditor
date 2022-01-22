using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Line : Shape
    {
        public Line(IElement element, SVG svg) : base(element, svg) { }

        public override Type Editor => typeof(LineEditor);

        public double x1
        {
            get { return Element.GetAttributeOrZero("x1"); }
            set { Element.SetAttribute("x1", value.AsString()); Changed.Invoke(this); }
        }
        public double y1
        {
            get { return Element.GetAttributeOrZero("y1"); }
            set { Element.SetAttribute("y1", value.AsString()); Changed.Invoke(this); }
        }
        public double x2
        {
            get { return Element.GetAttributeOrZero("x2"); }
            set { Element.SetAttribute("x2", value.AsString()); Changed.Invoke(this); }
        }
        public double y2
        {
            get { return Element.GetAttributeOrZero("y2"); }
            set { Element.SetAttribute("y2", value.AsString()); Changed.Invoke(this); }
        }

        private (double x, double y) AddPos { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (SVG.EditMode)
            {
                case EditMode.Add:
                    (x2, y2) = pos;
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - SVG.MovePanner.x, y: pos.y - SVG.MovePanner.y);
                    x1 += diff.x;
                    y1 += diff.y;
                    x2 += diff.x;
                    y2 += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (SVG.CurrentAnchor == null)
                    {
                        SVG.CurrentAnchor = 0;
                    }
                    switch (SVG.CurrentAnchor)
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
            var element = SVG.Document.CreateElement("LINE");

            var line = new Line(element, SVG);
            line.Changed = SVG.UpdateInput;
            line.Stroke = "black";
            line.StrokeWidth = "3";
            SVG.EditMode = EditMode.Add;

            var start = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
            (line.x1, line.y1) = start;
            (line.x2, line.y2) = start;

            SVG.SelectedElements.Clear();
            SVG.SelectedElements.Add(line);
            SVG.AddElement(line);
        }

        public override void Complete()
        {
        }
    }
}
