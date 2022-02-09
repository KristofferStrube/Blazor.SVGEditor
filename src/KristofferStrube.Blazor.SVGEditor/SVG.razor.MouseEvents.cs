using Microsoft.AspNetCore.Components.Web;
using System.Collections;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG
    {
        public void Move(MouseEventArgs eventArgs)
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
                if (CurrentAnchorElement is ISVGElement element)
                {
                    element.HandleMouseMove(eventArgs);
                }
                else
                {
                    if (MarkedElements.Count == 0 && SelectionBox is not null)
                    {
                        SelectionBox.Width = x - SelectionBox.X;
                        SelectionBox.Height = y - SelectionBox.Y;
                        BoxSelectionElements = WindowSelection(SelectionBox);
                    }
                    else
                    {
                        MarkedElements.ForEach(e => e.HandleMouseMove(eventArgs));
                        MovePanner = (x, y);
                    }
                }
            }
        }

        public void Down(MouseEventArgs eventArgs)
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

        public void Up(MouseEventArgs eventArgs)
        {
            CurrentAnchorElement = null;
            if (BoxSelectionElements is { Count: > 0})
            {
                SelectedElements = BoxSelectionElements;
                FocusedElement = null;
            }
            BoxSelectionElements = null;
            SelectionBox = null;
            SelectedElements.ForEach(e => e.HandleMouseUp(eventArgs));
            if (eventArgs.Button == 2)
            {
                LastRightClick = (eventArgs.OffsetX, eventArgs.OffsetY);
            }
            else if (eventArgs.Button == 1)
            {
                TranslatePanner = null;
                SelectedElements.Clear();
            }
        }

        public void UnSelect(MouseEventArgs eventArgs)
        {
            if (EditMode != EditMode.Add && !eventArgs.CtrlKey)
            {
                EditMode = EditMode.None;
                SelectedElements.Clear();
                FocusedElement = null;
            }
        }

        public void Out(MouseEventArgs eventArgs)
        {
            SelectedElements.ForEach(e => e.HandleMouseOut(eventArgs));
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

        private List<ISVGElement> WindowSelection(Box box)
        {
            return Elements.Where(e => e.SelectionPoints.All(p => PointWitinBox(p, box))).ToList();
        }

        private bool PointWitinBox((double x, double y) point, Box box)
        {
            return point.x >= box.X && point.y >= box.Y & point.x <= box.X + box.Width && point.y <= box.Y + box.Height;
        }
    }
}
