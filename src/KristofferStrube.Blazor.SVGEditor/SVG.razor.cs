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

        private ElementReference ElementReference { get; set; }

        internal IDocument Document { get; set; }

        public Shape ColorPickerShape { get; set; }

        public string ColorPickerAttribute { get; set; }

        public string ColorPickerTitle => $"Pick {ColorPickerAttribute} Color";

        public bool IsColorPickerOpen => ColorPickerShape is not null;

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

            ElementsAsHtml = Elements.Select(e => e.ToHtml()).ToList();
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
                        .ForEach(element =>
                        {
                            ElementsAsHtml[Elements.IndexOf(element)] = element.ToHtml();
                        });
                    UpdateInput();
                });
        }

        public BoundingBox BBox { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            BBox = await JSRuntime.BBox(ElementReference);
        }

        private Subject<ISVGElement> ElementSubject = new();

        internal void UpdateInput(ISVGElement SVGElement)
        {
            ElementSubject.OnNext(SVGElement);
        }

        internal void AddElement(ISVGElement SVGElement)
        {
            Elements.Add(SVGElement);
            ElementsAsHtml.Add(SVGElement.ToHtml());
            Document.GetElementsByTagName("BODY")[0].AppendElement(SVGElement.Element);
            UpdateInput();
        }

        public void UpdateInput()
        {
            _Input = string.Join(" \n", ElementsAsHtml);
            InputUpdated(_Input);
        }

        public void RerenderAll()
        {
            Elements.ForEach(e => e.Rerender());
        }

        public double Scale { get; set; } = 1;

        public (double x, double y) Translate = (0, 0);

        public List<ISVGElement> Elements { get; set; }

        public List<string> ElementsAsHtml { get; set; }

        public (double x, double y) LastRightClick { get; set; }

        public List<ISVGElement> SelectedElements { get; set; } = new();

        public (double x, double y) MovePanner { get; set; }

        public int? CurrentAnchor { get; set; }
        public ISVGElement? CurrentAnchorElement { get; set; }

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
                    SelectedElements.ForEach(e => e.HandleMouseMove(eventArgs));
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

        protected void OpenFillColorPicker(ItemClickEventArgs e)
        {
            ColorPickerShape = (Shape)e.Data;
            ColorPickerAttribute = "Fill";
        }

        protected void OpenStrokeColorPicker(ItemClickEventArgs e)
        {
            ColorPickerShape = (Shape)e.Data;
            ColorPickerAttribute = "Stroke";
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
            ColorPickerShape = null;
        }

        protected void MoveToBack(ItemClickEventArgs e)
        {
            var shape = (Shape)e.Data;
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(0, shape);
            ElementsAsHtml = Elements.Select(e => e.ToHtml()).ToList();
            UpdateInput();
            RerenderAll();
        }

        protected void MoveBack(ItemClickEventArgs e)
        {
            var shape = (Shape)e.Data;
            var index = Elements.IndexOf(shape);
            if (index != 0)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index - 1, shape);
                ElementsAsHtml = Elements.Select(e => e.ToHtml()).ToList();
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveForward(ItemClickEventArgs e)
        {
            var shape = (Shape)e.Data;
            var index = Elements.IndexOf(shape);
            if (index != Elements.Count - 1)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index + 1, shape);
                ElementsAsHtml = Elements.Select(e => e.ToHtml()).ToList();
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveToFront(ItemClickEventArgs e)
        {
            var shape = (Shape)e.Data;
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(Elements.Count, shape);
            ElementsAsHtml = Elements.Select(e => e.ToHtml()).ToList();
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

        public void Remove(ISVGElement SVGElement)
        {
            ElementsAsHtml.RemoveAt(Elements.IndexOf(SVGElement));
            Elements.Remove(SVGElement);
            SelectedElements.Clear();
            UpdateInput();
            RerenderAll();
        }

        public async Task CopyElementAsync(ISVGElement SVGElement)
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", SVGElement.ToHtml());
        }

        public async Task PasteElementAsync(ISVGElement SVGElement = null)
        {
            string clipboard = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
            if (SVGElement != null)
            {
                int index = Elements.IndexOf(SVGElement);
                ElementsAsHtml.Insert(index + 1, clipboard);
            }
            else
            {
                ElementsAsHtml.Add(clipboard);
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", ElementsAsHtml));
        }

        public void Group(Shape shape)
        {
            if (SelectedElements.Count == 0)
            {
                var pos = Elements.IndexOf(shape);
                ElementsAsHtml[pos] = "<g>" + shape.ToHtml() + "</g>";
            }
            else
            {
                var frontElement = SelectedElements.MaxBy(e => Elements.IndexOf(e));
                ElementsAsHtml[Elements.IndexOf(frontElement)] = "<g>\n" + string.Join("\n", SelectedElements.OrderBy(e => Elements.IndexOf(e)).Select(e => e.ToHtml()))+ "\n</g>";
                foreach (var element in SelectedElements.Where(e => e != frontElement))
                {
                    var pos = Elements.IndexOf(element);
                    Elements.RemoveAt(pos);
                    ElementsAsHtml.RemoveAt(pos);
                }
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", ElementsAsHtml));
        }

        public async Task Ungroup(G g)
        {
            var pos = Elements.IndexOf(g);
            ElementsAsHtml[pos] = string.Join("\n", g.ChildElements.Select(e => e.ToHtml()));
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", ElementsAsHtml));
        }
    }
}
