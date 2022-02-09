using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class G : Shape
    {
        public G(IElement element, SVG svg) : base(element, svg)
        {
            ChildElements = Element.Children.Select(child =>
            {
                ISVGElement ChildSVGElement;
                if (SVG.SupportedTypes.ContainsKey(child.TagName))
                {
                    ChildSVGElement = (ISVGElement)Activator.CreateInstance(SVG.SupportedTypes[child.TagName], child, SVG);
                }
                else
                {
                    throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
                }
                ChildSVGElement.Changed = UpdateInput;
                return ChildSVGElement;
            }).ToList();
        }

        private void UpdateInput(ISVGElement child)
        {
            child.UpdateHtml();
            Changed.Invoke(this);
        }

        public override Type Editor => typeof(GEditor);

        public override string StateRepresentation => string.Join("-", ChildElements.Select(c => c.StateRepresentation)) + string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + SVG.EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);

        public List<ISVGElement> ChildElements { get; set; } = new List<ISVGElement>();

        public override List<(double x, double y)> SelectionPoints => throw new NotImplementedException();

        public override void UpdateHtml()
        {
            ChildElements.ForEach(e => e.UpdateHtml());
            StoredHtml = $"<g {string.Join(" ", Element.Attributes.Select(a => $"{a.Name}=\"{a.Value}\""))}>\n" + string.Join("\n", ChildElements.Select(e => e.StoredHtml)) + "\n</g>";
        }

        public override void Rerender()
        {
            ChildElements.ForEach(c => c.Rerender());
            _stateRepresentation = null;
        }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            (double x, double y) = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (SVG.EditMode)
            {
                case EditMode.Move:
                    foreach (ISVGElement child in ChildElements)
                    {
                        child.HandleMouseMove(eventArgs);
                    }
                    (double x, double y) diff = (x: x - SVG.MovePanner.x, y: y - SVG.MovePanner.y);
                    BoundingBox.X += diff.x;
                    BoundingBox.Y += diff.y;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
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
    }
}