using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.ShapeEditors;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor;

public class G : Shape
{
    public G(IElement element, SVG svg) : base(element, svg)
    {
        ChildShapes = Element.Children.Select(child =>
        {
            Shape ChildShape;
            if (SVG.SupportedTypes.ContainsKey(child.TagName))
            {
                ChildShape = (Shape)Activator.CreateInstance(SVG.SupportedTypes[child.TagName], child, SVG);
            }
            else
            {
                throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            }
            ChildShape.Changed = UpdateInput;
            return ChildShape;
        }).ToList();
    }

    private void UpdateInput(ISVGElement child)
    {
        child.UpdateHtml();
        Changed.Invoke(this);
    }

    public override Type Editor => typeof(GEditor);

    public override string StateRepresentation => string.Join("-", ChildShapes.Select(c => c.StateRepresentation)) + string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + SVG.EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);

    public List<Shape> ChildShapes { get; set; } = new List<Shape>();

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
        _stateRepresentation = null;
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
        }
    }
    public override void Complete()
    {
    }

    public override void SnapToInteger()
    {
        foreach(var child in ChildShapes)
        {
            child.SnapToInteger();
        }
    }
}
