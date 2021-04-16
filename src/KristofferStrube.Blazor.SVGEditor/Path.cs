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
                catch(Exception e)
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
                    var _instructions = Instructions;
                    var inst = _instructions[(int)CurrentInstruction];
                    switch (Instructions[(int)CurrentInstruction])
                    {
                        case LineInstruction or MoveInstruction:
                            inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                            Instructions = _instructions;
                            break;
                        case HorizontalLineInstruction:
                            inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                            if (inst.PreviousInstruction is not null)
                            {
                                inst.PreviousInstruction.EndPosition = (inst.PreviousInstruction.EndPosition.x, inst.PreviousInstruction.EndPosition.y + (eventArgs.OffsetY - inst.EndPosition.y));
                            }
                            Instructions = _instructions;
                            break;
                        case VerticalLineInstruction:
                            inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                            if (inst.PreviousInstruction is not null)
                            {
                                inst.PreviousInstruction.EndPosition = (inst.PreviousInstruction.EndPosition.x + (eventArgs.OffsetX - inst.EndPosition.x), inst.PreviousInstruction.EndPosition.y);
                            }
                            Instructions = _instructions;
                            break;
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: eventArgs.OffsetX - Panner.x, y: eventArgs.OffsetY - Panner.y);
                    Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                    Instructions = Instructions.Select(inst => { inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y); return inst; }).ToList();
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
