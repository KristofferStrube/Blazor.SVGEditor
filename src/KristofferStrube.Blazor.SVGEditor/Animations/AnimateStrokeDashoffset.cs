using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.MenuItems.Animation;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateStrokeDashoffset : BaseAnimate
{
    public AnimateStrokeDashoffset(IElement element, ISVGElement parent, SVGEditor svg) : base(element, parent, svg) { }

    public override Type Presenter => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateStrokeDashoffsetMenuItem);

    public void SetParentStrokeDashoffset(int? frame)
    {
        CurrentFrame = frame;
        if (Parent is Path path)
        {
            path.StrokeDashoffset = frame is int i ? Values[i].ParseAsDouble() : path.Element.GetAttributeOrEmpty("stroke-dashoffset").ParseAsDouble();
            path.Changed?.Invoke(path);
        }
    }

    public override bool IsEditing(string property)
    {
        return property == "stroke-dashoffset" && CurrentFrame.HasValue;
    }

    public override void AddFrame()
    {
        Values.Add(Parent is Shape s ? s.StrokeDashoffset.AsString() : "0");
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
                path.StrokeDashoffset = path.Element.GetAttributeOrEmpty("stroke-dashoffset").ParseAsDouble();
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

        AnimateStrokeDashoffset animate = new(element, parent, SVG)
        {
            AttributeName = "stroke-dashoffset",
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
