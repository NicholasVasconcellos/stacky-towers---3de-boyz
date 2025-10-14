using System;
using Godot;

public partial class CameraController : Node3D
{
    private float hRotation = 0f;
    private float vRotation = 0f;

    [Export]
    public float sensitivity = 0.3f;

    [Export]
    public float maxPitch = 60f;

    [Export]
    public float minPitch = -30f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    // Called on Every Input
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseMotion motion)
        {
            // Get the mosue offset
            Vector2 relativeMotion = motion.Relative;

            // update the Horizontal rotation
            hRotation -= relativeMotion.X * sensitivity;

            // update the Vertical rotation
            vRotation -= relativeMotion.Y * sensitivity;

            // Calmp the Vertical Rotation
            vRotation = Mathf.Clamp(vRotation, minPitch, maxPitch);

            // Update Object Rotation
            RotationDegrees = new Vector3(0, hRotation, vRotation);
        }
    }
}
