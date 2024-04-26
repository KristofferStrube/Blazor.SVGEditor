using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.MenuItems.Animation;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateStroke : BaseAnimate
{
    public AnimateStroke(IElement element, ISVGElement parent, SVGEditor svg) : base(element, parent, svg) { }

    public override Type Presenter => typeof(AnimateDefaultEditor);
    public override Type MenuItem => typeof(AnimateStrokeMenuItem);

    public override bool IsEditing(string property)
    {
        return property == "stroke" && CurrentFrame.HasValue;
    }

    public override void AddFrame()
    {
        Values.Add(Parent is Shape s ? s.Stroke : "black");
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

        AnimateStroke animate = new(element, parent, SVG)
        {
            AttributeName = "stroke",
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
