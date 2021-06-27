using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using KristofferStrube.Blazor.SVGEditor.Models;
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

        public void Select(MouseEventArgs eventArgs)
        {
            if (SVG.CurrentShape == null || SVG.CurrentShape.EditMode == EditMode.None)
            {
                SVG.CurrentShape = this;
                Panner = (x: eventArgs.OffsetX/SVG.Scale, y: eventArgs.OffsetY / SVG.Scale);
                EditMode = EditMode.Move;
            }
        }

        public void SelectAnchor(int anchor)
        {
            CurrentAnchor = anchor;
            EditMode = EditMode.MoveAnchor;
        }

        public (double x, double y) Cursor { get; set; }

        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = (x: eventArgs.OffsetX / SVG.Scale, y: eventArgs.OffsetY / SVG.Scale);
            switch (EditMode)
            {
                case EditMode.Add:
                    Cursor = (pos.x, pos.y);
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
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }
    }
}
