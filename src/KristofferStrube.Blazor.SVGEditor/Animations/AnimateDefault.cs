using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.AnimationMenuItems;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateDefault : BaseAnimate
{
    public AnimateDefault(IElement element, SVG svg) : base(element, svg) { }

    public override Type Presenter => typeof(AnimateFallbackEditor);
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

    public override void RemoveFrame(int frame)
    {
        Values.RemoveAt(frame);
        UpdateValues();
        Parent.Changed(Parent);
    }
}
