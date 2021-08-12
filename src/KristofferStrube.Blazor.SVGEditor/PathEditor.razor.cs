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
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class PathEditor : ComponentBase
    {
        [Parameter]
        public Path SVGElement { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public ElementReference ElementReference { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            SVGElement.BBox = await JSRuntime.BBox(ElementReference);
        }

        public void KeyUp(KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key == "Delete")
            {
                SVGElement.SVG.Remove(SVGElement);
            }
        }

        public void AnchorSelect(int segment, int anchor)
        {
            if (SVGElement.EditMode is EditMode.Move or EditMode.None)
            {
                SVGElement.CurrentInstruction = segment;
                SVGElement.CurrentAnchor = anchor;
                SVGElement.EditMode = EditMode.MoveAnchor;
            }
            if (SVGElement.EditMode is EditMode.Scale)
            {
                SVGElement.CurrentAnchor = anchor;
            }
        }

        public async Task Select(MouseEventArgs eventArgs)
        {
            if (SVGElement.SVG.CurrentShape == null || SVGElement.SVG.CurrentShape.EditMode is EditMode.None or EditMode.Scale)
            {
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
            var StateRepresentation = SVGElement.StateRepresentation + Serialize(SVGElement.BBox);
            if (SVGElement._StateRepresentation != StateRepresentation)
            {
                SVGElement._StateRepresentation = StateRepresentation;
                return true;
            }
            return false;
        }
    }
}