using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Path : Shape
    {
        public Path(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
            try
            {
                Instructions = PathData.Parse(Element.GetAttribute("d") ?? string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Instructions = new();
            }
        }
        public List<IPathInstruction> Instructions { get; set; }

        private void UpdateData()
        {
            if (Instructions.Count > 0)
            {
                Element.SetAttribute("d", Instructions.AsString());
                Changed.Invoke(this);
            }
        }
        public int? CurrentInstruction { get; set; }
        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = (x: eventArgs.OffsetX / SVG.Scale, y: eventArgs.OffsetY / SVG.Scale);
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = -1;
                    }
                    var inst = Instructions[(int)CurrentInstruction];
                    var prev = inst.PreviousInstruction;
                    if (CurrentAnchor == -1)
                    {
                        switch (inst)
                        {
                            case LineInstruction or MoveInstruction:
                                inst.EndPosition = (pos.x, pos.y);
                                break;
                            case CubicBézierCurveInstruction or ShorthandCubicBézierCurveInstruction:
                                var diffX = pos.x - inst.EndPosition.x;
                                var diffY = pos.y - inst.EndPosition.y;
                                inst.EndPosition = (pos.x, pos.y);
                                var controlPointInstruction = (BaseControlPointPathInstruction)inst;
                                controlPointInstruction.ControlPoints = controlPointInstruction.ControlPoints.Select(p => (p.x + diffX, p.y + diffY)).ToList();
                                break;
                            case HorizontalLineInstruction:
                                inst.EndPosition = (pos.x, pos.y);
                                while (prev is HorizontalLineInstruction)
                                {
                                    prev = prev.PreviousInstruction;
                                }
                                if (prev is ClosePathInstruction closeBeforeHorizontal)
                                {
                                    prev = closeBeforeHorizontal.GetReferenceInstruction();
                                }
                                prev.EndPosition = (prev.EndPosition.x, prev.EndPosition.y + (pos.y - inst.EndPosition.y));
                                break;
                            case VerticalLineInstruction:
                                while (prev is VerticalLineInstruction)
                                {
                                    prev = prev.PreviousInstruction;
                                }
                                if (prev is ClosePathInstruction closeBeforeVertical)
                                {
                                    prev = closeBeforeVertical.GetReferenceInstruction();
                                }
                                inst.EndPosition = (pos.x, pos.y);
                                prev.EndPosition = (prev.EndPosition.x + (pos.x - inst.EndPosition.x), prev.EndPosition.y);
                                break;
                        }
                    }
                    else if (CurrentAnchor == -2)
                    {
                        var controlPointInstruction = (BaseControlPointPathInstruction)inst;
                        controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = (pos.x, pos.y);
                    }
                    else if (inst.GetType().IsSubclassOf(typeof(BaseControlPointPathInstruction)))
                    {
                        var controlPointInstruction = (BaseControlPointPathInstruction)inst;
                        controlPointInstruction.ControlPoints[(int)CurrentAnchor] = (pos.x, pos.y);
                    }
                    UpdateData();
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (pos.x, pos.y);
                    Instructions = Instructions.Select(inst =>
                    {
                        inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y);
                        if (inst.GetType().IsSubclassOf(typeof(BaseControlPointPathInstruction)))
                        {
                            var ControlPointInstruction = ((BaseControlPointPathInstruction)inst);
                            ControlPointInstruction.ControlPoints = ControlPointInstruction.ControlPoints.Select(p => (p.x + diff.x, p.y + diff.y)).ToList();
                        }
                        return inst;
                    }).ToList();
                    UpdateData();
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    CurrentAnchor = null;
                    EditMode = EditMode.None;
                    break;
                case EditMode.Move:
                    EditMode = EditMode.None;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }
    }
}
