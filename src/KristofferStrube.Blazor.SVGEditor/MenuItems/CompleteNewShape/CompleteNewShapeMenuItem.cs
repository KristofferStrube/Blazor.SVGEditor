namespace KristofferStrube.Blazor.SVGEditor.MenuItems.CompleteNewShape;

public record CompleteNewShapeMenuItem(Type ComponentType, Func<SVGEditor, bool> ShouldBePresented);
