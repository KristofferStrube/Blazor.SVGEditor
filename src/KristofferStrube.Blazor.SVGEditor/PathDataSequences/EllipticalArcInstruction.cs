using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class EllipticalArcInstruction : BasePathInstruction
{
    public EllipticalArcInstruction(double rx, double ry, double xAxisRotation, bool largeArcFlag, bool sweepFlag, double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        this.Rx = rx;
        this.Ry = ry;
        this.XAxisRotation = xAxisRotation;
        this.LargeArcFlag = largeArcFlag;
        this.SweepFlag = sweepFlag;
        if (Relative)
        {
            this.X = StartPosition.x + x;
            this.Y = StartPosition.y + y;
        }
        else
        {
            this.X = x;
            this.Y = y;
        }
    }

    public double Rx { get; set; }
    public double Ry { get; set; }
    public double XAxisRotation { get; set; }
    public bool LargeArcFlag { get; set; }
    public bool SweepFlag { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public (double x, double y) ControlPointYPos
    {
        get => (Center.x + EllipseRadi.Ry * Math.Sin(XAxisRotation / 180 * Math.PI), Center.y - EllipseRadi.Ry * Math.Cos(XAxisRotation / 180 * Math.PI));
        set
        {
            double radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
            EllipseRadi = (EllipseRadi.Rx, radius);
            XAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI;
        }
    }
    public (double x, double y) ControlPointYNeg
    {
        get => (Center.x - EllipseRadi.Ry * Math.Sin(XAxisRotation / 180 * Math.PI), Center.y + EllipseRadi.Ry * Math.Cos(XAxisRotation / 180 * Math.PI));
        set
        {
            double radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
            EllipseRadi = (EllipseRadi.Rx, radius);
            XAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI + 180;
        }
    }
    public (double x, double y) ControlPointXPos
    {
        get => (Center.x + EllipseRadi.Rx * Math.Cos(XAxisRotation / 180 * Math.PI), Center.y + EllipseRadi.Rx * Math.Sin(XAxisRotation / 180 * Math.PI));
        set
        {
            double radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
            EllipseRadi = (radius, EllipseRadi.Ry);
            XAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI + 90;
        }
    }
    public (double x, double y) ControlPointXNeg
    {
        get => (Center.x - EllipseRadi.Rx * Math.Cos(XAxisRotation / 180 * Math.PI), Center.y - EllipseRadi.Rx * Math.Sin(XAxisRotation / 180 * Math.PI));
        set
        {
            double radius = Math.Sqrt(Math.Pow(value.x - Center.x, 2) + Math.Pow(value.y - Center.y, 2));
            EllipseRadi = (radius, EllipseRadi.Ry);
            XAxisRotation = -Math.Atan((value.x - Center.x) / (value.y - Center.y)) * 180.0 / Math.PI - 90;
        }
    }

    private double X1Mark => (X - StartPosition.x) / 2 * Math.Cos(XAxisRotation / 180.0 * Math.PI) + (Y - StartPosition.y) / 2 * Math.Sin(XAxisRotation / 180.0 * Math.PI);
    private double Y1Mark => (X - StartPosition.x) / 2 * -Math.Sin(XAxisRotation / 180.0 * Math.PI) + (Y - StartPosition.y) / 2 * Math.Cos(XAxisRotation / 180.0 * Math.PI);

    // These use the implementation from https://svgwg.org/svg2-draft/implnote.html#ArcConversionEndpointToCenter
    public (double x, double y) Center
    {
        get
        {
            int sign = LargeArcFlag == SweepFlag ? 1 : -1;

            double rxPow = Math.Pow(EllipseRadi.Rx, 2);
            double ryPow = Math.Pow(EllipseRadi.Ry, 2);
            double x1MarkPow = Math.Pow(X1Mark, 2);
            double y1MarkPow = Math.Pow(Y1Mark, 2);

            double sqrtPart;
            if (Delta <= 1)
            {
                sqrtPart = sign * Math.Sqrt((rxPow * ryPow - rxPow * y1MarkPow - ryPow * x1MarkPow) / (rxPow * y1MarkPow + ryPow * x1MarkPow));
            }
            else
            {
                sqrtPart = 0;
            }

            double cxMark = sqrtPart * (EllipseRadi.Rx * Y1Mark / EllipseRadi.Ry);
            double cyMark = sqrtPart * (-(EllipseRadi.Ry * X1Mark) / EllipseRadi.Rx);

            double cx = Math.Cos(XAxisRotation / 180.0 * Math.PI) * cxMark - Math.Sin(XAxisRotation / 180.0 * Math.PI) * cyMark + (StartPosition.x + X) / 2;
            double cy = Math.Sin(XAxisRotation / 180.0 * Math.PI) * cxMark + Math.Cos(XAxisRotation / 180.0 * Math.PI) * cyMark + (StartPosition.y + Y) / 2;

            return (cx, cy);
        }
    }

    private double Delta => Math.Pow(X1Mark, 2) / Math.Pow(Math.Abs(Rx), 2) + Math.Pow(Y1Mark, 2) / Math.Pow(Math.Abs(Ry), 2);

    public (double Rx, double Ry) EllipseRadi
    {
        get
        {
            if (Delta <= 1)
            {
                return (Math.Abs(Rx), Math.Abs(Ry));
            }
            return (Math.Sqrt(Delta) * Math.Abs(Rx), Math.Sqrt(Delta) * Math.Abs(Ry));
        }
        set
        {
            if (Delta <= 1)
            {
                Rx = value.Rx;
                Ry = value.Ry;
            }
            else
            {
                Rx = value.Rx / Math.Sqrt(Delta);
                Ry = value.Ry / Math.Sqrt(Delta);
            }
        }
    }

    public override (double x, double y) EndPosition
    {
        get => (X, Y);
        set { X = value.x; Y = value.y; }
    }

    public override string AbsoluteInstruction => "A";

    public override string RelativeInstruction => "a";

    public override string ToString()
    {
        return Relative ?
$"{RelativeInstruction} {Rx.AsString()} {Ry.AsString()} {XAxisRotation.AsString()} {(LargeArcFlag ? '1' : '0')} {(SweepFlag ? '1' : '0')} {(X - StartPosition.x).AsString()} {(Y - StartPosition.y).AsString()}" :
$"{AbsoluteInstruction} {Rx.AsString()} {Ry.AsString()} {XAxisRotation.AsString()} {(LargeArcFlag ? '1' : '0')} {(SweepFlag ? '1' : '0')} {X.AsString()} {Y.AsString()}";
    }

    public override void SnapToInteger()
    {
        base.SnapToInteger();
        Rx = (int)Rx;
        Ry = (int)Ry;
        XAxisRotation = (int)XAxisRotation;
    }
}
