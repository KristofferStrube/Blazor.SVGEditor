using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using AngleSharp;
using AngleSharp.Html.Parser;
using BlazorContextMenu;
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
            if (SVGElement.EditMode == EditMode.Scale)
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

        public async Task KeyUp(KeyboardEventArgs eventArgs)
        {
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
            SVGElement.CurrentAnchor = anchor;
            SVGElement.EditMode = EditMode.MoveAnchor;
        }

        public async Task Select(MouseEventArgs eventArgs)
        {
            if (SVGElement.Element.ParentElement.TagName == "G") return;
            if (SVGElement.SVG.CurrentShape == null || SVGElement.SVG.CurrentShape.EditMode is EditMode.None or EditMode.Scale)
            {
                if (SVGElement.SVG.CurrentShape is not null && SVGElement.SVG.CurrentShape != SVGElement)
                {
                    SVGElement.SVG.CurrentShape.EditMode = EditMode.None;
                }

                await JSRuntime.Focus(ElementReference);
                SVGElement.SVG.CurrentShape = SVGElement;
                SVGElement.Panner = SVGElement.SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                if (SVGElement.SVG.CurrentShape.EditMode == EditMode.None)
                {
                    SVGElement.EditMode = EditMode.Move;
                }
                else
                {
                    SVGElement.CurrentAnchor = -1;
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