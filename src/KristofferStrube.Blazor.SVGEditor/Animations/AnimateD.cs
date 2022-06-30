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
            path.Changed?.Invoke(path);
        }
    }

    public override bool IsEditing(string property)
    {
        return property == "d" && CurrentFrame.HasValue;
    }

    public override void AddFrame()
    {
        Values.Add(Parent is Path p ? p.Instructions.AsString() : "M 0 0 L 10 10");
        UpdateValues();
        Parent.Changed(Parent);
    }

    public static void AddNew(SVG SVG, Shape parent)
    {
        IElement element = SVG.Document.CreateElement("ANIMATE");

        AnimateD animate = new(element, SVG)
        {
            AttributeName = "d",
            Parent = parent,
            Values = new(),
            Begin = 0,
            Dur = 5,
        };
        animate.AddFrame();
        animate.UpdateValues();
        SVG.EditMode = EditMode.None;
        SVG.SelectedShapes.Clear();

        SVG.AddElement(animate, parent);
        parent.AnimationElements.Add(animate);
    }
}
