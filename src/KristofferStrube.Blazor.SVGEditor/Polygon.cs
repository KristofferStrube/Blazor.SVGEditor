using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor.Models
{
    public class Polygon : Shape
    {
        public Polygon(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
            Points = StringToPoints(Element.GetAttribute("points") ?? string.Empty);
        }

        public List<(double x, double y)> Points { get; set; }

        private void UpdatePoints()
        {
            Element.SetAttribute("points", PointsToString(Points));
            Changed.Invoke(this);
        }

        public string PointsToString(List<(double x, double y)> points)
        {
            return string.Join(" ", points.Select(point => $"{point.x.AsString()},{point.y.AsString()}"));
        }
        public List<(double x, double y)> StringToPoints(string points)
        {
            if (points == string.Empty)
            {
                return new();
            }
            return points.Split(" ").Select(p => (x: double.Parse(p.Split(",")[0]), y: double.Parse(p.Split(",")[1]))).ToList();
        }

        public (double x, double y) Cursor { get; set; }

        public int? CurrentAnchor { get; set; }

        public int? TempPoint { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = (x: eventArgs.OffsetX / SVG.Scale, y: eventArgs.OffsetY / SVG.Scale);
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    Points[(int)CurrentAnchor] = (pos.x, pos.y);
                    UpdatePoints();
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (x: pos.x, y: pos.y);
                    Points = Points.Select(point => { point.x += diff.x; point.y += diff.y; return point; }).ToList();
                    UpdatePoints();
                    break;
                case EditMode.Add:
                    if (TempPoint == null)
                    {
                        TempPoint = Points.Count();
                        Points.Add((x: pos.x, y: pos.y));
                    }
                    else
                    {
                        Points[(int)TempPoint] = (x: pos.x, y: pos.y);
                    }
                    break;
                case EditMode.None:
                    if (TempPoint != null)
                    {
                        Points.RemoveAt((int)TempPoint);
                        TempPoint = null;
                    }
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            var pos = (x: eventArgs.OffsetX / SVG.Scale, y: eventArgs.OffsetY / SVG.Scale);
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (pos.x < 50 && pos.y < 50)
                    {
                        Points.RemoveAt((int)CurrentAnchor);
                        UpdatePoints();
                    }
                    CurrentAnchor = null;
                    EditMode = EditMode.None;
                    if (Points.Count() == 0)
                    {
                        SVG.Elements.Remove(this);
                        SVG.CurrentShape = null;
                        Changed.Invoke(this);
                    }
                    break;
                case EditMode.Move:
                    EditMode = EditMode.None;
                    break;
                case EditMode.Add:
                    TempPoint = null;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }
    }
}
