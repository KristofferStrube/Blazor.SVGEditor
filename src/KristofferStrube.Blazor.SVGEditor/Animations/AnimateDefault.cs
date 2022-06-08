using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateDefault : BaseAnimate
{
    public AnimateDefault(IElement element, SVG svg) : base(element, svg) { }

    public override Type Editor => typeof(AnimateDefaultEditor);
}
