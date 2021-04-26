using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Path : Shape
    {
        public List<IPathInstruction> Instructions
        {
            get
            {
                var path = new List<IPathInstruction>();
                try
                {
                    path = PathData.Parse(Element.GetAttribute("d") ?? string.Empty);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return path;
            }
            set { Element.SetAttribute("d", value.AsString()); Changed.Invoke(this); }
        }
        public int? CurrentInstruction { get; set; }
        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    if (CurrentAnchor == 0)
                    {
                        var _instructions = Instructions;
                        var inst = _instructions[(int)CurrentInstruction];
                        var prev = inst.PreviousInstruction;
                        switch (inst)
                        {
                            case LineInstruction or MoveInstruction or CubicBézierCurveInstruction:
                                inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                                Instructions = _instructions;
                                break;
                            case HorizontalLineInstruction:
                                inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                                while (prev is HorizontalLineInstruction)
                                {
                                    prev = prev.PreviousInstruction;
                                }
                                if (prev is ClosePathInstruction closeBeforeHorizontal)
                                {
                                    prev = closeBeforeHorizontal.GetReferenceInstruction();
                                }
                                prev.EndPosition = (prev.EndPosition.x, prev.EndPosition.y + (eventArgs.OffsetY - inst.EndPosition.y));
                                Instructions = _instructions;
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
                                inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                                prev.EndPosition = (prev.EndPosition.x + (eventArgs.OffsetX - inst.EndPosition.x), prev.EndPosition.y);
                                Instructions = _instructions;
                                break;
                        }
                    }
                    else if (CurrentAnchor == 1)
                    {
                        var _instructions = Instructions;
                        var inst = _instructions[(int)CurrentInstruction];
                        var prev = inst.PreviousInstruction;
                        switch (inst)
                        {
                            case CubicBézierCurveInstruction cubicCurve:
                                cubicCurve.x1 = eventArgs.OffsetX;
                                cubicCurve.y1 = eventArgs.OffsetY;
                                Instructions = _instructions;
                                break;
                        }
                    }
                    else if (CurrentAnchor == 2)
                    {
                        var _instructions = Instructions;
                        var inst = _instructions[(int)CurrentInstruction];
                        var prev = inst.PreviousInstruction;
                        switch (inst)
                        {
                            case CubicBézierCurveInstruction cubicCurve:
                                cubicCurve.x2 = eventArgs.OffsetX;
                                cubicCurve.y2 = eventArgs.OffsetY;
                                Instructions = _instructions;
                                break;
                        }
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: eventArgs.OffsetX - Panner.x, y: eventArgs.OffsetY - Panner.y);
                    Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                    Instructions = Instructions.Select(inst => {
                        inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y);
                        if (inst is CubicBézierCurveInstruction cubicCurve)
                        {
                            cubicCurve.x1 += diff.x;
                            cubicCurve.y1 += diff.y;
                            cubicCurve.x2 += diff.x;
                            cubicCurve.y2 += diff.y;
                        }
                        return inst;
                    }).ToList();
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
