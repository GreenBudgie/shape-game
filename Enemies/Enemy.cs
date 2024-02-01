using System;
using ShapeGame.Character;
using ShapeGame.Common;

namespace ShapeGame.Enemies;

public abstract partial class Enemy : ShapeCastCharacterBody2D
{

    public float Health { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Health = GetMaxHealth();
    }

    public void Kill()
    {
        QueueFree();
    }

    public void Damage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Kill();
        }
    }

    public abstract float GetMaxHealth();

}