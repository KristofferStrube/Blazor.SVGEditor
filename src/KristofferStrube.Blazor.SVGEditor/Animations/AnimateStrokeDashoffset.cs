using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.AnimationMenuItems;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using KristofferStrube.Blazor.SVGEditor.PathDataSequences;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateStrokeDashoffset : BaseAnimate
{
    public AnimateStrokeDashoffset(IElement element, SVG svg) : base(element, svg) { }

    public override Type Presenter => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateStrokeDashoffsetMenuItem);

    public void SetParentStrokeDashoffset(int? frame)
    {
        CurrentFrame = frame;
        if (Parent is Path path)
        {
            if (frame is int i)
            {
                path.StrokeDashoffset = Values[i].ParseAsDouble();
            }
            else
            {
                path.StrokeDashoffset = path.Element.GetAttributeOrEmpty("stroke-dashoffset").ParseAsDouble();
            }
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
        Parent.Changed(Parent);
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
                Parent.Changed(Parent);
            }
        }
    }

    public static void AddNew(SVG SVG, Shape parent)
    {
        IElement element = SVG.Document.CreateElement("ANIMATE");

        AnimateStrokeDashoffset animate = new(element, SVG)
        {
            AttributeName = "stroke-dashoffset",
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
