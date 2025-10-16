using System;
using Godot;

public partial class Player : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    [Export]
    public float JumpVelocity = 4.5f;

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            // Decrement Velocity by a * dT
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Get 2d Input Vector
        Vector2 inputVector = Input.GetVector(
            "Move Left",
            "Move Right",
            "Move Forward",
            "Move Back"
        );

        // Transform.Basis rotates the input to match player rotation.
        Vector3 moveDirection = (
            Transform.Basis * new Vector3(inputVector.X, 0, inputVector.Y)
        ).Normalized();

        if (moveDirection != Vector3.Zero)
        {
            velocity.X = moveDirection.X * Speed;
            velocity.Z = moveDirection.Z * Speed;
        }
        else
        {
            // Smooth Stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
