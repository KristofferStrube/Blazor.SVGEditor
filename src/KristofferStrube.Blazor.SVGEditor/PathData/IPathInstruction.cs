using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public interface IPathInstruction
    {
        public IPathInstruction PreviousInstruction { get; set; }
        public IPathInstruction NextInstruction { get; set; }
        public bool ExplicitSymbol { get; set; }
        public (double x, double y) StartPosition { get; }
        public (double x, double y) EndPosition { get; set; }
        public string AbsoluteInstruction { get; }
        public string RelativeInstruction { get; }
        public string ToString();
        public bool Relative { get; set; }
    }
}
