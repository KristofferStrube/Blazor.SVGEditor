using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVG
{
    private void ContextZoomIn()
    {
        ZoomIn(LastRightClick.x, LastRightClick.y, 1.5);
    }

    private void ContextZoomOut()
    {
        ZoomOut(LastRightClick.x, LastRightClick.y, 1.5);
    }

    private void OpenColorPicker(AttributeNames attribute)
    {
        ColorPickerShapes = MarkedShapes.Where(e => e is Shape).Select(e => (Shape)e).ToList();
        ColorPickerAttribute = attribute;
    }

    public void OpenAnimateColorPicker(BaseAnimate animate, AttributeNames attribute, int frame)
    {
        ColorPickerAnimate = animate;
        ColorPickerAnimateFrame = frame;
        ColorPickerAttribute = attribute;
        StateHasChanged();
    }

    private void ColorPickerClosed(string value)
    {
        if (ColorPickerAttribute == AttributeNames.Fill)
        {
            ColorPickerShapes.ForEach(shape => shape.Fill = value);
        }
        else if (ColorPickerAttribute == AttributeNames.Stroke)
        {
            ColorPickerShapes.ForEach(shape => shape.Stroke = value);
        }
        else if (ColorPickerAttribute is AttributeNames.FillAnimate or AttributeNames.StrokeAnimate)
        {
            ColorPickerAnimate.Values[ColorPickerAnimateFrame] = value;
            ColorPickerAnimate.UpdateValues();
        }
        ColorPickerShapes = null;
        ColorPickerAnimate = null;
    }

    private void MoveToBack(Shape shape)
    {
        SelectedShapes.Clear();
        Elements.Remove(shape);
        Elements.Insert(0, shape);
        Elements.ForEach(e => e.UpdateHtml());
        UpdateInput();
        RerenderAll();
    }

    private void MoveBack(Shape shape)
    {
        int index = Elements.IndexOf(shape);
        if (index != 0)
        {
            SelectedShapes.Clear();
            Elements.Remove(shape);
            Elements.Insert(index - 1, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }
    }

    private void MoveForward(Shape shape)
    {
        int index = Elements.IndexOf(shape);
        if (index != Elements.Count - 1)
        {
            SelectedShapes.Clear();
            Elements.Remove(shape);
            Elements.Insert(index + 1, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }
    }

    private void MoveToFront(Shape shape)
    {
        SelectedShapes.Clear();
        Elements.Remove(shape);
        Elements.Insert(Elements.Count, shape);
        Elements.ForEach(e => e.UpdateHtml());
        UpdateInput();
        RerenderAll();
    }

    private void CompleteShape(ISVGElement sVGElement)
    {
        if (SelectedShapes.Count == 1)
        {
            EditMode = EditMode.None;
            sVGElement.Complete();
            SelectedShapes.Clear();
        }
    }

    private void CompleteShapeWithoutClose(Path path)
    {
        CompleteShape(path);
        path.Instructions.Remove(path.Instructions.Last());
        path.UpdateData();
    }

    private static void DeletePreviousInstruction(Path path)
    {
        path.Instructions = path.Instructions.Take(0..^2).ToList();
        path.UpdateData();
    }

    private void ScaleShape(Shape shape)
    {
        SelectedShapes.Clear();
        SelectedShapes.Add(shape);
        if (FocusedShapes != shape)
        {
            FocusedShapes = null;
        }
        EditMode = EditMode.Scale;
    }

    public void Remove()
    {
        MarkedShapes.ForEach(e => Elements.Remove(e));
        SelectedShapes.Clear();
        UpdateInput();
        RerenderAll();
    }

    public async Task CopyElementsAsync()
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", string.Join("\n", MarkedShapes.Select(e => e.StoredHtml)));
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
        SelectedShapes.Clear();
        InputUpdated(string.Join("\n", elementsAsHtml));
    }

    public void Group(Shape shape)
    {
        List<string> elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
        if (MarkedShapes.Count == 1)
        {
            int pos = Elements.IndexOf(shape);
            elementsAsHtml[pos] = "<g>" + shape.StoredHtml + "</g>";
        }
        else
        {
            ISVGElement frontElement = MarkedShapes.MaxBy(e => Elements.IndexOf(e));
            elementsAsHtml[Elements.IndexOf(frontElement)] = "<g>\n" + string.Join("\n", MarkedShapes.OrderBy(e => Elements.IndexOf(e)).Select(e => e.StoredHtml)) + "\n</g>";
            foreach (ISVGElement element in MarkedShapes.Where(e => e != frontElement))
            {
                int pos = Elements.IndexOf(element);
                Elements.RemoveAt(pos);
                elementsAsHtml.RemoveAt(pos);
            }
        }
        SelectedShapes.Clear();
        InputUpdated(string.Join("\n", elementsAsHtml));
    }

    public void Ungroup(G g)
    {
        List<string> elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
        int pos = Elements.IndexOf(g);
        elementsAsHtml[pos] = string.Join("\n", g.ChildShapes.Select(e => e.StoredHtml));
        SelectedShapes.Clear();
        InputUpdated(string.Join("\n", elementsAsHtml));
    }

    protected void StopAnimation()
    {
        MarkedShapes
            .Where(e => e is Shape)
            .ToList()
            .ForEach(s =>
            {
                s.AnimationElements.ForEach(a => {
                    a.Playing = false;
                });
            });
    }

    protected void PlayAnimation()
    {
        MarkedShapes
            .Where(e => e is Shape)
            .ToList()
            .ForEach(s =>
            {
                s.AnimationElements.ForEach(a => {
                    a.Playing = true;
                });
            });
    }
}
