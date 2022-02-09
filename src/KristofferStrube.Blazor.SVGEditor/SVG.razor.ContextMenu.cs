using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor
{
    public partial class SVG
    {
        public void ContextZoomIn()
        {
            ZoomIn(LastRightClick.x, LastRightClick.y, 1.5);
        }

        public void ContextZoomOut()
        {
            ZoomOut(LastRightClick.x, LastRightClick.y, 1.5);
        }

        protected void OpenColorPicker(Shape shape, string attribute)
        {
            ColorPickerShape = shape;
            ColorPickerAttribute = attribute;
        }

        protected void OpenAnimateColorPicker(Animate fillAnimate, string attribute, int frame)
        {
            ColorPickerAnimate = fillAnimate;
            ColorPickerAnimateFrame = frame;
            ColorPickerAttribute = attribute;
        }

        protected void ColorPickerClosed(string value)
        {
            if (ColorPickerAttribute == "Fill")
            {
                ColorPickerShape.Fill = value;
            }
            else if (ColorPickerAttribute == "Stroke")
            {
                ColorPickerShape.Stroke = value;
            }
            else if (ColorPickerAttribute is "FillAnimate" or "StrokeAnimate")
            {
                ColorPickerAnimate.Values[ColorPickerAnimateFrame] = value;
                ColorPickerAnimate.UpdateValues();
            }
            ColorPickerShape = null;
            ColorPickerAnimate = null;
        }

        protected void MoveToBack(Shape shape)
        {
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(0, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }

        protected void MoveBack(Shape shape)
        {
            int index = Elements.IndexOf(shape);
            if (index != 0)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index - 1, shape);
                Elements.ForEach(e => e.UpdateHtml());
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveForward(Shape shape)
        {
            int index = Elements.IndexOf(shape);
            if (index != Elements.Count - 1)
            {
                SelectedElements.Clear();
                Elements.Remove(shape);
                Elements.Insert(index + 1, shape);
                Elements.ForEach(e => e.UpdateHtml());
                UpdateInput();
                RerenderAll();
            }
        }

        protected void MoveToFront(Shape shape)
        {
            SelectedElements.Clear();
            Elements.Remove(shape);
            Elements.Insert(Elements.Count, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }

        protected void CompleteShape(ISVGElement sVGElement)
        {
            if (SelectedElements.Count == 1)
            {
                EditMode = EditMode.None;
                sVGElement.Complete();
                SelectedElements.Clear();
            }
        }

        protected void CompleteShapeWithoutClose(Path path)
        {
            CompleteShape(path);
            path.Instructions.Remove(path.Instructions.Last());
            path.UpdateData();
        }

        protected static void DeletePreviousInstruction(Path path)
        {
            path.Instructions = path.Instructions.Take(0..^2).ToList();
            path.UpdateData();
        }

        protected void ScaleShape(Shape shape)
        {
            SelectedElements.Clear();
            SelectedElements.Add(shape);
            if (FocusedElement != shape)
            {
                FocusedElement = null;
            }
            EditMode = EditMode.Scale;
        }

        public void Remove()
        {
            MarkedElements.ForEach(e => Elements.Remove(e));
            SelectedElements.Clear();
            UpdateInput();
            RerenderAll();
        }

        public async Task CopyElementsAsync()
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", string.Join("\n", MarkedElements.Select(e => e.StoredHtml)));
        }

        public async Task PasteElementsAsync(ISVGElement SVGElement = null)
        {
            string clipboard = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
            List<string> elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            if (SVGElement != null)
            {
                int index = Elements.IndexOf(SVGElement);
                elementsAsHtml.Insert(index + 1, clipboard);
            }
            else
            {
                elementsAsHtml.Add(clipboard);
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        public void Group(Shape shape)
        {
            List<string> elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            if (MarkedElements.Count == 1)
            {
                int pos = Elements.IndexOf(shape);
                elementsAsHtml[pos] = "<g>" + shape.StoredHtml + "</g>";
            }
            else
            {
                ISVGElement frontElement = MarkedElements.MaxBy(e => Elements.IndexOf(e));
                elementsAsHtml[Elements.IndexOf(frontElement)] = "<g>\n" + string.Join("\n", MarkedElements.OrderBy(e => Elements.IndexOf(e)).Select(e => e.StoredHtml)) + "\n</g>";
                foreach (ISVGElement element in MarkedElements.Where(e => e != frontElement))
                {
                    int pos = Elements.IndexOf(element);
                    Elements.RemoveAt(pos);
                    elementsAsHtml.RemoveAt(pos);
                }
            }
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        public void Ungroup(G g)
        {
            List<string> elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
            int pos = Elements.IndexOf(g);
            elementsAsHtml[pos] = string.Join("\n", g.ChildElements.Select(e => e.StoredHtml));
            SelectedElements.Clear();
            InputUpdated(string.Join("\n", elementsAsHtml));
        }

        protected static void ToggleAnimation(Shape shape)
        {
            shape.Playing = !shape.Playing;
            shape.Rerender();
        }
    }
}
