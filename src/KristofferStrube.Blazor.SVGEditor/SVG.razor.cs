using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG : ComponentBase
    {
        [Parameter]
        public string Input { get; set; }

        private string _input;

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
        public string PreviousColor => ColorPickerShape is not null ? (ColorPickerAttribute == "Fill" ? ColorPickerShape.Fill : ColorPickerShape.Stroke) : string.Empty;

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
            if (Input == _input)
            {
                return;
            }
            _input = Input;

            IConfiguration config = Configuration.Default;

            IBrowsingContext context = BrowsingContext.New(config);

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
                    throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
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

        private readonly Subject<ISVGElement> ElementSubject = new();

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
            _input = string.Join(" \n", Elements.Select(e => e.StoredHtml));
            InputUpdated(_input);
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

        public (double x, double y) LocalTransform((double x, double y) pos)
        {
            return (pos.x * Scale + Translate.x, pos.y * Scale + Translate.y);
        }

        public (double x, double y) LocalDetransform((double x, double y) pos)
        {
            (double x, double y) res = (x: (pos.x - Translate.x) / Scale, y: (pos.y - Translate.y) / Scale);
            if (SnapToInteger)
            {
                return ((int)res.x, (int)res.y);
            }
            return res;
        }

        private void ZoomIn(double x, double y, double ZoomFactor = 1.1)
        {
            double prevScale = Scale;
            Scale *= ZoomFactor;
            if (Scale > 0.91 && Scale < 1.09)
            {
                Scale = 1;
            }
            Translate = (Translate.x + (x - Translate.x) * (1 - Scale / prevScale), Translate.y + (y - Translate.y) * (1 - Scale / prevScale));
        }

        private void ZoomOut(double x, double y, double ZoomFactor = 1.1)
        {
            double prevScale = Scale;
            Scale /= ZoomFactor;
            if (Scale > 0.91 && Scale < 1.09)
            {
                Scale = 1;
            }
            Translate = (Translate.x + (x - Translate.x) * (1 - Scale / prevScale), Translate.y + (y - Translate.y) * (1 - Scale / prevScale));
        }
    }
}
