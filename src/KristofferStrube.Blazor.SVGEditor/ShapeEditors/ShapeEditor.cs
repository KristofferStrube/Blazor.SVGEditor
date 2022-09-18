using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class ShapeEditor<TShape> : ComponentBase where TShape : Shape
    {
        [Parameter]
        public TShape SVGElement { get; set; }

        public ElementReference ElementReference { get; set; }

        protected override async Task OnAfterRenderAsync(bool _)
        {
            if (SVGElement.Selected)
            {
                Box BBox = await SVGElement.SVG.GetBoundingBox(ElementReference);
                (double x, double y) = SVGElement.SVG.LocalDetransform((BBox.X - SVGElement.SVG.BBox.X, BBox.Y - SVGElement.SVG.BBox.Y));
                SVGElement.BoundingBox = new Box()
                {
                    X = x,
                    Y = y,
                    Height = BBox.Height / SVGElement.SVG.Scale,
                    Width = BBox.Width / SVGElement.SVG.Scale
                };
            }
        }

        public void FocusElement()
        {
            SVGElement.SVG.FocusedShapes = SVGElement;
        }

        public void UnfocusElement()
        {
            SVGElement.SVG.FocusedShapes = null;
        }

        public async Task KeyUp(KeyboardEventArgs eventArgs)
        {
            if (SVGElement.IsChildElement)
            {
                return;
            }

            if (eventArgs.CtrlKey)
            {
                if (eventArgs.Key == "c")
                {
                    await SVGElement.SVG.CopyElementsAsync();
                }
                else if (eventArgs.Key == "v")
                {
                    await SVGElement.SVG.PasteElementsAsync(SVGElement);
                }
            }
            else
            {
                if (eventArgs.Key == "Delete")
                {
                    SVGElement.SVG.Remove();
                }
            }
        }

        public void AnchorSelect(int anchor)
        {
            SVGElement.SVG.CurrentAnchorShape = SVGElement;
            SVGElement.SVG.CurrentAnchor = anchor;
            SVGElement.SVG.EditMode = EditMode.MoveAnchor;
        }

        public async Task Select(MouseEventArgs eventArgs)
        {
            if (SVGElement.IsChildElement)
            {
                return;
            }

            if (SVGElement.SVG.EditMode is EditMode.Add)
            {
                return;
            }

            if (eventArgs.CtrlKey)
            {
                if (!SVGElement.Selected)
                {
                    SVGElement.SVG.SelectedShapes.Add(SVGElement);
                    await SVGElement.SVG.Focus(ElementReference);
                }
                SVGElement.SVG.EditMode = EditMode.None;
            }
            else
            {
                SVGElement.SVG.MovePanner = SVGElement.SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                if (!SVGElement.Selected)
                {
                    SVGElement.SVG.EditMode = EditMode.Move;
                    SVGElement.SVG.SelectedShapes.Clear();
                    SVGElement.SVG.SelectedShapes.Add(SVGElement);
                    await SVGElement.SVG.Focus(ElementReference);
                }
                StateHasChanged();
                if (eventArgs.Button == 0)
                {
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
        }

        protected override bool ShouldRender()
        {
            string StateRepresentation = SVGElement.StateRepresentation;
            if (SVGElement._stateRepresentation != StateRepresentation)
            {
                SVGElement._stateRepresentation = StateRepresentation;
                StateHasChanged();
                return true;
            }
            return false;
        }
    }
}