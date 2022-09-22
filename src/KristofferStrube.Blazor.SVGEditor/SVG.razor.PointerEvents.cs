using Microsoft.AspNetCore.Components.Web;
using System.Data.SqlTypes;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVG
{
    private void Move(PointerEventArgs eventArgs)
    {
        if (TranslatePanner.HasValue)
        {
            (double x, double y) newPanner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
            Translate = (Translate.x + newPanner.x - TranslatePanner.Value.x, Translate.y + newPanner.y - TranslatePanner.Value.y);
            TranslatePanner = newPanner;
        }
        else
        {
            (double x, double y) = LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            if (EditGradient is LinearGradient linearGradient && linearGradient.CurrentStop is Stop stop)
            {
                var boundingBox = (x: EditGradientShape.BoundingBox.X, y: EditGradientShape.BoundingBox.Y);
                var external = sub((x,y), boundingBox);
                var p1 = (x: linearGradient.X1 * EditGradientShape.BoundingBox.Width, y: linearGradient.Y1 * EditGradientShape.BoundingBox.Height);
                var p2 = (x: linearGradient.X2 * EditGradientShape.BoundingBox.Width, y: linearGradient.Y2 * EditGradientShape.BoundingBox.Height);
                var v = sub(p2, p1);
                var externalPrime = sub(external, p1);
                var scalar = projectScalar(externalPrime, v);
                stop.Offset = ((int)(scalar * 100)) / 100.0;
            }
            else if (CurrentAnchorShape is Shape shape)
            {
                shape.HandlePointerMove(eventArgs);
            }
            else
            {
                if (MarkedShapes.Count == 0 && SelectionBox is not null)
                {
                    SelectionBox.Width = x - SelectionBox.X;
                    SelectionBox.Height = y - SelectionBox.Y;
                    BoxSelectionShapes = SelectionMode switch
                    {
                        SelectionMode.WindowSelection => WindowSelection(SelectionBox),
                        _ => CrossingSelection(SelectionBox)
                    };
                }
                else
                {
                    MarkedShapes.ForEach(e => e.HandlePointerMove(eventArgs));
                    MovePanner = (x, y);
                }
            }
        }
    }

    private (double x, double y) sub((double x, double y) u, (double x, double y) v)
    {
        return (u.x - v.x, u.y - v.x);
    }

    private double dot((double x, double y) u, (double x, double y) v)
    {
        return u.x * v.x + u.y * v.y;
    }

    private double projectScalar((double x, double y) u, (double x, double y) v)
    {
        return dot(u, v) / dot(v, v);
    }

    public void Down(PointerEventArgs eventArgs)
    {
        if (eventArgs.Button == 1)
        {
            TranslatePanner = (eventArgs.OffsetX, eventArgs.OffsetY);
        }
        else
        {
            (double x, double y) = LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            SelectionBox = new Box() { X = x, Y = y };
        }
    }

    public void Up(PointerEventArgs eventArgs)
    {
        CurrentAnchorShape = null;
        if (BoxSelectionShapes is { Count: > 0 })
        {
            SelectedShapes = BoxSelectionShapes;
            FocusedShapes = null;
        }
        BoxSelectionShapes = null;
        SelectionBox = null;
        if (EditGradient is LinearGradient linearGradient)
        {
            linearGradient.CurrentStop = null;
        }
        SelectedShapes.ForEach(e => e.HandlePointerUp(eventArgs));
        if (eventArgs.Button == 2)
        {
            LastRightClick = (eventArgs.OffsetX, eventArgs.OffsetY);
        }
        else if (eventArgs.Button == 1)
        {
            TranslatePanner = null;
            SelectedShapes.Clear();
        }
    }

    public void UnSelect(PointerEventArgs eventArgs)
    {
        if (EditMode != EditMode.Add && !eventArgs.CtrlKey)
        {
            EditMode = EditMode.None;
            SelectedShapes.Clear();
            FocusedShapes = null;
            EditGradient = null;
            EditGradientShape = null;
        }
    }

    public void Out(PointerEventArgs eventArgs)
    {
        SelectedShapes.ForEach(e => e.HandlePointerOut(eventArgs));
    }

    public void Wheel(WheelEventArgs eventArgs)
    {
        if (eventArgs.DeltaY < 0)
        {
            ZoomIn(eventArgs.OffsetX, eventArgs.OffsetY);
        }
        else if (eventArgs.DeltaY > 0)
        {
            ZoomOut(eventArgs.OffsetX, eventArgs.OffsetY);
        }
    }

    private List<Shape> WindowSelection(Box box)
    {
        return Elements.Where(e => e is Shape).Select(e => (Shape)e).Where(s => s.SelectionPoints.All(p => PointWitinBox(p, box))).ToList();
    }

    private List<Shape> CrossingSelection(Box box)
    {
        return Elements.Where(e => e is Shape).Select(e => (Shape)e).Where(s => s.SelectionPoints.Any(p => PointWitinBox(p, box))).ToList();
    }

    private static bool PointWitinBox((double x, double y) point, Box box)
    {
        return (box.Width, box.Height) switch
        {
            ( >= 0, >= 0) => point.x >= box.X && point.y >= box.Y & point.x <= box.X + box.Width && point.y <= box.Y + box.Height,
            ( >= 0, < 0) => point.x >= box.X && point.y <= box.Y & point.x <= box.X + box.Width && point.y >= box.Y + box.Height,
            ( < 0, >= 0) => point.x <= box.X && point.y >= box.Y & point.x >= box.X + box.Width && point.y <= box.Y + box.Height,
            _ => point.x <= box.X && point.y <= box.Y & point.x >= box.X + box.Width && point.y >= box.Y + box.Height,
        };
    }
}
