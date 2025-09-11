using Godot;
using System;
using System.Globalization;

public partial class FpsCounter : Label
{

    public override void _Process(double delta)
    {
        Text = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
    }

}
