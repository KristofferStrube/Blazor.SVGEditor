using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateFill : BaseAnimate
{
    public AnimateFill(IElement element, SVG svg) : base(element, svg) { }

    public override Type Editor => typeof(AnimateFillEditor);
}
