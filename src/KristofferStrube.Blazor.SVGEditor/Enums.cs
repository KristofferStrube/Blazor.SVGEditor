namespace KristofferStrube.Blazor.SVGEditor
{
    public enum EditMode
    {
        None,
        Add,
        Move,
        MoveAnchor,
        Scale,
    }

    public enum SelectionMode
    {
        WindowSelection,
        CrossingSelection,
    }
}
