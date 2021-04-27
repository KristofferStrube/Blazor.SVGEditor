using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class BaseControlPointPathInstruction : BasePathInstruction
    {
        public List<(double x, double y)> ControlPoints { get; set; } = new();
    }
}
