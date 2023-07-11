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

    public void MoveToBack(ISVGElement element)
    {
        ClearSelectedShapes();
        _ = Elements.Remove(element);
        Elements.Insert(0, element);
        Elements.ForEach(e => e.UpdateHtml());
        UpdateInput();
        RerenderAll();
    }

    public void MoveBack(ISVGElement element)
    {
        int index = Elements.IndexOf(element);
        if (index != 0)
        {
            ClearSelectedShapes();
            _ = Elements.Remove(element);
            Elements.Insert(index - 1, element);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }
    }

    public void MoveForward(ISVGElement element)
    {
        int index = Elements.IndexOf(element);
        if (index != Elements.Count - 1)
        {
            ClearSelectedShapes();
            _ = Elements.Remove(element);
            Elements.Insert(index + 1, element);
            Elements.ForEach(e => e.UpdateHtml());
            UpdateInput();
            RerenderAll();
        }
    }

    public void MoveToFront(ISVGElement element)
    {
        ClearSelectedShapes();
        _ = Elements.Remove(element);
        Elements.Insert(Elements.Count, element);
        Elements.ForEach(e => e.UpdateHtml());
        UpdateInput();
        RerenderAll();
    }

    public void CompleteShape(Shape shape)
    {
        if (EditMode is not EditMode.Add)
        {
            return;
        }

        if (SelectedShapes.Count == 1)
        {
            EditMode = EditMode.None;
            shape.Complete();
            ClearSelectedShapes();
        }
    }

    public void Remove()
    {
        MarkedShapes.ForEach(e => RemoveElement(e));
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
}
