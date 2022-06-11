using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.AnimationMenuItems;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateDefault : BaseAnimate
{
    public AnimateDefault(IElement element, SVG svg) : base(element, svg) { }

    public override Type Editor => typeof(AnimateFallbackEditor);
    public override Type MenuItem => typeof(AnimateDefaultMenuItem);

    public override bool IsEditing(string property)
    {
        return false;
    }

    public override void AddFrame()
    {
        Values.Add("");
        UpdateValues();
        Parent.Changed(Parent);
    }
}
