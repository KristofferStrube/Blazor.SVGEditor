using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BlazorContextMenu;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG : ComponentBase
    {
        [Parameter]
        public string Input { get; set; }

        private string _Input { get; set; }

        [Parameter]
        public Action<string> InputUpdated { get; set; }

        internal IDocument Document { get; set; }

        public Shape ColorPickerShape { get; set; }

        public string ColorPickerAttribute { get; set; }

        public string ColorPickerTitle => $"Pick {ColorPickerAttribute} Color";

        public bool IsColorPickerOpen => ColorPickerShape is not null;

        public string PreviousColor => ColorPickerShape is not null ? (ColorPickerAttribute == "Fill" ? ColorPickerShape.Fill : ColorPickerShape.Stroke) : String.Empty;

        public Dictionary<string, Type> SupportedTypes { get; set; } = new Dictionary<string, Type> {
                { "RECT", typeof(Rectangle) },
                { "CIRCLE", typeof(Circle) },
                { "POLYGON", typeof(Polygon) },
                { "POLYLINE", typeof(Polyline) },
                { "PATH", typeof(Path) },
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

            ElementsAsHtml = Elements.Select(e => e.Element.ToHtml()).ToList();
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
                            ElementsAsHtml[Elements.IndexOf(element)] = element.Element.ToHtml();
                        });
                    UpdateInput();
                });
        }

        private Subject<ISVGElement> ElementSubject = new();

        internal void UpdateInput(ISVGElement SVGElement)
        {
            ElementSubject.OnNext(SVGElement);
        }

        internal void AddElement(ISVGElement SVGElement)
{
            Elements.Add(SVGElement);
            ElementsAsHtml.Add(SVGElement.Element.ToHtml());
            UpdateInput();
        }

        public void UpdateInput()
        {
            _Input = string.Join(" \n", ElementsAsHtml);
            InputUpdated(_Input);
        }

        public void RerenderAll()
        {
            Elements.ForEach(e => e._StateRepresentation = null);
        }

        public double Scale { get; set; } = 1;

        public (double x, double y) Translate = (0, 0);

        public List<ISVGElement> Elements { get; set; }

        public List<string> ElementsAsHtml { get; set; }

        public ISVGElement CurrentShape { get; set; }

        public (double x, double y) LastRightClick { get; set; }

        public void Move(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseMove(eventArgs);
        }

        public void Up(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseUp(eventArgs);
            if (eventArgs.Button == 2)
            {
                LastRightClick = (eventArgs.OffsetX, eventArgs.OffsetY);
            }
        }

        public void UnSelect(MouseEventArgs eventArgs)
        {
            if (CurrentShape != null && CurrentShape.EditMode != EditMode.Add)
            {
                CurrentShape = null;
            }
        }

        public void Out(MouseEventArgs eventArgs)
        {
            CurrentShape?.HandleMouseOut(eventArgs);
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
            return ((pos.x - Translate.x) / Scale, (pos.y - Translate.y) / Scale);
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

        protected void AddNewPath(ItemClickEventArgs e)
        {
            Path.AddNew(this);
        }

        protected void AddNewPolygon(ItemClickEventArgs e)
        {
            Polygon.AddNew(this);
        }

        protected void AddNewPolyline(ItemClickEventArgs e)
        {
            Polyline.AddNew(this);
        }

        protected void CompleteShape(ItemClickEventArgs e)
        {
            CurrentShape.EditMode = EditMode.None;
            CurrentShape.Complete();
            CurrentShape = null;
        }
    }
}
