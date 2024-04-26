using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.MenuItems.Animation;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateD : BaseAnimate
{
    public AnimateD(IElement element, ISVGElement parent, SVGEditor svg) : base(element, parent, svg) { }

    public override Type Presenter => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateDMenuItem);

    public void SetParentD(int? frame)
    {
        CurrentFrame = frame;
        if (Parent is Path path)
        {
            path.Instructions = frame is int i ? PathData.Parse(Values[i]) : PathData.Parse(path.Element.GetAttributeOrEmpty("d"));
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
        Parent.Changed?.Invoke(Parent);
    }

    public override void RemoveFrame(int frame)
    {
        if (Parent is Path path)
        {
            if (CurrentFrame == frame)
            {
                CurrentFrame = null;
                path.Instructions = PathData.Parse(path.Element.GetAttributeOrEmpty("d"));
            }
            else
            {
                Values.RemoveAt(frame);
                UpdateValues();
                Parent.Changed?.Invoke(Parent);
            }
        }
    }

    public static void AddNew(SVGEditor SVG, Shape parent)
    {
        IElement element = SVG.Document.CreateElement("ANIMATE");

        AnimateD animate = new(element, parent, SVG)
        {
            AttributeName = "d",
            Values = [],
            Begin = 0,
            Dur = 5,
        };
        animate.AddFrame();
        animate.UpdateValues();
        SVG.EditMode = EditMode.None;
        SVG.ClearSelectedShapes();

        SVG.AddElement(animate, parent);
        parent.AnimationElements.Add(animate);
    }
}
