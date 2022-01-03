using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class ShapeEditor<TShape> : ComponentBase where TShape : Shape
    {
        [Parameter]
        public TShape SVGElement { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        public ElementReference ElementReference { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (SVGElement.SVG.EditMode == EditMode.Scale || (SVGElement is G && SVGElement.Selected))
            {
                var BBox = await JSRuntime.BBox(ElementReference);
                var pos = SVGElement.SVG.LocalDetransform((BBox.x - SVGElement.SVG.BBox.x, BBox.y - SVGElement.SVG.BBox.y));
                SVGElement.BoundingBox = new BoundingBox()
                {
                    x = pos.x,
                    y = pos.y,
                    height = BBox.height / SVGElement.SVG.Scale,
                    width = BBox.width / SVGElement.SVG.Scale
                };
            }
        }

        public void FocusElement()
        {
            SVGElement.SVG.SelectedElements.Clear();
            SVGElement.SVG.SelectedElements.Add(SVGElement);
            SVGElement.SVG.FocusedElement = SVGElement;
        }

        public void UnfocusElement()
        {
            SVGElement.SVG.SelectedElements.Remove(SVGElement);
        }

        public async Task KeyUp(KeyboardEventArgs eventArgs)
        {
            if (SVGElement.IsChildElement) return;
            if (eventArgs.CtrlKey)
            {
                if (eventArgs.Key == "c")
                {
                    await SVGElement.SVG.CopyElementAsync(SVGElement);
                }
                if (eventArgs.Key == "v")
                {
                    await SVGElement.SVG.PasteElementAsync();
                }
            }
            else
            {
                if (eventArgs.Key == "Delete")
                {
                    SVGElement.SVG.Remove(SVGElement);
                }
            }
        }

        public void AnchorSelect(int anchor)
        {
            SVGElement.SVG.CurrentAnchorElement = SVGElement;
            SVGElement.SVG.CurrentAnchor = anchor;
            SVGElement.SVG.EditMode = EditMode.MoveAnchor;
        }

        public async Task Select(MouseEventArgs eventArgs)
        {
            if (SVGElement.IsChildElement) return;
            if (SVGElement.SVG.EditMode is EditMode.Add) return;
            if (eventArgs.CtrlKey)
            {
                if (!SVGElement.Selected)
                {
                    SVGElement.SVG.SelectedElements.Add(SVGElement);
                }
                SVGElement.SVG.EditMode = EditMode.None;
            }
            else
            {
                SVGElement.SVG.MovePanner = SVGElement.SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                if (!SVGElement.Selected)
                {
                    SVGElement.SVG.EditMode = EditMode.Move;
                    SVGElement.SVG.SelectedElements.Clear();
                    SVGElement.SVG.SelectedElements.Add(SVGElement);
                    await JSRuntime.Focus(ElementReference);
                }
                StateHasChanged();
                switch (SVGElement.SVG.EditMode)
                {
                    case EditMode.None:
                        SVGElement.SVG.EditMode = EditMode.Move;
                        break;
                    case EditMode.Scale:
                        SVGElement.SVG.CurrentAnchor = -1;
                        break;
                }
            }
        }

        protected override bool ShouldRender()
        {
            var StateRepresentation = SVGElement.StateRepresentation;
            if (SVGElement._StateRepresentation != StateRepresentation)
            {
                SVGElement._StateRepresentation = StateRepresentation;
                return true;
            }
            return false;
        }
    }
}