using System;

public partial class ExplosionRadiusPreview : Node2D
{
    [Export]
    public float Radius = 300.0f; // Only adjustable property

    private const float Thickness = 20.0f; // Fixed thickness
    private const float BaseRadius = 300.0f;
    private const float BaseDashDeg = 20.0f;
    private const float BaseGapDeg = 20.0f;
    [Export]
    public float RotationSpeed = 0.0f; // Adjust for rotation rate (0.0f to disable)
    [Export]
    public Color Color = ColorScheme.Red; // Color of the circle

    public override void _Process(double delta)
    {
        Radius += (float)delta * 100;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var center = Vector2.Zero; // Center of the circle (adjust if needed)
        var dashRad = (BaseRadius / Radius) * DegToRad(BaseDashDeg); // Scaled to keep linear length constant
        var gapRad = (BaseRadius / Radius) * DegToRad(BaseGapDeg); // Scaled to keep linear length constant

        var startingAngle = (Radius * RotationSpeed) % Tau; // Phase for smooth rotation
        var currentAngle = startingAngle;

        while (currentAngle < startingAngle + Tau)
        {
            var startAngle = currentAngle % Tau;
            var remaining = startingAngle + Tau - currentAngle;

            if (remaining < gapRad)
            {
                break;
            }

            var dashLengthRad = Math.Min(dashRad, remaining - gapRad);

            if (dashLengthRad <= 0)
            {
                break;
            }

            var endAngle = (currentAngle + dashLengthRad) % Tau;

            // Draw the arc (body of the dash)
            if (endAngle > startAngle)
            {
                DrawArc(center, Radius, startAngle, endAngle, 32, Color, Thickness, true);
            }
            else
            {
                // Wrapped dash: draw from start to Tau, then 0 to end
                DrawArc(center, Radius, startAngle, Tau, 32, Color, Thickness, true);
                DrawArc(center, Radius, 0, endAngle, 32, Color, Thickness, true);
            }

            // Calculate positions for the caps
            var startPos = center + new Vector2(Cos(startAngle), Sin(startAngle)) * Radius;
            var endPos = center + new Vector2(Cos(endAngle), Sin(endAngle)) * Radius;

            // Draw round caps (filled circles) at each end
            DrawCircle(startPos, Thickness / 2, Color);
            DrawCircle(endPos, Thickness / 2, Color);

            currentAngle += dashLengthRad + gapRad;
        }
    }
}