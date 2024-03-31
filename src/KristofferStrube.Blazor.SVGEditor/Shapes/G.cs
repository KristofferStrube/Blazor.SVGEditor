using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor;

public class G : Shape
{
    public G(IElement element, SVGEditor svg) : base(element, svg)
    {
        ChildShapes = Element.Children.Select(child =>
        {
            if (SVG.SupportedElements.FirstOrDefault(se => se.CanHandle(child))?.ElementType is not Type type)
            {
                throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            }
            var childShape = (Shape)Activator.CreateInstance(type, child, SVG)!;
            childShape.Changed = UpdateInput;
            return childShape;
        }).ToList();
    }

    private void UpdateInput(ISVGElement child)
    {
        child.UpdateHtml();
        Changed?.Invoke(this);
    }

    public override Type Presenter => typeof(GEditor);

    public override string StateRepresentation => string.Join("-", ChildShapes.Select(c => c.StateRepresentation)) + string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + SVG.EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);

    public List<Shape> ChildShapes { get; set; } = [];

    public override IEnumerable<(double x, double y)> SelectionPoints => ChildShapes.SelectMany(child => child.SelectionPoints);

    public override void UpdateHtml()
    {
        ChildShapes.ForEach(e => e.UpdateHtml());
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<g{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>\n" + string.Join("", ChildShapes.Select(e => e.StoredHtml + "\n")) + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + "</g>";
    }

    public override void Rerender()
    {
        ChildShapes.ForEach(c => c.Rerender());
        _stateRepresentation = string.Empty;
    }

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
        switch (SVG.EditMode)
        {
            case EditMode.Move:
                foreach (Shape child in ChildShapes)
                {
                    child.HandlePointerMove(eventArgs);
                }
                (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                BoundingBox.X += diff.x;
                BoundingBox.Y += diff.y;
                break;
            case EditMode.None:
                break;
            case EditMode.Add:
                break;
            case EditMode.MoveAnchor:
                break;
            case EditMode.Scale:
                break;
            default:
                break;
        }
    }

    public override void HandlePointerOut(PointerEventArgs eventArgs)
    {
    }

    public override void HandlePointerUp(PointerEventArgs eventArgs)
    {
        switch (SVG.EditMode)
        {
            case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                SVG.EditMode = EditMode.None;
                break;
            default:
                break;
        }
    }
    public override void Complete()
    {
    }

    public override void SnapToInteger()
    {
        foreach (Shape child in ChildShapes)
        {
            child.SnapToInteger();
        }
    }
}
