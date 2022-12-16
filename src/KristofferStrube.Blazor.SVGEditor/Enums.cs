namespace KristofferStrube.Blazor.SVGEditor;

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

public enum Linecap
{
    Butt,
    Round,
    Square,
}

public enum Linejoin
{
    Miter,
    MiterClip,
    Round,
    Bevel,
    Arcs,
}