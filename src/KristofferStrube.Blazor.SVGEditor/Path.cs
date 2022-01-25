using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Path : Shape
    {
        public Path(IElement element, SVG svg) : base(element, svg)
        {
            try
            {
                Instructions = PathData.Parse(Element.GetAttributeOrEmpty("d"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Instructions = new();
            }
        }

        public override Type Editor => typeof(PathEditor);

        public List<IPathInstruction> Instructions { get; set; }

        public void UpdateData()
        {
            if (Instructions.Count > 0)
            {
                Element.SetAttribute("d", Instructions.AsString());
                Changed.Invoke(this);
            }
        }
        public int? CurrentInstruction;

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (SVG.EditMode)
            {
                case EditMode.MoveAnchor:
                    if (SVG.CurrentAnchor == null)
                    {
                        SVG.CurrentAnchor = -1;
                    }
                    IPathInstruction inst = Instructions[(int)CurrentInstruction];
                    double diffX = x - inst.EndPosition.x;
                    double diffY = y - inst.EndPosition.y;
                    IPathInstruction prev = inst.PreviousInstruction;
                    if (SVG.CurrentAnchor == -1)
                    {
                        inst.EndPosition = (x, y);
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
                                prev.EndPosition = (prev.EndPosition.x, prev.EndPosition.y + (y - inst.EndPosition.y));
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
                                prev.EndPosition = (prev.EndPosition.x + (x - inst.EndPosition.x), prev.EndPosition.y);
                                break;
                        }
                        if (inst.NextInstruction is not null and not ShorthandCubicBézierCurveInstruction and not QuadraticBézierCurveInstruction and not ShorthandQuadraticBézierCurveInstruction and BaseControlPointPathInstruction nextInst)
                        {
                            nextInst.ControlPoints[0] = (nextInst.ControlPoints[0].x + diffX, nextInst.ControlPoints[0].y + diffY);
                        }
                    }
                    else if (inst is BaseControlPointPathInstruction controlPointInstruction)
                    {
                        if (SVG.CurrentAnchor == -2)
                        {
                            controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = (x, y);
                        }
                        else
                        {
                            controlPointInstruction.ControlPoints[(int)SVG.CurrentAnchor] = (x, y);
                            controlPointInstruction.UpdateReflectionForInstructions();
                        }
                    }
                    else if (inst is EllipticalArcInstruction ellipticalArcInstruction)
                    {
                        switch (SVG.CurrentAnchor)
                        {
                            case 0:
                                ellipticalArcInstruction.ControlPointYPos = (x, y);
                                break;
                            case 1:
                                ellipticalArcInstruction.ControlPointYNeg = (x, y);
                                break;
                            case 2:
                                ellipticalArcInstruction.ControlPointXPos = (x, y);
                                break;
                            case 3:
                                ellipticalArcInstruction.ControlPointXNeg = (x, y);
                                break;
                        }
                    }
                    UpdateData();
                    break;
                case EditMode.Move:
                    (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                    UpdatePoints(((double x, double y) point) => (point.x + diff.x, point.y + diff.y), (1, 1));
                    UpdateData();
                    break;
                case EditMode.Add:
                    if (Instructions.Count == 0)
                    {
                        (double x, double y) startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
                        Instructions.Add(new MoveInstruction(startPos.x, startPos.y, false, null) { ExplicitSymbol = true });
                        Instructions.Add(new CubicBézierCurveInstruction(0, 0, 0, 0, 0, 0, false, Instructions.Last()) { ExplicitSymbol = true });
                    }
                    CubicBézierCurveInstruction currentInstruction = (CubicBézierCurveInstruction)Instructions[^1];
                    currentInstruction.EndPosition = (x, y);
                    currentInstruction.ControlPoints[0] = ((int)(currentInstruction.StartPosition.x * 2.0 / 3.0 + currentInstruction.EndPosition.x * 1.0 / 3.0), (int)(currentInstruction.StartPosition.y * 2.0 / 3.0 + currentInstruction.EndPosition.y * 1.0 / 3.0));
                    currentInstruction.ControlPoints[^1] = ((int)(currentInstruction.StartPosition.x * 1.0 / 3.0 + currentInstruction.EndPosition.x * 2.0 / 3.0), (int)(currentInstruction.StartPosition.y * 1.0 / 3.0 + currentInstruction.EndPosition.y * 2.0 / 3.0));
                    UpdateData();
                    break;
                case EditMode.Scale:
                    switch (SVG.CurrentAnchor)
                    {
                        case -1:
                            (double x, double y) moveDiff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                            SVG.MovePanner = (x, y);
                            UpdatePoints(((double x, double y) point) => (point.x + moveDiff.x, point.y + moveDiff.y), (1, 1));
                            BoundingBox.x += moveDiff.x;
                            BoundingBox.y += moveDiff.y;
                            break;
                        case 0:
                            switch ((width: BoundingBox.width + BoundingBox.x - x, height: BoundingBox.height + BoundingBox.y - y))
                            {
                                case var dim when dim.width < 0 && dim.height < 0:
                                    SVG.CurrentAnchor = 2;
                                    break;
                                case var dim when dim.width < 0:
                                    SVG.CurrentAnchor = 1;
                                    break;
                                case var dim when dim.height < 0:
                                    SVG.CurrentAnchor = 3;
                                    break;
                            }
                            Func<(double x, double y), (double, double)> topLeftScaler = ((double x, double y) point) => ((point.x - BoundingBox.x - BoundingBox.width) * (BoundingBox.width + BoundingBox.x - x) / BoundingBox.width + BoundingBox.x + BoundingBox.width, (point.y - BoundingBox.y - BoundingBox.height) * (BoundingBox.height + BoundingBox.y - y) / BoundingBox.height + BoundingBox.y + BoundingBox.height);
                            UpdatePoints(topLeftScaler, ((BoundingBox.width + BoundingBox.x - x) / BoundingBox.width, (BoundingBox.height + BoundingBox.y - y) / BoundingBox.height));
                            BoundingBox.width += BoundingBox.x - x;
                            BoundingBox.height += BoundingBox.y - y;
                            (BoundingBox.x, BoundingBox.y) = (x, y);
                            break;
                        case 1:
                            switch ((width: x - BoundingBox.x, height: BoundingBox.height + BoundingBox.y - y))
                            {
                                case var dim when dim.width < 0 && dim.height < 0:
                                    SVG.CurrentAnchor = 3;
                                    break;
                                case var dim when dim.width < 0:
                                    SVG.CurrentAnchor = 0;
                                    break;
                                case var dim when dim.height < 0:
                                    SVG.CurrentAnchor = 2;
                                    break;
                            }
                            Func<(double x, double y), (double, double)> topRightScaler = ((double x, double y) point) => ((point.x - BoundingBox.x) * (x - BoundingBox.x) / BoundingBox.width + BoundingBox.x, (point.y - BoundingBox.y - BoundingBox.height) * (BoundingBox.height + BoundingBox.y - y) / BoundingBox.height + BoundingBox.y + BoundingBox.height);
                            UpdatePoints(topRightScaler, (1, 1));
                            BoundingBox.width = x - BoundingBox.x;
                            BoundingBox.height += BoundingBox.y - y;
                            (BoundingBox.x, BoundingBox.y) = (x - BoundingBox.width, y);
                            break;
                        case 2:
                            switch ((width: x - BoundingBox.x, height: y - BoundingBox.y))
                            {
                                case var dim when dim.width < 0 && dim.height < 0:
                                    SVG.CurrentAnchor = 0;
                                    break;
                                case var dim when dim.width < 0:
                                    SVG.CurrentAnchor = 3;
                                    break;
                                case var dim when dim.height < 0:
                                    SVG.CurrentAnchor = 1;
                                    break;
                            }
                            Func<(double x, double y), (double, double)> bottomRightScaler = ((double x, double y) point) => ((point.x - BoundingBox.x) * (x - BoundingBox.x) / BoundingBox.width + BoundingBox.x, (point.y - BoundingBox.y) * (y - BoundingBox.y) / BoundingBox.height + BoundingBox.y);
                            UpdatePoints(bottomRightScaler, (1, 1));
                            BoundingBox.width = x - BoundingBox.x;
                            BoundingBox.height = y - BoundingBox.y;
                            (BoundingBox.x, BoundingBox.y) = (x - BoundingBox.width, y - BoundingBox.height);
                            break;
                        case 3:
                            switch ((width: BoundingBox.width + BoundingBox.x - x, height: y - BoundingBox.y))
                            {
                                case var dim when dim.width < 0 && dim.height < 0:
                                    SVG.CurrentAnchor = 1;
                                    break;
                                case var dim when dim.width < 0:
                                    SVG.CurrentAnchor = 2;
                                    break;
                                case var dim when dim.height < 0:
                                    SVG.CurrentAnchor = 0;
                                    break;
                            }
                            Func<(double x, double y), (double, double)> bottomLeftScaler = ((double x, double y) point) => ((point.x - BoundingBox.x - BoundingBox.width) * (BoundingBox.width + BoundingBox.x - x) / BoundingBox.width + BoundingBox.x + BoundingBox.width, (point.y - BoundingBox.y) * (y - BoundingBox.y) / BoundingBox.height + BoundingBox.y);
                            UpdatePoints(bottomLeftScaler, (1, 1));
                            BoundingBox.width += BoundingBox.x - x;
                            BoundingBox.height = y - BoundingBox.y;
                            (BoundingBox.x, BoundingBox.y) = (x, y - BoundingBox.height);
                            break;
                    }
                    UpdateData();
                    break;
            }
        }

        private void UpdatePoints(Func<(double, double), (double, double)> transformer, (double x, double y) scale)
        {
            Instructions = Instructions.Select(inst =>
            {
                inst.EndPosition = transformer(inst.EndPosition);
                if (inst is BaseControlPointPathInstruction controlPointInstruction)
                {
                    controlPointInstruction.ControlPoints = controlPointInstruction.ControlPoints.Select(p => transformer(p)).ToList();
                    controlPointInstruction.UpdateReflectionForInstructions();
                }
                else if (inst is EllipticalArcInstruction arcInstruction)
                {
                    arcInstruction.Rx += arcInstruction.Rx * Math.Cos(arcInstruction.XAxisRotation / 180 * Math.PI) * (scale.x - 1);
                    arcInstruction.Ry += arcInstruction.Ry * Math.Sin(arcInstruction.XAxisRotation / 180 * Math.PI) * (scale.y - 1);
                }
                return inst;
            }).ToList();
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (SVG.EditMode)
            {
                case EditMode.MoveAnchor:
                    SVG.CurrentAnchor = null;
                    SVG.EditMode = EditMode.None;
                    break;
                case EditMode.Move:
                    SVG.EditMode = EditMode.None;
                    break;
                case EditMode.Add:
                    CubicBézierCurveInstruction currentInstruction = (CubicBézierCurveInstruction)Instructions.Last();
                    CubicBézierCurveInstruction nextInstruction = new(x, y, x, y, x, y, false, Instructions.Last()) { ExplicitSymbol = true };
                    currentInstruction.EndPosition = (x, y);
                    currentInstruction.ControlPoints[0] = (currentInstruction.StartPosition.x * 2.0 / 3.0 + currentInstruction.EndPosition.x * 1.0 / 3.0, currentInstruction.StartPosition.y * 2.0 / 3.0 + currentInstruction.EndPosition.y * 1.0 / 3.0);
                    currentInstruction.ControlPoints[^1] = (currentInstruction.StartPosition.x * 1.0 / 3.0 + currentInstruction.EndPosition.x * 2.0 / 3.0, currentInstruction.StartPosition.y * 1.0 / 3.0 + currentInstruction.EndPosition.y * 2.0 / 3.0);
                    currentInstruction.NextInstruction = nextInstruction;
                    Instructions.Add(nextInstruction);
                    UpdateData();
                    break;
                case EditMode.Scale:
                    SVG.CurrentAnchor = null;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public static void AddNew(SVG SVG)
        {
            IElement element = SVG.Document.CreateElement("PATH");

            Path path = new(element, SVG)
            {
                Changed = SVG.UpdateInput,
                Stroke = "black",
                StrokeWidth = "1",
                Fill = "lightgrey"
            };
            SVG.EditMode = EditMode.Add;

            SVG.SelectedElements.Clear();
            SVG.SelectedElements.Add(path);
            SVG.AddElement(path);
        }

        public override void Complete()
        {
            Instructions.RemoveAt(Instructions.Count - 1);
            Instructions.Add(new ClosePathInstruction(false, Instructions.Last()));
            UpdateData();
        }
    }
}
