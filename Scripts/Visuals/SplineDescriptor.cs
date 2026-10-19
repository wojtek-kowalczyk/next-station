using System;
using Dreamteck.Splines;

namespace NS.Visuals
{
    [Serializable]
    public struct SplineDescriptor
    {
        public SplineComputer spline;
        public int fromEdge; // UGLY: in component == number (index + 1), in code == index
        public int toEdge; // UGLY: in component number (index + 1), in code == index

        public SplineDescriptor(SplineComputer spline, int fromEdge, int toEdge)
        {
            this.spline = spline;
            this.fromEdge = fromEdge;
            this.toEdge = toEdge;
        }
    }
}
