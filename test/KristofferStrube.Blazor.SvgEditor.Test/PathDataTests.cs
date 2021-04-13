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
        public void EmptyData()
        {
            var input = "";
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void AbsoluteLineTest()
        {
            var input = "L 20 10 40 10";
            Assert.AreEqual(20, PathData.Parse(input)[0].EndPosition.x);
            Assert.AreEqual(10, PathData.Parse(input)[0].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RelativeLineTest()
        {
            var input = "l 50.1 21.0001 -10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[0].EndPosition.x);
            Assert.AreEqual(21.0001, PathData.Parse(input)[0].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void CompressedRelativeLineTest()
        {
            var input = "l50.1-21.0001-10 20";
            Assert.AreEqual(50.1, PathData.Parse(input)[0].EndPosition.x);
            Assert.AreEqual(-21.0001, PathData.Parse(input)[0].EndPosition.y);
            Assert.AreEqual("l 50.1 -21.0001 -10 20", PathData.Parse(input).AsString());
        }

        [Test]
        public void MultipleCommandsTest()
        {
            var input = "l 1 1 L 2 2 l 1 2";
            Assert.AreEqual(3, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual(input, PathData.Parse(input).AsString());
        }

        [Test]
        public void RemovesExcessSpacesTest()
        {
            var input = " l 1  1   L   2 2   l   1   2   ";
            Assert.AreEqual(3, PathData.Parse(input)[2].EndPosition.x);
            Assert.AreEqual(4, PathData.Parse(input)[2].EndPosition.y);
            Assert.AreEqual("l 1 1 L 2 2 l 1 2", PathData.Parse(input).AsString());
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
    }
}