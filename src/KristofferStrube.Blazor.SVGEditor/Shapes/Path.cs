using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor;

public class Path : Shape
{
    public Path(IElement element, SVGEditor svg) : base(element, svg)
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

    public override Type Presenter => typeof(PathEditor);

    public List<IPathInstruction> Instructions { get; set; }

    public override IEnumerable<(double x, double y)> SelectionPoints => Instructions.Count > 0 ? Instructions.Where(inst => inst is not MoveInstruction).SelectMany(inst => inst.SelectionPoints) : Enumerable.Empty<(double x, double y)>();

    public void UpdateData()
    {
        if (Instructions.Count > 0)
        {
            BaseAnimate? editingAnimation = AnimationElements.FirstOrDefault(a => a.IsEditing("d"));
            if (editingAnimation is not null)
            {
                editingAnimation.Values[editingAnimation.CurrentFrame!.Value] = Instructions.AsString();
                editingAnimation.UpdateValues();
            }
            else
            {
                Element.SetAttribute("d", Instructions.AsString());
            }
            Changed?.Invoke(this);
        }
    }

    public int? CurrentInstruction;

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.MoveAnchor:
                if (SVG.CurrentAnchor == null)
                {
                    SVG.CurrentAnchor = -1;
                }
                IPathInstruction inst = Instructions[CurrentInstruction!.Value];
                double diffX = x - inst.EndPosition.x;
                double diffY = y - inst.EndPosition.y;
                IPathInstruction prev = inst.PreviousInstruction!;

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
                                prev = prev.PreviousInstruction!;
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
                                prev = prev.PreviousInstruction!;
                            }
                            if (prev is ClosePathInstruction closeBeforeVertical)
                            {
                                prev = closeBeforeVertical.GetReferenceInstruction();
                            }
                            prev.EndPosition = (prev.EndPosition.x + (x - inst.EndPosition.x), prev.EndPosition.y);
                            break;
                        default:
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
                    }
                    controlPointInstruction.UpdateReflectionForInstructions();
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
                        default:
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
                var currentInstruction = (CubicBézierCurveInstruction)Instructions[^1];
                currentInstruction.EndPosition = (x, y);
                currentInstruction.ControlPoints[0] = ((int)((currentInstruction.StartPosition.x * 2.0 / 3.0) + (currentInstruction.EndPosition.x * 1.0 / 3.0)), (int)((currentInstruction.StartPosition.y * 2.0 / 3.0) + (currentInstruction.EndPosition.y * 1.0 / 3.0)));
                currentInstruction.ControlPoints[^1] = ((int)((currentInstruction.StartPosition.x * 1.0 / 3.0) + (currentInstruction.EndPosition.x * 2.0 / 3.0)), (int)((currentInstruction.StartPosition.y * 1.0 / 3.0) + (currentInstruction.EndPosition.y * 2.0 / 3.0)));
                UpdateData();
                break;
            case EditMode.Scale:
                switch (SVG.CurrentAnchor)
                {
                    case -1:
                        (double x, double y) moveDiff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                        SVG.MovePanner = (x, y);
                        UpdatePoints(((double x, double y) point) => (point.x + moveDiff.x, point.y + moveDiff.y), (1, 1));
                        BoundingBox.X += moveDiff.x;
                        BoundingBox.Y += moveDiff.y;
                        break;
                    case 0:
                        switch ((width: BoundingBox.Width + BoundingBox.X - x, height: BoundingBox.Height + BoundingBox.Y - y))
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
                            default:
                                break;
                        }
                        Func<(double x, double y), (double, double)> topLeftScaler = ((double x, double y) point) => (((point.x - BoundingBox.X - BoundingBox.Width) * (BoundingBox.Width + BoundingBox.X - x) / BoundingBox.Width) + BoundingBox.X + BoundingBox.Width, ((point.y - BoundingBox.Y - BoundingBox.Height) * (BoundingBox.Height + BoundingBox.Y - y) / BoundingBox.Height) + BoundingBox.Y + BoundingBox.Height);
                        UpdatePoints(topLeftScaler, ((BoundingBox.Width + BoundingBox.X - x) / BoundingBox.Width, (BoundingBox.Height + BoundingBox.Y - y) / BoundingBox.Height));
                        BoundingBox.Width += BoundingBox.X - x;
                        BoundingBox.Height += BoundingBox.Y - y;
                        (BoundingBox.X, BoundingBox.Y) = (x, y);
                        break;
                    case 1:
                        switch ((width: x - BoundingBox.X, height: BoundingBox.Height + BoundingBox.Y - y))
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
                            default:
                                break;
                        }
                        Func<(double x, double y), (double, double)> topRightScaler = ((double x, double y) point) => (((point.x - BoundingBox.X) * (x - BoundingBox.X) / BoundingBox.Width) + BoundingBox.X, ((point.y - BoundingBox.Y - BoundingBox.Height) * (BoundingBox.Height + BoundingBox.Y - y) / BoundingBox.Height) + BoundingBox.Y + BoundingBox.Height);
                        UpdatePoints(topRightScaler, (1, 1));
                        BoundingBox.Width = x - BoundingBox.X;
                        BoundingBox.Height += BoundingBox.Y - y;
                        (BoundingBox.X, BoundingBox.Y) = (x - BoundingBox.Width, y);
                        break;
                    case 2:
                        switch ((width: x - BoundingBox.X, height: y - BoundingBox.Y))
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
                            default:
                                break;
                        }
                        Func<(double x, double y), (double, double)> bottomRightScaler = ((double x, double y) point) => (((point.x - BoundingBox.X) * (x - BoundingBox.X) / BoundingBox.Width) + BoundingBox.X, ((point.y - BoundingBox.Y) * (y - BoundingBox.Y) / BoundingBox.Height) + BoundingBox.Y);
                        UpdatePoints(bottomRightScaler, (1, 1));
                        BoundingBox.Width = x - BoundingBox.X;
                        BoundingBox.Height = y - BoundingBox.Y;
                        (BoundingBox.X, BoundingBox.Y) = (x - BoundingBox.Width, y - BoundingBox.Height);
                        break;
                    case 3:
                        switch ((width: BoundingBox.Width + BoundingBox.X - x, height: y - BoundingBox.Y))
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
                            default:
                                break;
                        }
                        Func<(double x, double y), (double, double)> bottomLeftScaler = ((double x, double y) point) => (((point.x - BoundingBox.X - BoundingBox.Width) * (BoundingBox.Width + BoundingBox.X - x) / BoundingBox.Width) + BoundingBox.X + BoundingBox.Width, ((point.y - BoundingBox.Y) * (y - BoundingBox.Y) / BoundingBox.Height) + BoundingBox.Y);
                        UpdatePoints(bottomLeftScaler, (1, 1));
                        BoundingBox.Width += BoundingBox.X - x;
                        BoundingBox.Height = y - BoundingBox.Y;
                        (BoundingBox.X, BoundingBox.Y) = (x, y - BoundingBox.Height);
                        break;
                    default:
                        break;
                }
                UpdateData();
                break;
            case EditMode.None:
                break;
            default:
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

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.MoveAnchor:
                if (CurrentInstruction is null)
                {
                    break;
                }

                if (Instructions[CurrentInstruction.Value] is CubicBézierCurveInstruction cubicCurve)
                {
                    if (cubicCurve.ControlPoints[0].WithinRangeOf(cubicCurve.ControlPoints[1], 5 / SVG.Scale))
                    {
                        var newQuadraticCurve = new QuadraticBézierCurveInstruction(
                            cubicCurve.ControlPoints[0].x,
                            cubicCurve.ControlPoints[0].y,
                            cubicCurve.EndPosition.x,
                            cubicCurve.EndPosition.y,
                            false,
                            cubicCurve.PreviousInstruction!
                            )
                        {
                            ExplicitSymbol = cubicCurve.ExplicitSymbol,
                            NextInstruction = cubicCurve.NextInstruction
                        };
                        if (cubicCurve.NextInstruction is { } next)
                        {
                            next.PreviousInstruction = newQuadraticCurve;
                        }
                        cubicCurve.PreviousInstruction!.NextInstruction = newQuadraticCurve;
                        Instructions[CurrentInstruction.Value] = newQuadraticCurve;
                        UpdateData();
                    }
                }
                else if (Instructions[CurrentInstruction.Value] is ShorthandCubicBézierCurveInstruction shorthandCubicCurve)
                {
                    if (shorthandCubicCurve.ReflectedPreviousInstructionsLastControlPoint.WithinRangeOf(shorthandCubicCurve.ControlPoints[0], 5 / SVG.Scale))
                    {
                        var newShorthandQuadraticCurve = new ShorthandQuadraticBézierCurveInstruction(
                            shorthandCubicCurve.EndPosition.x,
                            shorthandCubicCurve.EndPosition.y,
                            false,
                            shorthandCubicCurve.PreviousInstruction!
                            )
                        {
                            ExplicitSymbol = shorthandCubicCurve.ExplicitSymbol,
                            NextInstruction = shorthandCubicCurve.NextInstruction
                        };
                        if (shorthandCubicCurve.NextInstruction is { } next)
                        {
                            next.PreviousInstruction = newShorthandQuadraticCurve;
                        }
                        shorthandCubicCurve.PreviousInstruction!.NextInstruction = newShorthandQuadraticCurve;
                        Instructions[CurrentInstruction.Value] = newShorthandQuadraticCurve;
                        newShorthandQuadraticCurve.UpdateReflectionForInstructions();
                        UpdateData();
                    }
                }
                SVG.CurrentAnchor = null;
                SVG.EditMode = EditMode.None;
                break;
            case EditMode.Move:
                SVG.EditMode = EditMode.None;
                break;
            case EditMode.Add:
                var currentInstruction = (CubicBézierCurveInstruction)Instructions.Last();
                CubicBézierCurveInstruction nextInstruction = new(x, y, x, y, x, y, false, Instructions.Last()) { ExplicitSymbol = true };
                currentInstruction.EndPosition = (x, y);
                currentInstruction.ControlPoints[0] = ((currentInstruction.StartPosition.x * 2.0 / 3.0) + (currentInstruction.EndPosition.x * 1.0 / 3.0), (currentInstruction.StartPosition.y * 2.0 / 3.0) + (currentInstruction.EndPosition.y * 1.0 / 3.0));
                currentInstruction.ControlPoints[^1] = ((currentInstruction.StartPosition.x * 1.0 / 3.0) + (currentInstruction.EndPosition.x * 2.0 / 3.0), (currentInstruction.StartPosition.y * 1.0 / 3.0) + (currentInstruction.EndPosition.y * 2.0 / 3.0));
                if (SVG.SnapToInteger)
                {
                    currentInstruction.ControlPoints[0] = ((int)currentInstruction.ControlPoints[0].x, (int)currentInstruction.ControlPoints[0].y);
                    currentInstruction.ControlPoints[^1] = ((int)currentInstruction.ControlPoints[^1].x, (int)currentInstruction.ControlPoints[^1].y);
                }
                currentInstruction.NextInstruction = nextInstruction;
                Instructions.Add(nextInstruction);
                UpdateData();
                break;
            case EditMode.Scale:
                SVG.CurrentAnchor = null;
                if (SVG.SnapToInteger)
                {
                    SnapToInteger();
                }
                break;
            case EditMode.None:
                break;
            default:
                break;
        }
    }

    public override void HandlePointerOut(PointerEventArgs eventArgs)
    {
    }

    public static void AddNew(SVGEditor SVG)
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

        SVG.ClearSelectedShapes();
        SVG.SelectShape(path);
        SVG.AddElement(path);
    }

    public override void Complete()
    {
        Instructions.RemoveAt(Instructions.Count - 1);
        Instructions.Add(new ClosePathInstruction(false, Instructions.Last()));
        UpdateData();
    }

    public override void SnapToInteger()
    {
        foreach (IPathInstruction inst in Instructions)
        {
            inst.SnapToInteger();
        }
        UpdateData();
    }

    public void ConvertToRelative()
    {
        foreach (IPathInstruction inst in Instructions)
        {
            inst.Relative = true;
        }
        UpdateData();
    }
}
