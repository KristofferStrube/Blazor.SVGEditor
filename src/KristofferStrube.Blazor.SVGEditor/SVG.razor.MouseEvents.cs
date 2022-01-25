using Microsoft.AspNetCore.Components.Web;

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
                    MarkedElements.ForEach(e => e.HandleMouseMove(eventArgs));
                    MovePanner = (x, y);
                }
            }
        }

        public void Down(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == 1)
            {
                TranslatePanner = (eventArgs.OffsetX, eventArgs.OffsetY);
            }
        }

        public void Up(MouseEventArgs eventArgs)
        {
            CurrentAnchorElement = null;
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
    }
}
