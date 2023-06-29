using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.SVGEditor;

public partial class SVGEditor
{
    public void OpenColorPicker(string attributeName, string previousColor, Action<string> colorSetter)
    {
        ColorPickerShapes = MarkedShapes;
        PreviousColor = previousColor;
        ColorPickerAttributeName = attributeName;
        ColorPickerSetter = colorSetter;
        StateHasChanged();
    }

    private void ColorPickerClosed(string value)
    {
        if (ColorPickerAttributeName is "Fill")
        {
            ColorPickerShapes?.ForEach(s => s.Fill = value);
        }
        else if (ColorPickerAttributeName is "Stroke")
        {
            ColorPickerShapes?.ForEach(s => s.Stroke = value);
        }
        else
        {
            ColorPickerSetter?.Invoke(value);
        }
        ColorPickerShapes = null;
    }

    private void SetFillForMarkedShapes(string value)
    {
        MarkedShapes.ForEach(shape => shape.Fill = value);
    }

    private void SetStrokeForMarkedShapes(string value)
    {
        MarkedShapes.ForEach(shape => shape.Stroke = value);
    }

    private void MoveToBack(Shape shape)
    {
        ClearSelectedShapes();
        _ = Elements.Remove(shape);
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
            ClearSelectedShapes();
            _ = Elements.Remove(shape);
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
            ClearSelectedShapes();
            _ = Elements.Remove(shape);
            Elements.Insert(index + 1, shape);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }
    }

    private void MoveToFront(Shape shape)
    {
        ClearSelectedShapes();
        _ = Elements.Remove(shape);
        Elements.Insert(Elements.Count, shape);
        Elements.ForEach(e => e.UpdateHtml());
        UpdateInput();
        RerenderAll();
    }

    public void CompleteShape(ISVGElement sVGElement)
    {
        if (EditMode is not EditMode.Add) return;
        if (SelectedShapes.Count == 1)
        {
            EditMode = EditMode.None;
            sVGElement.Complete();
            ClearSelectedShapes();
        }
    }

    private void ScaleShape(Shape shape)
    {
        ClearSelectedShapes();
        SelectShape(shape);
        if (FocusedShape != shape)
        {
            UnfocusShape();
        }
        EditMode = EditMode.Scale;
    }

    public void Remove()
    {
        MarkedShapes.ForEach(e => Elements.Remove(e));
        ClearSelectedShapes();
        UpdateInput();
        RerenderAll();
    }

    public async Task CopyElementsAsync()
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", string.Join("\n", MarkedShapes.Select(e => e.StoredHtml)));
    }

    public async Task PasteElementsAsync(ISVGElement? SVGElement = null)
    {
        string clipboard = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
        if (SVGElement != null)
        {
            int index = Elements.IndexOf(SVGElement);
            elementsAsHtml.Insert(index + 1, clipboard);
        }
        else
        {
            elementsAsHtml.Add(clipboard);
        }
        ClearSelectedShapes();
        InputUpdated?.Invoke(string.Join("\n", elementsAsHtml));
    }

    public void Group(Shape shape)
    {
        var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
        if (MarkedShapes.Count == 1)
        {
            int pos = Elements.IndexOf(shape);
            elementsAsHtml[pos] = "<g>" + shape.StoredHtml + "</g>";
        }
        else if (MarkedShapes is { Count: > 1 })
        {
            ISVGElement frontElement = MarkedShapes.MaxBy(Elements.IndexOf)!;
            elementsAsHtml[Elements.IndexOf(frontElement)] = "<g>\n" + string.Join("\n", MarkedShapes.OrderBy(e => Elements.IndexOf(e)).Select(e => e.StoredHtml)) + "\n</g>";
            foreach (ISVGElement element in MarkedShapes.Where(e => e != frontElement))
            {
                int pos = Elements.IndexOf(element);
                Elements.RemoveAt(pos);
                elementsAsHtml.RemoveAt(pos);
            }
        }
        ClearSelectedShapes();
        InputUpdated?.Invoke(string.Join("\n", elementsAsHtml));
    }

    public void Ungroup(G g)
    {
        var elementsAsHtml = Elements.Select(e => e.StoredHtml).ToList();
        int pos = Elements.IndexOf(g);
        elementsAsHtml[pos] = string.Join("\n", g.ChildShapes.Select(e => e.StoredHtml));
        ClearSelectedShapes();
        InputUpdated?.Invoke(string.Join("\n", elementsAsHtml));
    }

    protected void StopAnimation()
    {
        MarkedShapes
            .ForEach(s =>
            {
                s.AnimationElements.ForEach(a =>
                {
                    a.Playing = false;
                });
            });
    }

    protected void PlayAnimation()
    {
        MarkedShapes
            .ForEach(s =>
            {
                s.AnimationElements.ForEach(a =>
                {
                    a.Playing = true;
                });
            });
    }

    protected void SnapShapesToInteger()
    {
        MarkedShapes
            .ForEach(s =>
            {
                s.SnapToInteger();
            });
    }

    protected void CompactPaths()
    {
        MarkedShapes
            .ForEach(s =>
            {
                if (s is Path p)
                {
                    p.ConvertToRelative();
                }
                else if (s is G g)
                {
                    g.ChildShapes.ForEach(c =>
                    {
                        if (c is Path p)
                        {
                            p.ConvertToRelative();
                        }
                    });
                }
            });
    }
}
