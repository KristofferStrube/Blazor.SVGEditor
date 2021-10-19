using Microsoft.AspNetCore.Components.Web;
using AngleSharp.Dom;
using static System.Text.Json.JsonSerializer;
using AngleSharp;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class G : Shape
    {
        public G(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;

            ChildElements = Element.Children.Select(child =>
            {
                ISVGElement ChildSVGElement;
                if (SVG.SupportedTypes.ContainsKey(child.TagName))
                {
                    ChildSVGElement = (ISVGElement)Activator.CreateInstance(SVG.SupportedTypes[child.TagName], child, SVG);
                }
                else
                {
                    ChildSVGElement = new NonImplmentedElement();
                }
                ChildSVGElement.Changed = UpdateInput;
                return ChildSVGElement;
            }).ToList();

            ChildElementsAsHtml = Element.Children.Select(child => child.ToHtml()).ToList();
        }

        private void UpdateInput(ISVGElement child)
        {
            ChildElementsAsHtml[ChildElements.IndexOf(child)] = child.ToHtml();
            Changed.Invoke(this);
        }

        public override Type Editor => typeof(GEditor);

        public override string StateRepresentation => string.Join("-", ChildElements.Select(c => c._StateRepresentation)) + string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);

        public List<ISVGElement> ChildElements { get; set; } = new List<ISVGElement>();

        private List<string> ChildElementsAsHtml { get; set; } = new List<string>();

        public override string ToHtml() => $"<g {string.Join(" ", Element.Attributes.Select(a => $"{a.Name}=\"{a.Value}\""))}>\n" + string.Join("\n", ChildElementsAsHtml) + "\n</g>";

        public override void ReRender()
        {
            ChildElements.ForEach(c => c.ReRender());
            _StateRepresentation = null;
        }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Move:

                    foreach (ISVGElement child in ChildElements)
                    {
                        child.EditMode = EditMode.Move;
                        child.Panner = (Panner.x, Panner.y);
                    }
                    foreach (ISVGElement child in ChildElements)
                    {
                        child.HandleMouseMove(eventArgs);
                        child.EditMode = EditMode.None;
                    }
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    BoundingBox.x += diff.x;
                    BoundingBox.y += diff.y;
                    Panner = (x: pos.x, y: pos.y);
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                    EditMode = EditMode.None;
                    break;
            }
        }
        public override void Complete()
        {
            SVG.Remove(this);
        }
    }
}