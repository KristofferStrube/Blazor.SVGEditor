using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class ClosePathInstruction : BasePathInstruction
    {
        public ClosePathInstruction(IPathInstruction PreviousInstruction, bool Relative)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
        }

        public override (double x, double y) EndPosition
        {
            get { return (0, 0); }
            set { }
        }

        public override string AbsoluteInstruction => "Z";

        public override string RelativeInstruction => "z";

        public override string ToString() => Relative ? RelativeInstruction : AbsoluteInstruction;
    }
}
