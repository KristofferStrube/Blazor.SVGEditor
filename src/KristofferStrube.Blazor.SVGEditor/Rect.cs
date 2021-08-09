using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public double x
        {
            get { return double.Parse(Element.GetAttribute("x") ?? "0"); }
            set { Element.SetAttribute("x", value.ToString()); Changed.Invoke(this); }
        }
        public double y
        {
            get { return double.Parse(Element.GetAttribute("y") ?? "0"); }
            set { Element.SetAttribute("y", value.ToString()); Changed.Invoke(this); }
        }
        public double width
        {
            get { return double.Parse(Element.GetAttribute("width") ?? "0"); }
            set { Element.SetAttribute("width", value.ToString()); Changed.Invoke(this); }
        }
        public double height
        {
            get { return double.Parse(Element.GetAttribute("height") ?? "0"); }
            set { Element.SetAttribute("height", value.ToString()); Changed.Invoke(this); }
        }

        public int? CurrentAnchor { get; set; }

        private (double x, double y)? AddPos { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Add:
                    if (AddPos is null)
                    {
                        AddPos = (x, y);
                    }
                    if (pos.x < AddPos.Value.x)
                    {
                        x = pos.x;
                        width = AddPos.Value.x - pos.x;
                    }
                    else
                    {
                        x = AddPos.Value.x;
                        width = pos.x - AddPos.Value.x;
                    }
                    if (pos.y < AddPos.Value.y)
                    {
                        y = pos.y;
                        height = AddPos.Value.y - pos.y;
                    }
                    else
                    {
                        y = AddPos.Value.y;
                        height = pos.y - AddPos.Value.y;
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (x: pos.x, y: pos.y);
                    x += diff.x;
                    y += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    switch (CurrentAnchor)
                    {
                        case 0:
                            width -= pos.x - x;
                            height -= pos.y - y;
                            x = pos.x;
                            y = pos.y;
                            break;
                        case 1:
                            width = pos.x - x;
                            height -= pos.y - y;
                            y = pos.y;
                            break;
                        case 2:
                            width = pos.x - x;
                            height = pos.y - y;
                            break;
                        case 3:
                            width -= pos.x - x;
                            height = pos.y - y;
                            x = pos.x;
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
            var element = SVG.Document.CreateElement("RECT");

            var rect = new Rect(element, SVG);
            rect.Changed = SVG.UpdateInput;
            rect.Stroke = "black";
            rect.StrokeWidth = "1";
            rect.Fill = "lightgrey";
            rect.EditMode = EditMode.Add;

            (rect.x, rect.y) = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));

            SVG.CurrentShape = rect;
            SVG.AddElement(rect);
        }

        public override void Complete()
        {
            SVG.Remove(this);
        }
    }
}
