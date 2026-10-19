using Dreamteck.Splines;

namespace NS.Visuals.PathFollowing
{
    /// Spline with a direction you should take to traverse it properly.
    public readonly struct DirectedSpline
    {
        public readonly SplineComputer spline;
        public readonly Spline.Direction direction;
        public DirectedSpline(SplineComputer spline, Spline.Direction direction)
        {
            this.spline = spline;
            this.direction = direction;
        }
    }
}
