using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Animations;
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
    public Action? InputRendered { get; set; }

    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.WindowSelection;

    [Parameter]
    public bool SnapToInteger { get; set; } = false;

    [Parameter]
    public bool HideElements { get; set; } = false;

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
    public List<CompleteNewShapeMenuItem> CompleteNewShapeMenuItems { get; set; } = [
        new(typeof(CompleteWithoutCloseMenuItem), (svgEditor) => svgEditor.SelectedShapes[0] is Path),
        new(typeof(RemoveLastInstruction), (svgEditor) => svgEditor.SelectedShapes[0] is Path),
    ];

    [Parameter]
    public List<SupportedAddNewSVGElementMenuItem> AddNewSVGElementMenuItems { get; set; } = [
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
    ];

    [Parameter]
    public List<ActionMenuItem> ActionMenuItems { get; set; } = [
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
    ];

    [Parameter]
    public List<SupportedElement> SupportedElements { get; set; } = [
        new(typeof(Rect), (IElement element) => element.TagName.ToUpper() == "RECT"),
        new(typeof(Circle), (IElement element) => element.TagName.ToUpper() == "CIRCLE"),
        new(typeof(Ellipse), (IElement element) => element.TagName.ToUpper() == "ELLIPSE"),
        new(typeof(Polygon), (IElement element) => element.TagName.ToUpper() == "POLYGON"),
        new(typeof(Polyline), (IElement element) => element.TagName.ToUpper() == "POLYLINE"),
        new(typeof(Line), (IElement element) => element.TagName.ToUpper() == "LINE"),
        new(typeof(Path), (IElement element) => element.TagName.ToUpper() == "PATH"),
        new(typeof(Text), (IElement element) => element.TagName.ToUpper() == "TEXT"),
        new(typeof(G), (IElement element) => element.TagName.ToUpper() == "G"),
        new(typeof(Defs), (IElement element) => element.TagName.ToUpper() == "DEFS"),
    ];

    [Parameter]
    public List<SupportedAnimation> AnimationTypes { get; set; } =
    [
        new(typeof(AnimateFill), "fill"),
        new(typeof(AnimateStroke), "stroke"),
        new(typeof(AnimateStrokeDashoffset), "stroke-dashoffset"),
        new(typeof(AnimateD), "d"),
    ];

    public void DisableAllInteractions()
    {
        DisableContextMenu = true;
        DisableZoom = true;
        DisablePanning = true;
        DisableDeselecting = true;
        DisableSelecting = true;
        DisableBoxSelection = true;
        DisableMoveEditMode = true;
        DisableMoveAnchorEditMode = true;
        DisableRemoveElement = true;
        DisableCopyElement = true;
        DisablePasteElement = true;
        DisableScaleLabel = true;
    }

    public void EnableAllInteractions()
    {
        DisableContextMenu = false;
        DisableZoom = false;
        DisablePanning = false;
        DisableDeselecting = false;
        DisableSelecting = false;
        DisableBoxSelection = false;
        DisableMoveEditMode = false;
        DisableMoveAnchorEditMode = false;
        DisableRemoveElement = false;
        DisableCopyElement = false;
        DisablePasteElement = false;
        DisableScaleLabel = false;
    }

    public void ClearCompleteNewShapeMenuItems()
    {
        CompleteNewShapeMenuItems.Clear();
    }

    public void ClearAddNewSVGElementMenuItems()
    {
        AddNewSVGElementMenuItems.Clear();
    }

    public void ClearActionMenuItems()
    {
        ActionMenuItems.Clear();
    }

    public void ClearSupportedTypes()
    {
        SupportedElements.Clear();
    }

    public void ClearAnimationTypes()
    {
        AnimationTypes.Clear();
    }

    public void ClearAllMenuItems()
    {
        ClearCompleteNewShapeMenuItems();
        ClearAddNewSVGElementMenuItems();
        ClearActionMenuItems();
    }

    public void ClearAllSupportedTypes()
    {
        ClearSupportedTypes();
        ClearAnimationTypes();
    }

    public void DisableAndClearAllFeatures()
    {
        DisableAllInteractions();
        ClearAllMenuItems();
        ClearAllSupportedTypes();
    }
}
