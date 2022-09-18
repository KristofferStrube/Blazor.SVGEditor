using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVG : ComponentBase
{
    private string _input;
    private ElementReference SVGElementReference;
    private List<Shape> ColorPickerShapes;
    private BaseAnimate ColorPickerAnimate;
    private int ColorPickerAnimateFrame;
    private AttributeNames ColorPickerAttribute;
    private List<ISVGElement> Elements;
    private (double x, double y)? TranslatePanner;
    private readonly Subject<ISVGElement> ElementSubject = new();
#nullable enable
    private List<Shape>? BoxSelectionShapes;
    private Box? SelectionBox;
#nullable disable
    private string ColorPickerTitle => $"Pick {ColorPickerAttribute} Color";
    private bool IsColorPickerOpen => ColorPickerShapes is not null || ColorPickerAnimate is not null;


    [Parameter]
    public string Input { get; set; }

    [Parameter]
    public Action<string> InputUpdated { get; set; }

    [Parameter]
    public bool SnapToInteger { get; set; } = false;

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.WindowSelection;

    internal IDocument Document { get; set; }
    public double Scale { get; set; } = 1;

    public (double x, double y) Translate = (0, 0);

    public (double x, double y) LastRightClick { get; set; }

    public List<Shape> SelectedShapes { get; set; } = new();

    public Dictionary<string, ISVGElement> Definitions { get; set; } = new();

    public ISVGElement EditGradient { get; set; }
    public Shape EditGradientShape { get; set; }

    public Shape FocusedShapes { get; set; }

    public (double x, double y) MovePanner { get; set; }

    public int? CurrentAnchor { get; set; }
#nullable enable
    public Shape? CurrentAnchorShape { get; set; }
#nullable disable

    public EditMode EditMode { get; set; } = EditMode.None;

    public List<Shape> MarkedShapes =>
        FocusedShapes != null && !SelectedShapes.Contains(FocusedShapes) ?
        SelectedShapes.Append(FocusedShapes).ToList() :
        SelectedShapes;

    public List<Shape> VisibleSelectionShapes =>
        BoxSelectionShapes is not null ?
        BoxSelectionShapes.ToList() :
        MarkedShapes;

    public string PreviousColor =>
        ColorPickerShapes is null or { Count: 0 } ?
        string.Empty :
        ColorPickerAttribute switch
        {
            AttributeNames.Fill or AttributeNames.FillAnimate => ColorPickerShapes.GroupBy(shape => shape.Fill).MaxBy(group => group.Count()).Key,
            AttributeNames.Stroke or AttributeNames.StrokeAnimate => ColorPickerShapes.GroupBy(shape => shape.Stroke).MaxBy(group => group.Count()).Key,
            _ => string.Empty
        };

    public Dictionary<string, Type> SupportedTypes { get; set; } = new Dictionary<string, Type> {
            { "RECT", typeof(Rect) },
            { "CIRCLE", typeof(Circle) },
            { "ELLIPSE", typeof(Ellipse) },
            { "POLYGON", typeof(Polygon) },
            { "POLYLINE", typeof(Polyline) },
            { "LINE", typeof(Line) },
            { "PATH", typeof(Path) },
            { "G", typeof(G) },
            { "DEFS", typeof(Defs) },
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

        Definitions = new();

        Elements = Document.GetElementsByTagName("BODY")[0].Children.Select(child =>
        {
            ISVGElement sVGElement;
            if (SupportedTypes.ContainsKey(child.TagName))
            {
                sVGElement = (ISVGElement)Activator.CreateInstance(SupportedTypes[child.TagName], child, this);
            }
            else
            {
                throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            }
            sVGElement.Changed = UpdateInput;
            return sVGElement;
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

    public void UpdateInput(ISVGElement SVGElement)
    {
        ElementSubject.OnNext(SVGElement);
    }

    public void AddElement(ISVGElement SVGElement, ISVGElement parent = null)
    {
        if (parent is null)
        {
            Elements.Add(SVGElement);
            SVGElement.UpdateHtml();
            Document.GetElementsByTagName("BODY")[0].AppendElement(SVGElement.Element);
        }
        else
        {
            parent.Element.AppendChild(SVGElement.Element);
            parent.Changed.Invoke(parent);
        }
        UpdateInput();
    }

    public void RemoveElement(ISVGElement SVGElement)
    {
        Elements.Remove(SVGElement);
    }

    private void UpdateInput()
    {
        _input = string.Join(" \n", Elements.Select(e => e.StoredHtml));
        InputUpdated(_input);
    }

    private void RerenderAll()
    {
        Elements.ForEach(e => e.Rerender());
    }

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
