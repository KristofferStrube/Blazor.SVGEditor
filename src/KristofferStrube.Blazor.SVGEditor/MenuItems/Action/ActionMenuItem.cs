namespace KristofferStrube.Blazor.SVGEditor.MenuItems.CompleteNewShape;

public record ActionMenuItem(Type ComponentType, Func<SVGEditor, object?, bool> ShouldBePresented);
