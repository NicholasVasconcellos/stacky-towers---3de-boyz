using System;
using System.Runtime.CompilerServices;
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

    [Export]
    public double hAcceleration = 2.0;

    [Export]
    public double vAcceleration = 2.0;

    private int playerDeviceId;
    private Player player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = Owner as Player;

        if (player == null)
        {
            GD.PrintErr("Camera controller cannot find player ");
            return;
        }

        // Now you have the ID!
        playerDeviceId = player.PlayerDeviceId;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    // Called on Every Input
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.Device != playerDeviceId)
        {
            return; // If yes, stop. This isn't my business.
        }

        base._UnhandledInput(@event);

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

    //for doing joystick rotation
    private void RotateFromVector(Vector2 V)
    {
        if (V.Length() == 0)
        {
            return;
        }
        Rotation = new Vector3(Rotation.X, Rotation.Y + V.X, Rotation.Z);
    }
}
