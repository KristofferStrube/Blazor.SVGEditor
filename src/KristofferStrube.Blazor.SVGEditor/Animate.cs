using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor;

public class Animate
{
    public Animate(Shape shape, IElement element)
    {
        Shape = shape;
        Element = element;
        Values = StringToValues(Element.GetAttribute("values"));
    }

    public Shape Shape { get; set; }
    public IElement Element { get; set; }

    public int FrameCount => Values.Count;

    public void UpdateValues()
    {
        Element.SetAttribute("values", ValuesToString(Values));
        Shape.Changed.Invoke(Shape);
        Shape.Rerender();
    }

    public List<string> Values { get; set; }

    private static List<string> StringToValues(string attribute)
    {
        if (attribute == null)
        {
            return new List<string>();
        }

        return attribute.Split(";").Select(e => e.Trim()).ToList();
    }

    private static string ValuesToString(List<string> values)
    {
        return string.Join(";", values);
    }
}
