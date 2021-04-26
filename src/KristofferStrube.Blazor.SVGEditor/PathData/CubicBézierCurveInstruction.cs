using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class CubicBézierCurveInstruction : BasePathInstruction
    {
        public CubicBézierCurveInstruction(double x1, double y1, double x2, double y2, double x, double y, IPathInstruction PreviousInstruction, bool Relative)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
            if (Relative)
            {
                this.x1 = StartPosition.x + x1;
                this.y1 = StartPosition.y + y1;
                this.x2 = StartPosition.x + x2;
                this.y2 = StartPosition.y + y2;
                this.x = StartPosition.x + x;
                this.y = StartPosition.y + y;
            }
            else
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                this.x = x;
                this.y = y;
            }
        }

        public double x1 { get; set; }

        public double y1 { get; set; }

        public double x2 { get; set; }

        public double y2 { get; set; }

        private double x { get; set; }

        private double y { get; set; }

        public override (double x, double y) EndPosition
        {
            get { return (x, y); }
            set { x = value.x; y = value.y; }
        }

        public override string AbsoluteInstruction => "C";

        public override string RelativeInstruction => "c";

        public override string ToString() => (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? $"{(x1 - StartPosition.x).AsString()} {(y1 - StartPosition.y).AsString()} {(x2 - StartPosition.x).AsString()} {(y2 - StartPosition.y).AsString()} {(x - StartPosition.x).AsString()} {(y - StartPosition.y).AsString()}" : $"{x1.AsString()} {y1.AsString()} {x2.AsString()} {y2.AsString()} {x.AsString()} {y.AsString()}");
    }
}
