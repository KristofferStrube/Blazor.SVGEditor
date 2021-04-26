using NUnit.Framework;
using KristofferStrube.Blazor.SVGEditor;
using System.Collections.Generic;
using System;

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
            var input = "m 10 10";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
            input = "M 10 10";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
            input = "L 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void EmptyData()
        {
            var input = "";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteLineTest()
        {
            var input = "M 0 0 L 20 10 40 10";
            Assert.AreEqual(40, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeLineTest()
        {
            var input = "M 0 0 l 50.1 21.1 -10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[1].EndPosition.x);
            Assert.AreEqual(21.1, PathData.Parse(input)[1].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void CompressedRelativeLineTest()
        {
            var input = "M0-0l50.1-21.0001-10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[1].EndPosition.x);
            Assert.AreEqual(-21.0001, PathData.Parse(input)[1].EndPosition.y);
            Assert.AreEqual("M 0 -0 l 50.1 -21.0001 -10 20", PathData.Parse(input).AsString());
        }

        [Test]
        public void MultipleCommandsTest()
        {
            var input = "M 0 0 l 1 1 L 2 2 l 1 2";
            Assert.AreEqual(3, PathData.Parse(input)[3].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[3].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RemovesExcessSpacesTest()
        {
            var input = "M       0 0 l 1  1   L   2 2   l   1   2   ";
            Assert.AreEqual(3, PathData.Parse(input)[3].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[3].EndPosition.y);
            Assert.AreEqual("M 0 0 l 1 1 L 2 2 l 1 2", PathData.Parse(input).AsString());
        }

        [Test]
        public void MoveRelativeLineTest()
        {
            var input = "M 10 10 l 10 0 0 30";
            Assert.AreEqual(20, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(40, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void MoveAbsoluteLine()
        {
            var input = "M 10 10 L 10 0 0 30";
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
            var input = "M 10 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void ShouldThrowWhenWrongNumberOfArgumentsForLine()
        {
            var input = "L 10 10 10";
            Assert.Throws<ArgumentException>(() => PathData.Parse(input));
        }

        [Test]
        public void RelativeHorizontalLineAfterLine()
        {
            var input = "M 0 0 L 10 10 h 11";
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteHorizontalLine()
        {
            var input = "M 10 10 L 10 10 H 21";
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeVerticalLineAfterLine()
        {
            var input = "M 0 0 L 10 10 v 11";
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteVerticalLine()
        {
            var input = "M 10 10 L 10 10 V 21";
            Assert.AreEqual(10, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(21, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeCubicBézierCurve()
        {
            var input = "M 10 10 c 10 0 0 10 10 10";
            var inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(CubicBézierCurveInstruction), inst);
            var cubic = (CubicBézierCurveInstruction)inst;
            Assert.AreEqual(20, cubic.x1);
            Assert.AreEqual(10, cubic.y1);
            Assert.AreEqual(10, cubic.x2);
            Assert.AreEqual(20, cubic.y2);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteCubicBézierCurve()
        {
            var input = "M 10 10 C 20 10 10 20 20 20";
            var inst = PathData.Parse(input)[1];
            Assert.AreEqual(20, inst.EndPosition.x);
            Assert.AreEqual(20, inst.EndPosition.y);
            Assert.IsInstanceOf(typeof(CubicBézierCurveInstruction), inst);
            var cubic = (CubicBézierCurveInstruction)inst;
            Assert.AreEqual(20, cubic.x1);
            Assert.AreEqual(10, cubic.y1);
            Assert.AreEqual(10, cubic.x2);
            Assert.AreEqual(20, cubic.y2);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }
    }
}