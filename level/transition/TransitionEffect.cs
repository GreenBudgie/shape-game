using Godot;
using System;

public partial class TransitionEffect : Node2D
{

    private static readonly Texture2D Texture = GD.Load<Texture2D>("uid://bhtl546ix6wnm");

    public override void _Process(double delta)
    {
        if (Debug.IsDebugButtonJustPressed())
        {
            SpawnSquare();
        }
    }

    private void SpawnSquare()
    {
        const int amountX = 10;
        const int amountY = 5;
        for (var i = 0; i <= amountX; i++)
        {
            for (var j = 0; j <= amountY; j++)
            {
                var xStartPosition = GD.RandRange(0, amountX) * (ShapeGame.PlayableArea.End.X / amountX);
                var xMiddlePosition = i * (ShapeGame.PlayableArea.End.X / amountX);
                var xEndPosition = GD.RandRange(0, amountX) * (ShapeGame.PlayableArea.End.X / amountX);

                var yStartPosition = ShapeGame.PlayableArea.End.Y + 400;
                var yMiddlePosition = j * (ShapeGame.PlayableArea.End.Y / amountY);
                var yEndPosition = ShapeGame.PlayableArea.End.Y - 400;
                
                var scaleStart = RandomUtils.Range(0.8f, 1.0f);
                var scaleMiddle = RandomUtils.Range(0.8f, 1.0f);
                var scaleEnd = RandomUtils.Range(0.8f, 1.0f);
            
                var sprite = new Sprite2D
                {
                    Texture = Texture,
                    GlobalPosition = new Vector2(xStartPosition, yStartPosition),
                    Modulate = Colors.Transparent,
                    RotationDegrees = RandomUtils.Range(0, 360),
                    Scale = new Vector2(scaleStart, scaleStart)
                };
        
                AddChild(sprite);

                var tween = sprite.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
                tween.FadeIn(sprite, 0.25f);
                tween.Parallel().TweenPosition(sprite, new Vector2(xMiddlePosition, yMiddlePosition), 0.5f);
                tween.Parallel().TweenRotationDegrees(sprite, RandomUtils.Range(0, 360), 0.5f);
                tween.Parallel().TweenScale(sprite, scaleMiddle, 0.5f);
  
                tween.FadeOut(sprite, 0.25f).SetEase(Tween.EaseType.In);
                tween.Parallel().TweenPosition(sprite, new Vector2(xEndPosition, yEndPosition), 0.5f).SetEase(Tween.EaseType.In);
                tween.Parallel().TweenRotationDegrees(sprite, RandomUtils.Range(0, 360), 0.5f).SetEase(Tween.EaseType.In);
                tween.Parallel().TweenScale(sprite, scaleEnd, 0.5f).SetEase(Tween.EaseType.In);
            }
        }
    }
    
}
