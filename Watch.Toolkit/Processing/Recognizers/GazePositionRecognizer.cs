// -*- mode: csharp; c-basic-offset: 4; indent-tabs-mode: nil -*-
using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Input.Gaze
{
    class Vec2D
    {
        public readonly double X, Y;

        public Vec2D(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    interface IShape
    {
        bool Contains(double x, double y);
    }

    class Rectangle: IShape
    {
        public readonly Vec2D UpperLeft, LowerRight;

        public Rectangle(Vec2D upperLeft, Vec2D lowerRight)
        {
            UpperLeft = upperLeft;
            LowerRight = lowerRight;
        }

        public bool Contains(double x, double y)
        {
            return UpperLeft.X <= x && x <= LowerRight.X && UpperLeft.Y <= y && y <= LowerRight.Y;
        }
    }

    class ConvexPolygon : IShape
    {
        readonly IShape Bounds;
        readonly Vec2D[] Vertices;

        public ConvexPolygon(Vec2D[] vertices)
        {
            Vertices = vertices;
            Bounds = new Rectangle(
                new Vec2D(
                    Vertices.Select(v => v.X).Min(),
                    Vertices.Select(v => v.Y).Min()),
                new Vec2D(
                    Vertices.Select(v => v.X).Max(),
                    Vertices.Select(v => v.Y).Max()));
        }

        public bool Contains(double x, double y)
        {
            return Bounds.Contains(x, y) && IsInsidePoly(x, y);
        }

        /// <summary>
        /// Determines whether the point at x and y is is inside this instance.
        /// Algorithm by http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
        /// </summary>
        /// <returns><c>true</c> if the point at x and y is inside this polygon; <c>false</c>, otherwise.</returns>
        /// <param name="x">The x coordinate of the point to test.</param>
        /// <param name="y">The y coordinate of the point to test.</param>
        bool IsInsidePoly(double x, double y)
        {
            bool inside = false;
            for (int i = 0, j = Vertices.Length - 1; i < Vertices.Length; j = i++)
            {
                if (Vertices[i].Y > y != Vertices[j].Y > y
                    && x < (Vertices[j].X - Vertices[i].X) * (y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y)
                    + Vertices[i].X)
                    inside = !inside;
            }
            return inside;
        }
    }

    public enum GazePosition
    {
        Left,
        Right,
        Above,
        Below,
        Center,
        Unknown
    }

    public interface GazePattern
    {
        GazePosition ComputeGazePosition(GazeFrame frame);
    }

    public class MalteserCross : GazePattern
    {
        const int MARGIN = 50; // pixels
        readonly Dictionary<GazePosition, IShape> Patterns;

        public MalteserCross(int areaWidth, int areaHeight, int screenWidth, int screenHeight)
        {
            var c = new Rectangle(
                new Vec2D(areaWidth / 2 - screenWidth / 2, areaHeight / 2 - screenHeight / 2),
                new Vec2D(areaWidth / 2 + screenWidth / 2, areaHeight / 2 + screenHeight / 2));
            Patterns = new Dictionary<GazePosition, IShape>();
            Patterns.Add(GazePosition.Center, c);
            Patterns.Add(GazePosition.Above, new ConvexPolygon(new []{
                        new Vec2D(MARGIN, 0),
                        new Vec2D(areaWidth - MARGIN, 0),
                        new Vec2D(c.LowerRight.X - MARGIN, c.UpperLeft.Y),
                        new Vec2D(c.UpperLeft.X + MARGIN, c.UpperLeft.Y)
                    }));
            Patterns.Add(GazePosition.Below, new ConvexPolygon(new []{
                        new Vec2D(MARGIN, areaHeight),
                        new Vec2D(c.UpperLeft.X + MARGIN, c.LowerRight.Y),
                        new Vec2D(c.LowerRight.X - MARGIN, c.LowerRight.Y),
                        new Vec2D(areaWidth - MARGIN, areaHeight)
                    }));
            Patterns.Add(GazePosition.Right, new ConvexPolygon(new []{
                        new Vec2D(c.LowerRight.X, c.UpperLeft.Y + MARGIN),
                        new Vec2D(areaWidth, MARGIN),
                        new Vec2D(areaWidth, areaHeight - MARGIN),
                        new Vec2D(c.LowerRight.X, c.LowerRight.Y - MARGIN)
                    }));
            Patterns.Add(GazePosition.Left, new ConvexPolygon(new []{
                        new Vec2D(0, MARGIN),
                        new Vec2D(c.UpperLeft.X, c.UpperLeft.Y + MARGIN),
                        new Vec2D(c.UpperLeft.X, c.LowerRight.Y - MARGIN),
                        new Vec2D(0, areaHeight - MARGIN)
                    }));
        }

        /// <summary>
        ///   Compute the gaze position in the Malteser cross pattern.
        /// </summary>
        public GazePosition ComputeGazePosition(GazeFrame frame)
        {
            foreach(GazePosition pos in Enum.GetValues(typeof(GazePosition)))
            {
                if (pos != GazePosition.Unknown && Patterns[pos].Contains(frame.X, frame.Y))
                    return pos;
            }
            return GazePosition.Unknown;
        }
    }
}
