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
        public List<(double x, double y)> Points {
            get { return StringToPoints(Element.GetAttribute("points") ?? string.Empty); }
            set { Element.SetAttribute("points", PointsToString(value)); Changed.Invoke(this); }
        }

        public string PointsToString(List<(double x, double y)> points)
        {
            return string.Join(" ", points.Select(point => $"{point.x},{point.y}"));
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
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    var _points = Points;
                    _points[(int)CurrentAnchor] = (eventArgs.OffsetX, eventArgs.OffsetY);
                    Points = _points;
                    break;
                case EditMode.Move:
                    var diff = (x: eventArgs.OffsetX - Panner.x, y: eventArgs.OffsetY - Panner.y);
                    Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                    Points = Points.Select(point => { point.x += diff.x; point.y += diff.y; return point; }).ToList();
                    break;
                case EditMode.Add:
                    if (TempPoint == null)
                    {
                        TempPoint = Points.Count();
                        Points.Add((x: eventArgs.OffsetX, y: eventArgs.OffsetY));
                    }
                    else
                    {
                        Points[(int)TempPoint] = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
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
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (eventArgs.OffsetX < 50 && eventArgs.OffsetY < 50)
                    {
                        var _points = Points;
                        _points.RemoveAt((int)CurrentAnchor);
                        Points = _points;
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
