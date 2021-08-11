using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Path : Shape
    {
        public Path(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
            try
            {
                Instructions = PathData.Parse(Element.GetAttribute("d") ?? string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Instructions = new();
            }
        }

        public override Type Editor => typeof(PathEditor);

        public BoundingBox BBox { get; set; }

        public List<IPathInstruction> Instructions { get; set; }

        private void UpdateData()
        {
            if (Instructions.Count > 0)
            {
                Element.SetAttribute("d", Instructions.AsString());
                Changed.Invoke(this);
            }
        }
        public int? CurrentInstruction { get; set; }
        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = -1;
                    }
                    var inst = Instructions[(int)CurrentInstruction];
                    var diffX = pos.x - inst.EndPosition.x;
                    var diffY = pos.y - inst.EndPosition.y;
                    var prev = inst.PreviousInstruction;
                    if (CurrentAnchor == -1)
                    {
                        inst.EndPosition = (pos.x, pos.y);
                        switch (inst)
                        {
                            case BaseControlPointPathInstruction controlPointInstruction:
                                if (inst is CubicBézierCurveInstruction or ShorthandCubicBézierCurveInstruction)
                                {
                                    controlPointInstruction.ControlPoints[^1] = (controlPointInstruction.ControlPoints[^1].x + diffX, controlPointInstruction.ControlPoints[^1].y + diffY);
                                }
                                controlPointInstruction.UpdateReflectionForInstructions();
                                break;
                            case HorizontalLineInstruction:
                                while (prev is HorizontalLineInstruction)
                                {
                                    prev = prev.PreviousInstruction;
                                }
                                if (prev is ClosePathInstruction closeBeforeHorizontal)
                                {
                                    prev = closeBeforeHorizontal.GetReferenceInstruction();
                                }
                                prev.EndPosition = (prev.EndPosition.x, prev.EndPosition.y + (pos.y - inst.EndPosition.y));
                                break;
                            case VerticalLineInstruction:
                                while (prev is VerticalLineInstruction)
                                {
                                    prev = prev.PreviousInstruction;
                                }
                                if (prev is ClosePathInstruction closeBeforeVertical)
                                {
                                    prev = closeBeforeVertical.GetReferenceInstruction();
                                }
                                prev.EndPosition = (prev.EndPosition.x + (pos.x - inst.EndPosition.x), prev.EndPosition.y);
                                break;
                        }
                        if (inst.NextInstruction is not null and not ShorthandCubicBézierCurveInstruction and not QuadraticBézierCurveInstruction and not ShorthandQuadraticBézierCurveInstruction and BaseControlPointPathInstruction nextInst)
                        {
                            nextInst.ControlPoints[0] = (nextInst.ControlPoints[0].x + diffX, nextInst.ControlPoints[0].y + diffY);
                        }
                    }
                    else if (inst is BaseControlPointPathInstruction controlPointInstruction)
                    {
                        if (CurrentAnchor == -2)
                        {
                            controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = (pos.x, pos.y);
                        }
                        else
                        {
                            controlPointInstruction.ControlPoints[(int)CurrentAnchor] = (pos.x, pos.y);
                            controlPointInstruction.UpdateReflectionForInstructions();
                        }
                    }
                    else if (inst is EllipticalArcInstruction ellipticalArcInstruction)
                    {
                        switch (CurrentAnchor)
                        {
                            case 0:
                                ellipticalArcInstruction.ControlPointYPos = (pos.x, pos.y);
                                break;
                            case 1:
                                ellipticalArcInstruction.ControlPointYNeg = (pos.x, pos.y);
                                break;
                            case 2:
                                ellipticalArcInstruction.ControlPointXPos = (pos.x, pos.y);
                                break;
                            case 3:
                                ellipticalArcInstruction.ControlPointXNeg = (pos.x, pos.y);
                                break;
                        }
                    }
                    UpdateData();
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (pos.x, pos.y);
                    Instructions = Instructions.Select(inst =>
                    {
                        inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y);
                        if (inst is BaseControlPointPathInstruction controlPointInstruction)
                        {
                            controlPointInstruction.ControlPoints = controlPointInstruction.ControlPoints.Select(p => (p.x + diff.x, p.y + diff.y)).ToList();
                            controlPointInstruction.UpdateReflectionForInstructions();
                        }
                        return inst;
                    }).ToList();
                    UpdateData();
                    break;
                case EditMode.Add:
                    if (Instructions.Count == 0)
                    {
                        var startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
                        Instructions.Add(new MoveInstruction(startPos.x, startPos.y, false, null) { ExplicitSymbol = true });
                        Instructions.Add(new CubicBézierCurveInstruction(0, 0, 0, 0, 0, 0, false, Instructions.Last()) { ExplicitSymbol = true });
                    }
                    var currentInstruction = (CubicBézierCurveInstruction)Instructions[^1];
                    currentInstruction.EndPosition = (pos.x, pos.y);
                    currentInstruction.ControlPoints[0] = ((int)(currentInstruction.StartPosition.x * 2.0 / 3.0 + currentInstruction.EndPosition.x * 1.0 / 3.0), (int)(currentInstruction.StartPosition.y * 2.0 / 3.0 + currentInstruction.EndPosition.y * 1.0 / 3.0));
                    currentInstruction.ControlPoints[^1] = ((int)(currentInstruction.StartPosition.x * 1.0 / 3.0 + currentInstruction.EndPosition.x * 2.0 / 3.0), (int)(currentInstruction.StartPosition.y * 1.0 / 3.0 + currentInstruction.EndPosition.y * 2.0 / 3.0));
                    UpdateData();
                    break;
                case EditMode.Scale:
                    switch (CurrentAnchor)
                    {
                        case 0:

                            break;
                    }
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    CurrentAnchor = null;
                    EditMode = EditMode.None;
                    break;
                case EditMode.Move:
                    EditMode = EditMode.None;
                    break;
                case EditMode.Add:
                    var currentInstruction = (CubicBézierCurveInstruction)Instructions.Last();
                    var nextInstruction = new CubicBézierCurveInstruction(pos.x, pos.y, pos.x, pos.y, pos.x, pos.y, false, Instructions.Last()) { ExplicitSymbol = true };
                    currentInstruction.EndPosition = (pos.x, pos.y);
                    currentInstruction.ControlPoints[0] = (currentInstruction.StartPosition.x * 2.0 / 3.0 + currentInstruction.EndPosition.x * 1.0 / 3.0, currentInstruction.StartPosition.y * 2.0 / 3.0 + currentInstruction.EndPosition.y * 1.0 / 3.0);
                    currentInstruction.ControlPoints[^1] = (currentInstruction.StartPosition.x * 1.0 / 3.0 + currentInstruction.EndPosition.x * 2.0 / 3.0, currentInstruction.StartPosition.y * 1.0 / 3.0 + currentInstruction.EndPosition.y * 2.0 / 3.0);
                    currentInstruction.NextInstruction = nextInstruction;
                    Instructions.Add(nextInstruction);
                    UpdateData();
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public static void AddNew(SVG SVG)
        {
            var element = SVG.Document.CreateElement("PATH");

            var path = new Path(element, SVG);
            path.Changed = SVG.UpdateInput;
            path.Stroke = "black";
            path.StrokeWidth = "1";
            path.Fill = "lightgrey";
            path.EditMode = EditMode.Add;

            SVG.CurrentShape = path;
            SVG.AddElement(path);
        }

        public override void Complete()
        {
            Instructions.RemoveAt(Instructions.Count - 1);
            Instructions.Add(new ClosePathInstruction(false, Instructions.Last()));
            UpdateData();
            SVG.UpdateInput(this);
        }
    }
}
