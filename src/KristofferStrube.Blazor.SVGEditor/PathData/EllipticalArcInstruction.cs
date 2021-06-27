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
                this.x = StartPosition.x + x;
                this.y = StartPosition.y + y;
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

        public (double x, double y) ControlPointYPos
        {
            get => (Center.x + EllipseRadi.Ry * Math.Sin(xAxisRotation / 180 * Math.PI), Center.y - EllipseRadi.Ry * Math.Cos(xAxisRotation / 180 * Math.PI));
            set
            {
                var radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
                EllipseRadi = (EllipseRadi.Rx, radius);
                xAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI;
            }
        }
        public (double x, double y) ControlPointYNeg
        {
            get => (Center.x - EllipseRadi.Ry * Math.Sin(xAxisRotation / 180 * Math.PI), Center.y + EllipseRadi.Ry * Math.Cos(xAxisRotation / 180 * Math.PI));
            set
            {
                var radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
                EllipseRadi = (EllipseRadi.Rx, radius);
                xAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI+180;
            }
        }
        public (double x, double y) ControlPointXPos
        {
            get => (Center.x + EllipseRadi.Rx * Math.Cos(xAxisRotation / 180 * Math.PI), Center.y + EllipseRadi.Rx * Math.Sin(xAxisRotation / 180 * Math.PI));
            set
            {
                var radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
                EllipseRadi = (radius, EllipseRadi.Ry);
                xAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI+90;
            }
        }
        public (double x, double y) ControlPointXNeg
        {
            get => (Center.x - EllipseRadi.Rx * Math.Cos(xAxisRotation / 180 * Math.PI), Center.y - EllipseRadi.Rx * Math.Sin(xAxisRotation / 180 * Math.PI));
            set
            {
                var radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
                EllipseRadi = (radius, EllipseRadi.Ry);
                xAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI-90;
            }
        }

        private double x1Mark => (x - StartPosition.x) / 2 * Math.Cos(xAxisRotation / 180.0 * Math.PI) + (y - StartPosition.y) / 2 * Math.Sin(xAxisRotation / 180.0 * Math.PI);
        private double y1Mark => (x - StartPosition.x) / 2 * -Math.Sin(xAxisRotation / 180.0 * Math.PI) + (y - StartPosition.y) / 2 * Math.Cos(xAxisRotation / 180.0 * Math.PI);

        // These use the implementation from https://svgwg.org/svg2-draft/implnote.html#ArcConversionEndpointToCenter
        public (double x, double y) Center
        {
            get
            {
                var sign = (largeArcFlag == sweepFlag ? 1 : -1);

                var rxPow = Math.Pow(EllipseRadi.Rx, 2);
                var ryPow = Math.Pow(EllipseRadi.Ry, 2);
                var x1MarkPow = Math.Pow(x1Mark, 2);
                var y1MarkPow = Math.Pow(y1Mark, 2);

                double sqrtPart;
                if (Delta <= 1)
                {
                    sqrtPart = sign * Math.Sqrt((rxPow * ryPow - rxPow * y1MarkPow - ryPow * x1MarkPow) / (rxPow * y1MarkPow + ryPow * x1MarkPow));
                }
                else
                {
                    sqrtPart = 0;
                }

                var cxMark = sqrtPart * ((EllipseRadi.Rx * y1Mark) / EllipseRadi.Ry);
                var cyMark = sqrtPart * (-(EllipseRadi.Ry * x1Mark) / EllipseRadi.Rx);

                var cx = Math.Cos(xAxisRotation / 180.0 * Math.PI) * cxMark - Math.Sin(xAxisRotation / 180.0 * Math.PI) * cyMark + (StartPosition.x + x) / 2;
                var cy = Math.Sin(xAxisRotation / 180.0 * Math.PI) * cxMark + Math.Cos(xAxisRotation / 180.0 * Math.PI) * cyMark + (StartPosition.y + y) / 2;

                return (cx, cy);
            }
        }

        private double Delta => Math.Pow(x1Mark, 2) / Math.Pow(Math.Abs(rx), 2) + Math.Pow(y1Mark, 2) / Math.Pow(Math.Abs(ry), 2);

        public (double Rx, double Ry) EllipseRadi
        {
            get
            {
                if (Delta <= 1)
                {
                    return (Math.Abs(rx), Math.Abs(ry));
                }
                return (Math.Sqrt(Delta) * Math.Abs(rx), Math.Sqrt(Delta) * Math.Abs(ry));
            }
            set
            {
                if (Delta <= 1)
                {
                    rx = value.Rx;
                    ry = value.Ry;
                }
                else
                {
                    rx = value.Rx / Math.Sqrt(Delta);
                    ry = value.Ry / Math.Sqrt(Delta);
                }
            }
        }

        public override (double x, double y) EndPosition
        {
            get => (x, y);
            set { x = value.x; y = value.y; }
        }

        public override string AbsoluteInstruction => "A";

        public override string RelativeInstruction => "a";

        public override string ToString() => Relative ?
                            $"{RelativeInstruction} {rx.AsString()} {ry.AsString()} {xAxisRotation.AsString()} {(largeArcFlag ? '1' : '0')} {(sweepFlag ? '1' : '0')} {(x - StartPosition.x).AsString()} {(y - StartPosition.y).AsString()}" :
                            $"{AbsoluteInstruction} {rx.AsString()} {ry.AsString()} {xAxisRotation.AsString()} {(largeArcFlag ? '1' : '0')} {(sweepFlag ? '1' : '0')} {x.AsString()} {y.AsString()}";
    }
}
