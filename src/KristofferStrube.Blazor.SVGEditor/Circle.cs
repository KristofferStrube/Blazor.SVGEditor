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
    public class Circle : Shape
    {
        public Circle(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
        }

        public override Type Editor => typeof(CircleEditor);

        public double cx {
            get { return double.Parse(Element.GetAttribute("cx") ?? string.Empty); }
            set { Element.SetAttribute("cx", value.ToString()); Changed.Invoke(this); }
        }
        public double cy
        {
            get { return double.Parse(Element.GetAttribute("cy") ?? string.Empty); }
            set { Element.SetAttribute("cy", value.ToString()); Changed.Invoke(this); }
        }
        public double r
        {
            get { return double.Parse(Element.GetAttribute("r") ?? string.Empty); }
            set { Element.SetAttribute("r", value.ToString()); Changed.Invoke(this); }
        }

        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Add:
                    if (cx == 0 && cy == 0)
                    {
                        var startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
                        (cx, cy) = startPos;
                    }
                    r = Math.Sqrt(Math.Pow(cx-pos.x,2) + Math.Pow(cy - pos.y, 2));
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
                            r = Math.Abs(pos.x - cx);
                            break;
                        case 2:
                        case 3:
                            r = Math.Abs(pos.y - cy);
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
                    EditMode = EditMode.None;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public new static Action<SVG> AddNew = SVG =>
        {
            var element = SVG.Document.CreateElement("CIRCLE");

            var circle = new Circle(element, SVG);
            circle.Changed = SVG.UpdateInput;
            circle.Stroke = "black";
            circle.StrokeWidth = "1";
            circle.Fill = "lightgrey";
            circle.EditMode = EditMode.Add;

            SVG.CurrentShape = circle;
            SVG.AddElement(circle);
        };

        public override void Complete()
        {
            RemoveThis();
        }

        public void RemoveThis()
        {
            SVG.ElementsAsHtml.RemoveAt(SVG.Elements.IndexOf(this));
            SVG.Elements.Remove(this);
            _StateRepresentation = null;
            SVG.CurrentShape = null;
            SVG.UpdateInput();
            SVG.RerenderAll();
        }
    }
}
