using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class BasePathInstruction : IPathInstruction
    {
        public BasePathInstruction (bool Relative, IPathInstruction PreviousInstruction)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
        }
        public IPathInstruction PreviousInstruction { get; set; }
        public IPathInstruction NextInstruction { get; set; }
        public bool ExplicitSymbol { get; set; }
        public (double x, double y) StartPosition => PreviousInstruction is not null ? PreviousInstruction.EndPosition : (0, 0);
        public abstract (double x, double y) EndPosition { get; set; }
        public abstract string AbsoluteInstruction { get; }
        public abstract string RelativeInstruction { get; }
        public abstract override string ToString();
        public bool Relative { get; set; }
    }
}
