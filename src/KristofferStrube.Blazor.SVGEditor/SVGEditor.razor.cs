using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVGEditor : ComponentBase
{
    private string? input;
    private ElementReference svgElementReference;
    private List<Shape>? colorPickerShapes;
    private string? colorPickerAttributeName;
    private Action<string>? colorPickerSetter;
    private (double x, double y)? translatePanner;
    private ((double x, double y) firstFinger, (double x, double y) secondFinger)? multiplePointerPanners;
    private readonly Subject<ISVGElement> elementSubject = new();
    private List<Shape>? boxSelectionShapes;
    private string ColorPickerTitle => $"Pick {colorPickerAttributeName} Color";
    private bool IsColorPickerOpen => colorPickerShapes is not null;

    public bool ShouldShowContextMenu(object? data)
    {
        return !DisableContextMenu
        && (data is null || data is Shape { IsChildElement: false })
        && ((SelectedShapes.Count == 1 && EditMode == EditMode.Add)
            || AddNewSVGElementMenuItems.Any(item => item.ShouldBePresented(this, data))
            || data is Stop
            || ActionMenuItems.Any(item => item.ShouldBePresented(this, data))
        );
    }

    public required IDocument Document { get; set; }

    public double Scale { get; set; } = 1;

    public (double x, double y) Translate = (0, 0);

    public (double x, double y) LastRightClick { get; set; }

    public Box? SelectionBox { get; set; }

    public List<ISVGElement> Elements { get; protected set; } = default!;

    public List<Shape> SelectedShapes { get; set; } = [];

    public Dictionary<string, ISVGElement> Definitions { get; set; } = [];

    public ISVGElement? EditGradient { get; set; }

    public Shape? FocusedShape { get; set; }

    public List<Shape> MoveOverShapes { get; set; } = [];

    public (double x, double y) MovePanner { get; set; }

    public int? CurrentAnchor { get; set; }

    public Shape? CurrentEditShape { get; set; }

    private EditMode editMode = EditMode.None;

    public EditMode EditMode
    {
        get => editMode;
        set
        {
            if (value is EditMode.Move && DisableMoveEditMode)
            {
                return;
            }

            if (value is EditMode.MoveAnchor && DisableMoveAnchorEditMode)
            {
                return;
            }

            editMode = value;
        }
    }

    public List<Shape> MarkedShapes =>
        FocusedShape != null && !SelectedShapes.Contains(FocusedShape)
        ? [.. SelectedShapes, FocusedShape]
        : SelectedShapes;

    public List<Shape> VisibleSelectionShapes =>
        boxSelectionShapes is not null
        ? boxSelectionShapes
        : MarkedShapes;

    public string? PreviousColor { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Input == input)
        {
            return;
        }
        input = Input;

        Definitions.Clear();
        ClearSelectedShapes();

        Document = await BrowsingContext.New().OpenAsync(res => res.Content($"<svg>{input}</svg>"));

        Elements = [];
        foreach (IElement child in Document.GetElementsByTagName("BODY")[0].Children.First().Children)
        {
            ISVGElement? sVGElement = SupportedElements.FirstOrDefault(se => se.CanHandle(child))?.ElementType is Type type
                ? Activator.CreateInstance(type, child, this) as ISVGElement
                : throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            if (sVGElement is not null)
            {
                sVGElement.Changed = UpdateInput;
            }
            Elements.Add(sVGElement!);
        }

        Elements.ForEach(e => e.UpdateHtml());
    }

    protected override void OnInitialized()
    {
        _ = elementSubject
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

        moduleTask = new(async () => await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/KristofferStrube.Blazor.SVGEditor/KristofferStrube.Blazor.SVGEditor.js"));
    }

    public void UpdateInput(ISVGElement SVGElement)
    {
        elementSubject.OnNext(SVGElement);
    }

    public void SelectShape(Shape shape)
    {
        if (DisableSelecting)
        {
            return;
        }

        SelectedShapes.Add(shape);
    }

    public void ClearSelectedShapes()
    {
        if (DisableDeselecting)
        {
            return;
        }

        SelectedShapes.Clear();
    }

    public void FocusShape(Shape shape)
    {
        if (DisableSelecting)
        {
            return;
        }

        FocusedShape = shape;
    }

    public void UnfocusShape()
    {
        if (DisableDeselecting)
        {
            return;
        }

        FocusedShape = null;
    }

    public void AddElement(ISVGElement SVGElement, ISVGElement? parent = null)
    {
        if (parent is null)
        {
            Elements.Add(SVGElement);
            SVGElement.UpdateHtml();
            _ = Document.GetElementsByTagName("BODY")[0].AppendElement(SVGElement.Element);
        }
        else
        {
            _ = parent.Element.AppendChild(SVGElement.Element);
            parent.Changed?.Invoke(parent);
        }
        UpdateInput();
    }

    public void AddDefinition(ISVGElement SVGElement)
    {
        ISVGElement? firstDefs = Elements.Where(e => e is Defs).FirstOrDefault();
        if (firstDefs is Defs defs)
        {
            defs.Children.Add(SVGElement);
            SVGElement.Changed = defs.UpdateInput;
            AddElement(SVGElement, defs);
        }
        else
        {
            IElement element = Document.CreateElement("DEFS");
            var newDefs = new Defs(element, this)
            {
                Changed = UpdateInput
            };
            newDefs.Children.Add(SVGElement);
            SVGElement.Changed = newDefs.UpdateInput;
            AddElement(newDefs);
            AddElement(SVGElement, newDefs);
        }
    }

    public void RemoveElement(ISVGElement SVGElement, ISVGElement? parent = null)
    {
        if (parent is null)
        {
            SVGElement.BeforeBeingRemoved();
            _ = Elements.Remove(SVGElement);
        }
        else
        {
            _ = parent.Element.RemoveChild(SVGElement.Element);
            parent.Changed?.Invoke(parent);
        }
    }

    private void UpdateInput()
    {
        input = string.Join(" \n", Elements.Select(e => e.StoredHtml));
        InputUpdated?.Invoke(input);
    }

    private void RerenderAll()
    {
        Elements.ForEach(e => e.Rerender());
    }

    public (double x, double y) LocalTransform((double x, double y) pos)
    {
        return ((pos.x * Scale) + Translate.x, (pos.y * Scale) + Translate.y);
    }

    public (double x, double y) LocalDetransform((double x, double y) pos)
    {
        (double x, double y) res = (x: (pos.x - Translate.x) / Scale, y: (pos.y - Translate.y) / Scale);
        return SnapToInteger ? ((double x, double y))((int)res.x, (int)res.y) : res;
    }

    private void ZoomIn(double x, double y, double ZoomFactor = 1.1, bool snapToNeutralScale = true)
    {
        if (DisableZoom)
        {
            return;
        }

        double prevScale = Scale;
        Scale *= ZoomFactor;
        if (snapToNeutralScale && (Scale > 0.91) && (Scale < 1.09))
        {
            Scale = 1;
        }
        if (DisablePanning)
        {
            return;
        }
        Translate = (Translate.x + ((x - Translate.x) * (1 - (Scale / prevScale))), Translate.y + ((y - Translate.y) * (1 - (Scale / prevScale))));
    }

    private void ZoomOut(double x, double y, double ZoomFactor = 1.1, bool snapToNeutralScale = true)
    {
        if (DisableZoom)
        {
            return;
        }

        double prevScale = Scale;
        Scale /= ZoomFactor;
        if (snapToNeutralScale && (Scale > 0.91) && (Scale < 1.09))
        {
            Scale = 1;
        }
        if (DisablePanning)
        {
            return;
        }
        Translate = (Translate.x + ((x - Translate.x) * (1 - (Scale / prevScale))), Translate.y + ((y - Translate.y) * (1 - (Scale / prevScale))));
    }

    /// <summary>
    /// Fits all shapes in the editor to the viewport.
    /// </summary>
    /// <param name="delta">
    ///     A number between <c>0</c> and <c>1</c> inclusive that defines how much of the full transformation should be made.
    ///     <c>1</c> mean that it fits the shapes perfectly in the center.
    ///     <c>0.5</c> means that it makes half the zoom and translation change torwards the perfect fit.
    /// </param>
    /// <param name="padding">How much padding there should be around the shapes to the edge of the editor. The unit is pixels in the rendered space not the space of the SVG that is edited.</param>
    public void FitViewportToAllShapes(double delta = 1, double padding = 20)
    {
        FitViewportToShapes(Elements.Where(e => e is Shape).Cast<Shape>(), delta, padding);
    }

    /// <summary>
    /// Fits the selected shapes in the editor to the viewport.
    /// </summary>
    /// <param name="delta">
    ///     A number between <c>0</c> and <c>1</c> inclusive that defines how much of the full transformation should be made.
    ///     <c>1</c> mean that it fits the shapes perfectly in the center.
    ///     <c>0.5</c> means that it makes half the zoom and translation change torwards the perfect fit.
    /// </param>
    /// <param name="padding">How much padding there should be around the shapes to the edge of the editor. The unit is pixels in the rendered space not the space of the SVG that is edited.</param>
    public void FitViewporToSelectedShapes(double delta = 1, double padding = 20)
    {
        FitViewportToShapes(SelectedShapes, delta, padding);
    }

    /// <summary>
    /// Fits the <paramref name="shapes"/> to the viewport.
    /// </summary>
    /// <param name="shapes">The shapes to fit the viewport to.</param>
    /// <param name="delta">
    ///     A number between <c>0</c> and <c>1</c> inclusive that defines how much of the full transformation should be made.<br />
    ///     <c>1</c> mean that it fits the shapes perfectly in the center.<br />
    ///     <c>0.5</c> means that it makes half the zoom and translation change torwards the perfect fit.
    /// </param>
    /// <param name="padding">How much padding there should be around the shapes to the edge of the editor. The unit is pixels in the rendered space not the space of the SVG that is edited.</param>
    public void FitViewportToShapes(IEnumerable<Shape> shapes, double delta = 1, double padding = 20)
    {
        if (BBox is null)
        {
            return;
        }

        double lowerX = double.MaxValue, lowerY = double.MaxValue;
        double upperX = double.MinValue, upperY = double.MinValue;
        foreach (Shape shape in shapes)
        {
            foreach ((double x, double y) in shape.SelectionPoints)
            {
                double strokeWidth = double.TryParse(shape.StrokeWidth, CultureInfo.InvariantCulture, out double result) ? result : 0;
                lowerX = Math.Min(x - strokeWidth, lowerX);
                upperX = Math.Max(x + strokeWidth, upperX);
                lowerY = Math.Min(y - strokeWidth, lowerY);
                upperY = Math.Max(y + strokeWidth, upperY);
            }
        }
        double elementsWidth = upperX - lowerX;
        double elementsHeight = upperY - lowerY;
        double newScale = Math.Min((BBox.Width - (padding * 2)) / elementsWidth, (BBox.Height - (padding * 2)) / elementsHeight);
        (double x, double y) newTranslate = ((BBox.Width / 2) - ((lowerX + (elementsWidth / 2)) * newScale), (BBox.Height / 2) - ((lowerY + (elementsHeight / 2)) * newScale));
        Scale = (Scale * (1 - delta)) + (newScale * delta);
        Translate = ((Translate.x * (1 - delta)) + (newTranslate.x * delta), (Translate.y * (1 - delta)) + (newTranslate.y * delta));
    }
}
