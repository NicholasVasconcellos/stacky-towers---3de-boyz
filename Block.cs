using System;
using System.Diagnostics;
using Godot;

public partial class Block : Node3D
{
    [Export]
    public float spin = 0.1f;

    [Export]
    public float expansionRate = 0.75f;

    [Export]
    public float maxSize = 1.2f;

    [Export]
    public float minSize = 0.8f;

    public override void _Process(double delta)
    {
        // Move in a direction with velocity
        // movePosition(Vector3.Forward, 1, delta);

        // Rotate
        Rotate(new Vector3(1, 0, 0), spin * (float)delta);

        // expand
        // if gets too big or too small flip
        if (Scale.Length() > maxSize || Scale.Length() < minSize)
        {
            expansionRate = -expansionRate;
        }
        expand(expansionRate, delta);
    }

    void movePosition(Vector3 moveDirection, float speed, double delta)
    {
        Position += speed * (float)delta * moveDirection;
    }

    void expand(float amount, double delta)
    {
        Scale += new Vector3(1, 1, 1) * amount * (float)delta;
    }
}
