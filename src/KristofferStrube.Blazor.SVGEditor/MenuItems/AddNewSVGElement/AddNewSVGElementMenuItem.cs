namespace KristofferStrube.Blazor.SVGEditor.MenuItems.AddNewSVGElement;

public record AddNewSVGElementMenuItem(Type ComponentType, Func<SVGEditor, object, bool> ShouldBePresented);
