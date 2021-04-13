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
                    switch(Instructions[(int)CurrentInstruction])
                    {
                        case AbsoluteLineInstruction or RelativeLineInstruction or AbsoluteMoveInstruction or RelativeMoveInstruction:
                            var _instructions = Instructions;
                            var inst = _instructions[(int)CurrentInstruction];
                            if (inst.NextInstruction is not null && inst.NextInstruction.IsRelative())
                            {
                                inst.NextInstruction.EndPosition = (inst.NextInstruction.EndPosition.x - (eventArgs.OffsetX - inst.EndPosition.x), inst.NextInstruction.EndPosition.y - (eventArgs.OffsetY - inst.EndPosition.y));
                            }
                            inst.EndPosition = (eventArgs.OffsetX, eventArgs.OffsetY);
                            Instructions = _instructions;
                            break;
                    }
                    break;
                case EditMode.Move:
                    var diff = (x: eventArgs.OffsetX - Panner.x, y: eventArgs.OffsetY - Panner.y);
                    Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                    Instructions = Instructions.Select(inst => { if (!inst.IsRelative()) { inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y); } return inst; }).ToList();
                    if (Instructions.Count != 0 && Instructions[0] is RelativeMoveInstruction)
                    {
                        var inst = Instructions[0];
                        inst.EndPosition = (inst.EndPosition.x + diff.x, inst.EndPosition.y + diff.y);
                        Instructions = Instructions.Skip(1).Prepend(inst).ToList();
                    }
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
