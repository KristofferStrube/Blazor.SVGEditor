namespace KristofferStrube.Blazor.SVGEditor;

public record CompleteNewShapeMenuItem(Type ComponentType, Func<SVGEditor, bool> ShouldBePresented);