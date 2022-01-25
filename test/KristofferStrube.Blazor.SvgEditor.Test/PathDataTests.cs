using KristofferStrube.Blazor.SVGEditor.PathDataSequences;
using NUnit.Framework;

namespace KristofferStrube.Blazor.SvgEditor.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FirstSequenceNeedsToBeMove()
        {
            string input = "m 10 10";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
            input = "M 10 10";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
            input = "L 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void EmptyData()
        {
            string input = "";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteLineTest()
        {
            string input = "M 0 0 L 20 10 40 10";
            Assert.AreEqual(40, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeLineTest()
        {
            string input = "M 0 0 l 50.1 21.1 -10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[1].EndPosition.x);
            Assert.AreEqual(21.1, PathData.Parse(input)[1].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void CompressedRelativeLineTest()
        {
            string input = "M0-0l50.1-21.0001-10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[1].EndPosition.x);
            Assert.AreEqual(-21.0001, PathData.Parse(input)[1].EndPosition.y);
            Assert.AreEqual("M 0 -0 l 50.1 -21.0001 -10 20", PathData.Parse(input).AsString());
        }

        [Test]
        public void MultipleCommandsTest()
        {
            string input = "M 0 0 l 1 1 L 2 2 l 1 2";
            Assert.AreEqual(3, PathData.Parse(input)[3].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[3].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RemovesExcessSpacesTest()
        {
            string input = "M       0 0 l 1  1   L   2 2   l   1   2   ";
            Assert.AreEqual(3, PathData.Parse(input)[3].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[3].EndPosition.y);
            Assert.AreEqual("M 0 0 l 1 1 L 2 2 l 1 2", PathData.Parse(input).AsString());
        }

        [Test]
        public void MoveRelativeLineTest()
        {
            string input = "M 10 10 l 10 0 0 30";
            Assert.AreEqual(20, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(40, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void MoveAbsoluteLine()
        {
            string input = "M 10 10 L 10 0 0 30";
            Assert.AreEqual(10, PathData.Parse(input)[1].StartPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[1].StartPosition.y);
            Assert.AreEqual(10, PathData.Parse(input)[2].StartPosition.x);
            Assert.AreEqual(0, PathData.Parse(input)[2].StartPosition.y);
            Assert.AreEqual(0, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(30, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void ShouldThrowWhenWrongNumberOfArgumentsForMove()
        {
            string input = "M 10 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void ShouldThrowWhenWrongNumberOfArgumentsForLine()
        {
            string input = "L 10 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void RelativeHorizontalLineAfterLine()
        {
            string input = "M 0 0 L 10 10 h 11";
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteHorizontalLine()
        {
            string input = "M 10 10 L 10 10 H 21";
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeVerticalLineAfterLine()
        {
            string input = "M 0 0 L 10 10 v 11";
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteVerticalLine()
        {
            string input = "M 10 10 L 10 10 V 21";
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeCubicBézierCurve()
        {
            string input = "M 10 10 c 10 0 0 10 10 10";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(CubicBézierCurveInstruction), inst);
            CubicBézierCurveInstruction cubic = (CubicBézierCurveInstruction)inst;
            Assert.AreEqual((20, 10), cubic.ControlPoints[0]);
            Assert.AreEqual((10, 20), cubic.ControlPoints[1]);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteCubicBézierCurve()
        {
            string input = "M 10 10 C 20 10 10 20 20 20";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(CubicBézierCurveInstruction), inst);
            CubicBézierCurveInstruction cubic = (CubicBézierCurveInstruction)inst;
            Assert.AreEqual((20, 10), cubic.ControlPoints[0]);
            Assert.AreEqual((10, 20), cubic.ControlPoints[1]);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeShorthandCubicBézierCurve()
        {
            string input = "M 10 10 c 10 0 0 10 10 10 s 10 10 20 20";
            IPathInstruction inst = PathData.Parse(input)[2];
            Assert.AreEqual(40, inst.EndPosition.x);
            Assert.AreEqual(40, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(ShorthandCubicBézierCurveInstruction), inst);
            ShorthandCubicBézierCurveInstruction cubic = (ShorthandCubicBézierCurveInstruction)inst;
            Assert.AreEqual((30, 30), cubic.ControlPoints[0]);
            Assert.AreEqual((30, 20), cubic.ReflectedPreviousInstructionsLastControlPoint);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteShorthandCubicBézierCurve()
        {
            string input = "M 10 10 C 20 10 10 20 20 20 S 30 30 40 40";
            IPathInstruction inst = PathData.Parse(input)[2];
            Assert.AreEqual(40, inst.EndPosition.x);
            Assert.AreEqual(40, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(ShorthandCubicBézierCurveInstruction), inst);
            ShorthandCubicBézierCurveInstruction cubic = (ShorthandCubicBézierCurveInstruction)inst;
            Assert.AreEqual((30, 30), cubic.ControlPoints[0]);
            Assert.AreEqual((30, 20), cubic.ReflectedPreviousInstructionsLastControlPoint);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeQuadraticBézierCurve()
        {
            string input = "M 10 10 q 10 0 0 10";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(10, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(QuadraticBézierCurveInstruction), inst);
            QuadraticBézierCurveInstruction quadratic = (QuadraticBézierCurveInstruction)inst;
            Assert.AreEqual((20, 10), quadratic.ControlPoints[0]);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteQuadraticBézierCurve()
        {
            string input = "M 10 10 Q 20 10 10 20";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(10, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(QuadraticBézierCurveInstruction), inst);
            QuadraticBézierCurveInstruction quadratic = (QuadraticBézierCurveInstruction)inst;
            Assert.AreEqual((20, 10), quadratic.ControlPoints[0]);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeShorthandQuadraticBézierCurve()
        {
            string input = "M 10 10 q 10 0 0 10 t 10 10";
            IPathInstruction inst = PathData.Parse(input)[2];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(30, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(ShorthandQuadraticBézierCurveInstruction), inst);
            ShorthandQuadraticBézierCurveInstruction quadratic = (ShorthandQuadraticBézierCurveInstruction)inst;
            Assert.AreEqual((0, 30), quadratic.ReflectedPreviousInstructionsLastControlPoint);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteShorthandQuadraticBézierCurve()
        {
            string input = "M 10 10 q 10 0 0 10 T 20 30";
            IPathInstruction inst = PathData.Parse(input)[2];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(30, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(ShorthandQuadraticBézierCurveInstruction), inst);
            ShorthandQuadraticBézierCurveInstruction quadratic = (ShorthandQuadraticBézierCurveInstruction)inst;
            Assert.AreEqual((0, 30), quadratic.ReflectedPreviousInstructionsLastControlPoint);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteEllipticalArcCurve()
        {
            string input = "M 10 10 A 1 1 0 1 1 20 30";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(30, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(EllipticalArcInstruction), inst);
            EllipticalArcInstruction ellipticalArc = (EllipticalArcInstruction)inst;
            Assert.AreEqual(1, ellipticalArc.Rx);
            Assert.AreEqual(1, ellipticalArc.Ry);
            Assert.AreEqual((15, 20 - Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2))), ellipticalArc.ControlPointYPos);
            Assert.AreEqual((15, 20 + Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2))), ellipticalArc.ControlPointYNeg);
            Assert.AreEqual((15 - Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2)), 20), ellipticalArc.ControlPointXNeg);
            Assert.AreEqual((15 + Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2)), 20), ellipticalArc.ControlPointXPos);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeEllipticalArcCurve()
        {
            string input = "M 10 10 a 1 1 0 1 1 10 20";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(30, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(EllipticalArcInstruction), inst);
            EllipticalArcInstruction ellipticalArc = (EllipticalArcInstruction)inst;
            Assert.AreEqual(1, ellipticalArc.Rx);
            Assert.AreEqual(1, ellipticalArc.Ry);
            Assert.AreEqual((15, 20 - Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2))), ellipticalArc.ControlPointYPos);
            Assert.AreEqual((15, 20 + Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2))), ellipticalArc.ControlPointYNeg);
            Assert.AreEqual((15 - Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2)), 20), ellipticalArc.ControlPointXNeg);
            Assert.AreEqual((15 + Math.Sqrt(Math.Pow(5, 2) + Math.Pow(10, 2)), 20), ellipticalArc.ControlPointXPos);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void ImplicitZeroNumberInterpretation()
        {
            string input = "M 0 0 l.457.318";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(0.457, inst.EndPosition.x);
            Assert.AreEqual(0.318, inst.EndPosition.y);
            Assert.AreEqual("l 0.457 0.318", inst.ToString());
        }

        [Test]
        public void ImplicitNegativeStartZeroNumberInterpretation()
        {
            string input = "M 0 0 l-.004.007";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(-0.004, inst.EndPosition.x);
            Assert.AreEqual(0.007, inst.EndPosition.y);
            Assert.AreEqual("l -0.004 0.007", inst.ToString());
        }

        [Test]
        public void ImplicitNegativeMiddleZeroNumberInterpretation()
        {
            string input = "M 0 0 l.004-.007";
            IPathInstruction inst = PathData.Parse(input)[1];
            Assert.AreEqual(0.004, inst.EndPosition.x);
            Assert.AreEqual(-0.007, inst.EndPosition.y);
            Assert.AreEqual("l 0.004 -0.007", inst.ToString());
        }
    }
}