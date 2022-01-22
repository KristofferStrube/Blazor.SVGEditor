using System.Reactive.Linq;
using System.Reactive.Subjects;
using AngleSharp;
using AngleSharp.Dom;
using BlazorContextMenu;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG : ComponentBase
    {
        [Parameter]
        public string Input { get; set; }

        private string _Input { get; set; }

        [Parameter]
        public Action<string> InputUpdated { get; set; }

        [Parameter]
        public bool SnapToInteger { get; set; } = false;

        private ElementReference SVGElementReference { get; set; }

        internal IDocument Document { get; set; }

        public Shape ColorPickerShape { get; set; }

        protected Animate ColorPickerAnimate { get; set; }

        protected int ColorPickerAnimateFrame { get; set; }

        public string ColorPickerAttribute { get; set; }

        public string ColorPickerTitle => $"Pick {ColorPickerAttribute} Color";

        public bool IsColorPickerOpen => ColorPickerShape is not null || ColorPickerAnimate is not null;

        // TODO: Fix to include Animate Frame Color
        public string PreviousColor => ColorPickerShape is not null ? (ColorPickerAttribute == "Fill" ? ColorPickerShape.Fill : ColorPickerShape.Stroke) : String.Empty;

        public Dictionary<string, Type> SupportedTypes { get; set; } = new Dictionary<string, Type> {
                { "RECT", typeof(Rect) },
                { "CIRCLE", typeof(Circle) },
                { "ELLIPSE", typeof(Ellipse) },
                { "POLYGON", typeof(Polygon) },
                { "POLYLINE", typeof(Polyline) },
                { "LINE", typeof(Line) },
                { "PATH", typeof(Path) },
                { "G", typeof(G)}
            };

        protected override async Task OnParametersSetAsync()
        {
            if (Input == _Input)
            {
                return;
            }
            _Input = Input;

            var config = Configuration.Default;

            var context = BrowsingContext.New(config);

            Document = await context.OpenAsync(req => req.Content(Input));

            Elements = Document.GetElementsByTagName("BODY")[0].Children.Select(child =>
            {
                ISVGElement SVGElement;
                if (SupportedTypes.ContainsKey(child.TagName))
                {
                    SVGElement = (ISVGElement)Activator.CreateInstance(SupportedTypes[child.TagName], child, this);
                }
                else
                {
                    SVGElement = new NonImplmentedElement();
                }
                SVGElement.Changed = UpdateInput;
                return SVGElement;
            }
            ).ToList();

            Elements.ForEach(e => e.UpdateHtml());
        }

        protected override void OnInitialized()
        {
            ElementSubject
                .Buffer(TimeSpan.FromMilliseconds(33))
                .Where(updates => updates.Count > 0)
                .Subscribe(updates =>
                {
                    updates
                        .Distinct()
                        .ToList()
                        .ForEach(element => element.UpdateHtml());
                    UpdateInput();
                });
        }

        public BoundingBox BBox { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected IJSObjectReference JSModule { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js");
            BBox = await GetBBox(SVGElementReference);
        }

        private Subject<ISVGElement> ElementSubject = new();

        internal void UpdateInput(ISVGElement SVGElement)
        {
            ElementSubject.OnNext(SVGElement);
        }

        internal void AddElement(ISVGElement SVGElement)
        {
            Elements.Add(SVGElement);
            SVGElement.UpdateHtml();
            Document.GetElementsByTagName("BODY")[0].AppendElement(SVGElement.Element);
            UpdateInput();
        }

        public void UpdateInput()
        {
            _Input = string.Join(" \n", Elements.Select(e => e.StoredHtml));
            InputUpdated(_Input);
        }

        public void RerenderAll()
        {
            Elements.ForEach(e => e.Rerender());
        }

        public double Scale { get; set; } = 1;

        public (double x, double y) Translate = (0, 0);

        public List<ISVGElement> Elements { get; set; }

        public (double x, double y) LastRightClick { get; set; }

        public List<ISVGElement> SelectedElements { get; set; } = new();

        public ISVGElement FocusedElement { get; set; }

        public List<ISVGElement> MarkedElements
        {
            get
            {
                if (FocusedElement != null && !SelectedElements.Contains(FocusedElement))
                {
                    return SelectedElements.Append(FocusedElement).ToList();
                }
                return SelectedElements;
            }
        }

        public (double x, double y) MovePanner { get; set; }

        public int? CurrentAnchor { get; set; }
#nullable enable
        public ISVGElement? CurrentAnchorElement { get; set; }
#nullable disable

        private (double x, double y)? TranslatePanner { get; set; }

        public EditMode EditMode { get; set; } = EditMode.None;

        public void Move(MouseEventArgs eventArgs)
        {
            if (TranslatePanner.HasValue)
            {
                var newPanner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                Translate = (Translate.x + newPanner.x - TranslatePanner.Value.x, Translate.y + newPanner.y - TranslatePanner.Value.y);
                TranslatePanner = newPanner;
            }
            else
            {
                if (CurrentAnchorElement is ISVGElement element)
                {
                    var pos = LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                    element.HandleMouseMove(eventArgs);
                }
                else
                {
                    var pos = LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
                    MarkedElements.ForEach(e => e.HandleMouseMove(eventArgs));
                    MovePanner = (pos.x, pos.y);
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

        public void ContextZoomIn()
        {
            ZoomIn(LastRightClick.x, LastRightClick.y, 1.5);
        }

        public void ContextZoomOut()
        {
            ZoomOut(LastRightClick.x, LastRightClick.y, 1.5);
        }

        public (double x, double y) LocalTransform((double x, double y) pos)
        {
            return (pos.x * Scale + Translate.x, pos.y * Scale + Translate.y);
        }

        public (double x, double y) LocalDetransform((double x, double y) pos)
        {
            var res = (x: (pos.x - Translate.x) / Scale, y: (pos.y - Translate.y) / Scale);
            if (SnapToInteger)
            {
                return ((int)res.x, (int)res.y);
            }
            return res;
        }

        private void ZoomIn(double x, double y, double ZoomFactor = 1.1)
        {
            var prevScale = Scale;
            Scale *= ZoomFactor;
            if (Scale > 0.91 && Scale < 1.09)
            {
                Scale = 1;
            }
            Translate = (Translate.x + (x - Translate.x) * (1 - Scale / prevScale), Translate.y + (y - Translate.y) * (1 - Scale / prevScale));
        }

        private void ZoomOut(double x, double y, double ZoomFactor = 1.1)
        {
            var prevScale = Scale;
            Scale /= ZoomFactor;
            if (Scale > 0.91 && Scale < 1.09)
            {
                Scale = 1;
            }
            Translate = (Translate.x + (x - Translate.x) * (1 - Scale / prevScale), Translate.y + (y - Translate.y) * (1 - Scale / prevScale));
        }

        protected void OpenColorPicker(Shape shape, string attribute)
        {
            ColorPickerShape = shape;
            ColorPickerAttribute = attribute;
        }

        protected void OpenAnimateColorPicker(Animate fillAnimate, string attribute, int frame)
        {
            ColorPickerAnimate = fillAnimate;
            ColorPickerAnimateFrame = frame;
            ColorPickerAttribute = attribute;
        }

        protected void ColorPickerClosed(string value)
        {
            if (ColorPickerAttribute == "Fill")
            {
                ColorPickerShape.Fill = value;
            }
            else if (ColorPickerAttribute == "Stroke")
            {
                ColorPickerShape.Stroke = value;
            }
            else if (ColorPickerAttribute is "FillAnimate" or "StrokeAnimate")
            {
                ColorPickerAnimate.Values[ColorPickerAnimateFrame] = value;
                ColorPickerAnimate.UpdateValues();
            }
            ColorPickerShape = null;
            ColorPickerAnimate = null;
        }

        protected void MoveToBack(Shape shape)
        {
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(0, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }

        protected void MoveBack(Shape shape)
        {
            var index = Elements.IndexOf(shape);
            if (index != 0)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index - 1, shape);
                Elements.ForEach(e => e.UpdateHtml());
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveForward(Shape shape)
        {
            var index = Elements.IndexOf(shape);
            if (index != Elements.Count - 1)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index + 1, shape);
                Elements.ForEach(e => e.UpdateHtml());
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveToFront(Shape shape)
        {
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(Elements.Count, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }

        protected void CompleteShape(ISVGElement sVGElement)
        {
            if (SelectedElements.Count == 1)
            {
                EditMode = EditMode.None;
                sVGElement.Complete();
                SelectedElements.Clear();
            }
        }

        protected void CompleteShapeWithoutClose(Path path)
        {
            CompleteShape(path);
            path.Instructions.Remove(path.Instructions.Last());
            path.UpdateData();
        }

        protected void DeletePreviousInstruction(Path path)
        {
            path.Instructions = path.Instructions.Take(0..^2).ToList();
            path.UpdateData();
        }

        protected void ScaleShape(Shape shape)
        {
            if (!SelectedElements.Contains(shape))
            {
                SelectedElements.Clear();
                SelectedElements.Add(shape);
            }
            EditMode = EditMode.Scale;
        }

        public void Remove()
        {
            MarkedElements.ForEach(e => Elements.Remove(e));
            SelectedElements.Clear();
            UpdateInput();
            RerenderAll();
        }

        public async Task CopyElementsAsync()
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", string.Join("\n", MarkedElements.Select(e => e.StoredHtml)));
        }

        public async Task PasteElementsAsync(ISVGElement SVGElement = null)
        {
            string clipboard = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
            var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            if (SVGElement != null)
            {
                int index = Elements.IndexOf(SVGElement);
                elementsAsHtml.Insert(index + 1, clipboard);
            }
            else
            {
                elementsAsHtml.Add(clipboard);
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        public void Group(Shape shape)
        {
            var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            if (MarkedElements.Count == 1)
            {
                var pos = Elements.IndexOf(shape);
                elementsAsHtml[pos] = "<g>" + shape.StoredHtml + "</g>";
            }
            else
            {
                var frontElement = MarkedElements.MaxBy(e => Elements.IndexOf(e));
                elementsAsHtml[Elements.IndexOf(frontElement)] = "<g>\n" + string.Join("\n", MarkedElements.OrderBy(e => Elements.IndexOf(e)).Select(e => e.StoredHtml)) + "\n</g>";
                foreach (var element in MarkedElements.Where(e => e != frontElement))
                {
                    var pos = Elements.IndexOf(element);
                    Elements.RemoveAt(pos);
                    elementsAsHtml.RemoveAt(pos);
                }
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        public void Ungroup(G g)
        {
            var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            var pos = Elements.IndexOf(g);
            elementsAsHtml[pos] = string.Join("\n", g.ChildElements.Select(e => e.StoredHtml));
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        protected void ToggleAnimation(Shape shape)
        {
            shape.Playing = !shape.Playing;
            shape.Rerender();
        }

        public async Task Focus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("focus", elementReference);
        }

        public async Task UnFocus(ElementReference elementReference)
        {
            await JSModule.InvokeVoidAsync("unFocus", elementReference);
        }

        public async Task<BoundingBox> GetBBox(ElementReference elementReference)
        {
            return await JSModule.InvokeAsync<BoundingBox>("BBox", elementReference);
        }
    }
}
