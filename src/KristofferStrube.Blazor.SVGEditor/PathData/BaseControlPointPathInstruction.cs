using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class BaseControlPointPathInstruction : BasePathInstruction
    {
        public BaseControlPointPathInstruction(IPathInstruction PreviousInstruction)
        {
            this.PreviousInstruction = PreviousInstruction;
        }
        public List<(double x, double y)> ControlPoints { get; set; } = new();
        public (double x, double y) ReflectedPreviousInstructionsLastControlPoint
        {
            get
            {
                if (PreviousInstruction.GetType().IsSubclassOf(typeof(BaseControlPointPathInstruction)))
                {
                    var controlPointInstruction = (BaseControlPointPathInstruction)PreviousInstruction;
                    if (controlPointInstruction.ControlPoints.Count != 0)
                    {
                        return (StartPosition.x *2 - controlPointInstruction.ControlPoints[^1].x, StartPosition.y * 2 - controlPointInstruction.ControlPoints[^1].y);
                    }
                    else
                    {
                        return (StartPosition.x * 2 - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.x, StartPosition.y * 2 - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.y);
                    }
                }
                else
                {
                    return StartPosition;
                }
            }
            set
            {
                if (PreviousInstruction.GetType().IsSubclassOf(typeof(BaseControlPointPathInstruction)))
                {
                    var controlPointInstruction = (BaseControlPointPathInstruction)PreviousInstruction;
                    if (controlPointInstruction.ControlPoints.Count != 0)
                    {
                        controlPointInstruction.ControlPoints[^1] = (StartPosition.x * 2 - value.x, StartPosition.y * 2 - value.y);
                    }
                    else
                    {
                        controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = value;
                    }
                }
            }
        }
    }
}
