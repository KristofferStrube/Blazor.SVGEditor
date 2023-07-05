using KristofferStrube.Blazor.SVGEditor.MenuItems.Action;
using KristofferStrube.Blazor.SVGEditor.MenuItems.AddNewSVGElement;
using KristofferStrube.Blazor.SVGEditor.MenuItems.CompleteNewShape;
using Microsoft.AspNetCore.Components;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVGEditor
{
    [Parameter, EditorRequired]
    public string Input { get; set; } = string.Empty;

    [Parameter]
    public Action<string>? InputUpdated { get; set; }

    [Parameter]
    public bool SnapToInteger { get; set; } = false;

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.WindowSelection;

    [Parameter]
    public bool DisableContextMenu { get; set; }

    [Parameter]
    public bool DisableZoom { get; set; }

    [Parameter]
    public bool DisablePanning { get; set; }

    [Parameter]
    public bool DisableDeselecting { get; set; }

    [Parameter]
    public bool DisableSelecting { get; set; }

    [Parameter]
    public bool DisableBoxSelection { get; set; }

    [Parameter]
    public bool DisableMoveEditMode { get; set; }

    [Parameter]
    public bool DisableMoveAnchorEditMode { get; set; }

    [Parameter]
    public bool DisableRemoveElement { get; set; }

    [Parameter]
    public bool DisableCopyElement { get; set; }

    [Parameter]
    public bool DisablePasteElement { get; set; }

    [Parameter]
    public bool DisableScaleLabel { get; set; }

    [Parameter]
    public List<CompleteNewShapeMenuItem> CompleteNewShapeMenuItems { get; set; } = new() {
        new(typeof(CompleteWithoutCloseMenuItem), (svgEditor) => svgEditor.SelectedShapes[0] is Path),
        new(typeof(RemoveLastInstruction), (svgEditor) => svgEditor.SelectedShapes[0] is Path),
    };

    [Parameter]
    public List<AddNewSVGElementMenuItem> AddNewSVGElementMenuItems { get; set; } = new() {
        new(typeof(AddNewStopFromLinearGradientMenuItem), (svgEditor, data) => data is LinearGradient),
        new(typeof(AddNewStopFromStopMenuItem), (svgEditor, data) => data is Stop),
        new(typeof(AddNewPathMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewPolygonMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewPolylineMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewLineMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewCircleMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewEllipseMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewRectMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewTextMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewGradientMenuItem), (_, data) => data is not (LinearGradient or Stop)),
        new(typeof(AddNewAnimationMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement && !shape.AnimationElements.Any(a => a.AttributeName is "fill" or "stroke" or "d")),
    };

    [Parameter]
    public List<ActionMenuItem> ActionMenuItems { get; set; } = new() {
        new(typeof(FillMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement),
        new(typeof(StrokeMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement),
        new(typeof(TextMenuItem), (_, data) => data is Text text && !text.IsChildElement),
        new(typeof(AnimationsMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement && shape.HasAnimation),
        new(typeof(MoveMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement),
        new(typeof(ScaleMenuItem), (_, data) => data is Path path && !path.IsChildElement),
        new(typeof(GroupMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement),
        new(typeof(UngroupMenuItem), (_, data) => data is G g && !g.IsChildElement),
        new(typeof(RemoveMenuItem), (svgEditor, data) => data is Shape && !svgEditor.DisableRemoveElement),
        new(typeof(CopyMenuItem), (svgEditor, data) => data is Shape && !svgEditor.DisableCopyElement),
        new(typeof(PasteMenuItem), (svgEditor, _) => !svgEditor.DisablePasteElement),
        new(typeof(OptimizeMenuItem), (_, data) => data is Shape shape && !shape.IsChildElement),
    };

    [Parameter]
    public Dictionary<string, Type> SupportedTypes { get; set; } = new() {
        { "RECT", typeof(Rect) },
        { "CIRCLE", typeof(Circle) },
        { "ELLIPSE", typeof(Ellipse) },
        { "POLYGON", typeof(Polygon) },
        { "POLYLINE", typeof(Polyline) },
        { "LINE", typeof(Line) },
        { "PATH", typeof(Path) },
        { "TEXT", typeof(Text) },
        { "G", typeof(G) },
        { "DEFS", typeof(Defs) },
    };

    [Parameter]
    public Dictionary<string, Type> AnimationTypes { get; set; } = new()
    {
        { "fill", typeof(AnimateFill) },
        { "stroke", typeof(AnimateStroke) },
        { "stroke-dashoffset", typeof(AnimateStrokeDashoffset) },
        { "d", typeof(AnimateD) },
    };

}
