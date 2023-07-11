namespace KristofferStrube.Blazor.SVGEditor;

public record ActionMenuItem(Type ComponentType, Func<SVGEditor, object?, bool> ShouldBePresented);