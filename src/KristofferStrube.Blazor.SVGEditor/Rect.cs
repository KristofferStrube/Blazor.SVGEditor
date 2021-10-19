using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Rect : Shape
    {
        public Rect(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
        }

        public override Type Editor => typeof(RectEditor);

        public double X
        {
            get { return (Element.GetAttribute("x") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("x", value.AsString()); Changed.Invoke(this); }
        }
        public double Y
        {
            get { return (Element.GetAttribute("y") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("y", value.AsString()); Changed.Invoke(this); }
        }
        public double Width
        {
            get { return (Element.GetAttribute("width") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("width", value.AsString()); Changed.Invoke(this); }
        }
        public double Height
        {
            get { return (Element.GetAttribute("height") ?? "0").ParseAsDouble(); }
            set { Element.SetAttribute("height", value.AsString()); Changed.Invoke(this); }
        }

        private (double x, double y)? AddPos { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (SVG.EditMode)
            {
                case EditMode.Add:
                    if (AddPos is null)
                    {
                        AddPos = (X, Y);
                    }
                    if (pos.x < AddPos.Value.x)
                    {
                        X = pos.x;
                        Width = AddPos.Value.x - pos.x;
                    }
                    else
                    {
                        X = AddPos.Value.x;
                        Width = pos.x - AddPos.Value.x;
                    }
                    if (pos.y < AddPos.Value.y)
                    {
                        Y = pos.y;
                        Height = AddPos.Value.y - pos.y;
                    }
                    else
                    {
                        Y = AddPos.Value.y;
                        Height = pos.y - AddPos.Value.y;
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (x: pos.x, y: pos.y);
                    X += diff.x;
                    Y += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    switch (CurrentAnchor)
                    {
                        case 0:
                            Width -= pos.x - X;
                            Height -= pos.y - Y;
                            X = pos.x;
                            Y = pos.y;
                            break;
                        case 1:
                            Width = pos.x - X;
                            Height -= pos.y - Y;
                            Y = pos.y;
                            break;
                        case 2:
                            Width = pos.x - X;
                            Height = pos.y - Y;
                            break;
                        case 3:
                            Width -= pos.x - X;
                            Height = pos.y - Y;
                            X = pos.x;
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
            var element = SVG.Document.CreateElement("RECT");

            var rect = new Rect(element, SVG);
            rect.Changed = SVG.UpdateInput;
            rect.Stroke = "black";
            rect.StrokeWidth = "1";
            rect.Fill = "lightgrey";
            SVG.EditMode = EditMode.Add;

            (rect.X, rect.Y) = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));

            SVG.SelectedElements.Clear();
            SVG.SelectedElements.Add(rect);
            SVG.AddElement(rect);
        }

        public override void Complete()
        {
            SVG.Remove(this);
        }
    }
}
