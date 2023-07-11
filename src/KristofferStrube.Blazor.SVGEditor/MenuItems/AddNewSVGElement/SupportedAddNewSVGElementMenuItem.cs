namespace KristofferStrube.Blazor.SVGEditor;

public record SupportedAddNewSVGElementMenuItem(Type ComponentType, Func<SVGEditor, object?, bool> ShouldBePresented);