using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class EllipticalArcInstruction : BasePathInstruction
    {
        public EllipticalArcInstruction(double rx, double ry, double xAxisRotation, bool largeArcFlag, bool sweepFlag, double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
        {
            this.rx = rx;
            this.ry = ry;
            this.xAxisRotation = xAxisRotation;
            this.largeArcFlag = largeArcFlag;
            this.sweepFlag = sweepFlag;
            if (Relative)
            {
                // TODO:
                // When a relative a command is used, the end point of the arc is (cpx + x, cpy + y).
                this.x = x;
                this.y = y;
            }
            else
            {
                this.x = x;
                this.y = y;
            }
        }

        public double rx { get; set; }
        public double ry { get; set; }
        public double xAxisRotation { get; set; }
        public bool largeArcFlag { get; set; }
        public bool sweepFlag { get; set; }
        public double x { get; set; }
        public double y { get; set; }

        public (double x, double y) ControlPoint
        {
            get {
                var theta = xAxisRotation;
                if ((x + StartPosition.x) < 0)
                {
                    theta = -theta;
                }
                return (
                    (x + StartPosition.x) / 2 +
                    Math.Cos(theta / 180 * Math.PI) *
                    (rx / ry) *
                    Math.Sqrt(Math.Pow((y - StartPosition.y), 2) +
                    Math.Pow((x - StartPosition.x), 2)) / 2,

                    (y + StartPosition.y) / 2 +
                    Math.Sin(theta / 180 * Math.PI) *
                    (rx / ry) *
                    Math.Sqrt(Math.Pow((y - StartPosition.y), 2) +
                    Math.Pow((x - StartPosition.x), 2)) / 2
                );
            }
        }

        public double cx
        {
            get
            {
                return (x + StartPosition.x) / 2.0;
            }
        }

        public double cy
        {
            get
            {
                return (y + StartPosition.y) / 2.0;
            }
        }

        public double ellipseRx
        {
            get
            {
                var definedDistance = Math.Sqrt(Math.Pow(rx, 2) + Math.Pow(ry, 2));
                var actualDistance = Math.Sqrt(Math.Pow(x - StartPosition.x, 2) + Math.Pow(y - StartPosition.y, 2));
                if (actualDistance > definedDistance)
                {
                    return rx * (actualDistance / definedDistance);
                }
                return rx;
            }
        }

        public double ellipseRy
        {
            get
            {
                var definedDistance = Math.Sqrt(Math.Pow(rx, 2) + Math.Pow(ry, 2));
                var actualDistance = Math.Sqrt(Math.Pow(x - StartPosition.x, 2) + Math.Pow(y - StartPosition.y, 2));
                if (actualDistance > definedDistance)
                {
                    return ry * (actualDistance / definedDistance);
                }
                return ry;
            }
        }

        public override (double x, double y) EndPosition {
            get => (x, y);
            set { x = value.x; y = value.y; }
        }

        public override string AbsoluteInstruction => "A";

        public override string RelativeInstruction => "a";

        // TODO:
        // When a relative a command is used, the end point of the arc is (cpx + x, cpy + y).
        public override string ToString() => Relative ?
                            $"{RelativeInstruction} {rx.AsString()} {ry.AsString()} {xAxisRotation.AsString()} {(largeArcFlag ? '1' : '0')} {(sweepFlag ? '1' : '0')} {x.AsString()} {y.AsString()}" :
                            $"{AbsoluteInstruction} {rx.AsString()} {ry.AsString()} {xAxisRotation.AsString()} {(largeArcFlag ? '1' : '0')} {(sweepFlag ? '1' : '0')} {x.AsString()} {y.AsString()}";
    }
}
