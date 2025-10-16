using System;
using Godot;

public partial class Player : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    [Export]
    public float Acceleration = 5.0f;

    //how fast character turns to face move direction
    [Export]
    public float TurnSpeed = 2.0f;

    [Export]
    public float JumpVelocity = 4.5f;

    [Export]
    public NodePath CameraPath;
    private Node3D Camera;

    public override void _Ready()
    {
        Camera = GetNode<Node3D>(CameraPath);
    }

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
            "Move Back",
            "Move Forward",
            "Move Left",
            "Move Right"
        );
        GD.Print("input:", inputVector);

        // Create direction vector and rotate by camera's Y rotation
        Vector3 moveDirection = new Vector3(inputVector.X, 0, inputVector.Y);
        moveDirection = moveDirection.Rotated(Vector3.Up, Camera.GlobalRotation.Y);
        moveDirection = moveDirection.Normalized();

        if (moveDirection != Vector3.Zero)
        {
            velocity.X = moveDirection.X * Speed;
            velocity.Z = moveDirection.Z * Speed;

            // use this to change the angle of the skin once it's been imported
            Vector2 targetAngle = new Vector2(inputVector.X, inputVector.Y);
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
