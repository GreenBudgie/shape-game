using System;

public partial class ExplosionRadiusPreview : Node2D
{
    [Export]
    public float Radius = 300.0f; 

    private const float Thickness = 20.0f;
    private const float BaseRadius = 300.0f;
    private const float BaseDashDeg = 20.0f;
    private const float BaseGapDeg = 20.0f;
    [Export]
    public Color Color = ColorScheme.Red;

    public override void _Draw()
    {
        // Calculate fixed linear dash and base gap
        var baseDashLinear = BaseRadius * DegToRad(BaseDashDeg);
        var baseGapLinear = BaseRadius * DegToRad(BaseGapDeg);

        // Circumference in linear units
        var circum = Tau * Radius;

        // Ideal number of segments
        var idealNum = circum / (baseDashLinear + baseGapLinear);
        var num = (int)Math.Round(idealNum);
        if (num < 1) num = 1;

        // Total linear for dashes
        var totalDashLinear = num * baseDashLinear;
        var totalGapLinear = circum - totalDashLinear;
        if (totalGapLinear < 0)
        {
            num--;
            totalDashLinear = num * baseDashLinear;
            totalGapLinear = circum - totalDashLinear;
        }

        // Adjusted uniform gap linear
        var gapLinear = totalGapLinear / num;

        // Convert to radians (angles)
        var dashRad = baseDashLinear / Radius;
        var gapRad = gapLinear / Radius;

        // Draw the dashes
        var currentAngle = 0.0f;
        for (var i = 0; i < num; i++)
        {
            var startAngle = currentAngle;
            var endAngle = currentAngle + dashRad;

            // Draw the arc (body of the dash)
            DrawArc(Vector2.Zero, Radius, startAngle, endAngle, 32, Color, Thickness, true);

            // Calculate positions for the caps
            var (startAngleSin, startAngleCos) = SinCos(startAngle);
            var (endAngleSin, endAngleCos) = SinCos(endAngle);
            var startPos = new Vector2(startAngleCos, startAngleSin) * Radius;
            var endPos = new Vector2(endAngleCos, endAngleSin) * Radius;

            // Draw round caps (filled circles) at each end
            DrawCircle(startPos, Thickness / 2, Color);
            DrawCircle(endPos, Thickness / 2, Color);

            currentAngle += dashRad + gapRad;
        }
    }
}