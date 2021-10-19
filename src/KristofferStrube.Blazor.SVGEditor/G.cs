using Microsoft.AspNetCore.Components.Web;
using AngleSharp.Dom;
using static System.Text.Json.JsonSerializer;
using AngleSharp;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class G : ISVGElement
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

        public IElement Element { get; set; }
        public SVG SVG { get; set; }

        public Type Editor => typeof(GEditor);

        public bool Selectable => SVG.CurrentShape == null;
        public bool Selected => SVG.CurrentShape == this;

        public BoundingBox BoundingBox { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EditMode EditMode { get; set; }
        public (double x, double y) Panner { get; set; }
        public string _StateRepresentation { get; set; }
        public string StateRepresentation => string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);
        public Action<ISVGElement> Changed { get; set; }
        public List<ISVGElement> ChildElements { get; set; } = new List<ISVGElement>();

        private List<string> ChildElementsAsHtml { get; set; } = new List<string>();

        public string ToHtml() => "<g>\n" + string.Join("\n", ChildElementsAsHtml) + "\n</g>";

        public void Complete()
        {
            SVG.Remove(this);
        }

        public int? CurrentAnchor { get; set; }

        public void HandleMouseMove(MouseEventArgs eventArgs)
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

                    Panner = (x: pos.x, y: pos.y);
                    break;
            }
        }

        public void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Move or EditMode.MoveAnchor or EditMode.Add:
                    EditMode = EditMode.None;
                    break;
            }
        }
    }
}