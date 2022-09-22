using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.AnimationEditors;
using KristofferStrube.Blazor.SVGEditor.AnimationMenuItems;

namespace KristofferStrube.Blazor.SVGEditor;

public class AnimateFill : BaseAnimate
{
    public AnimateFill(IElement element, SVG svg) : base(element, svg) { }

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
        Parent.Changed(Parent);
    }

    public static void AddNew(SVG SVG, Shape parent)
    {
        IElement element = SVG.Document.CreateElement("ANIMATE");

        AnimateFill animate = new(element, SVG)
        {
            AttributeName = "fill",
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
