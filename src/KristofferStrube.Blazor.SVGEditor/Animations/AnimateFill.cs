using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.MenuItems.Animation;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateFill : BaseAnimate
{
    public AnimateFill(IElement element, ISVGElement parent, SVGEditor svg) : base(element, parent, svg) { }

    public override Type Presenter => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateFillMenuItem);

    public override bool IsEditing(string property)
    {
        return property == "fill" && CurrentFrame.HasValue;
    }

    public override void AddFrame()
    {
        Values.Add(Parent is Shape s ? s.Fill : "lightgrey");
        UpdateValues();
        Parent.Changed?.Invoke(Parent);
    }

    public override void RemoveFrame(int frame)
    {
        Values.RemoveAt(frame);
        UpdateValues();
        Parent.Changed?.Invoke(Parent);
    }

    public static void AddNew(SVGEditor SVG, Shape parent)
    {
        IElement element = SVG.Document.CreateElement("ANIMATE");

        AnimateFill animate = new(element, parent, SVG)
        {
            AttributeName = "fill",
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
