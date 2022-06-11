using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.AnimationMenuItems;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateD : BaseAnimate
{
    public AnimateD(IElement element, SVG svg) : base(element, svg) { }

    public override Type Editor => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateDMenuItem);

    public void SetParentD(int? frame)
    {
        CurrentFrame = frame;
        if (Parent is Path path)
        {
            if (frame is int i)
            {
                path.Instructions = PathData.Parse(Values[i]);
            }
            else
            {
                path.Instructions = PathData.Parse(path.Element.GetAttributeOrEmpty("d"));
            }
        }
    }

    public override bool IsEditing(string property)
    {
        return property == "d" && CurrentFrame.HasValue;
    }
}
