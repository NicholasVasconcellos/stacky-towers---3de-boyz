using System;
using System.Runtime.CompilerServices;
using Godot;

public partial class CameraController : Node3D
{
    [Export]
    public float sensitivity = 0.3f;

    [Export]
    public float maxPitch = 60f;

    [Export]
    public float minPitch = -30f;

    // Joystick sensitivity often needs to be different than mouse
    [Export]
    public float joystickSensitivity = 200.0f;

    private Player player;
    private SpringArm3D _springArm;

    public override void _Ready()
    {
        // 1. Grab the Player reference
        player = Owner as Player; // Or GetParent() depending on setup

        if (player == null)
        {
            GD.PrintErr("Camera controller cannot find player");
            SetProcess(false);
            return;
        }

        // 2. Grab the SpringArm reference
        _springArm = GetNode<SpringArm3D>("SpringArm3D");

        // Optional: Ensure the SpringArm doesn't inherit rotation from the parent
        // if you want the camera to stay steady when the player turns.
        _springArm.TopLevel = false;
    }

    // Called every frame (Joystick)
    public override void _Process(double delta)
    {
        if (player.PlayerDeviceId == -1)
            return;

        Vector2 lookDir = Input.GetVector("look_left", "look_right", "look_up", "look_down");

        // 3. Optional: Filter by specific device if you have multiple controllers
        // (Input.GetVector merges all devices, but usually P1 is the only one touching the stick)

        if (lookDir.Length() > 0)
        {
            RotateFromVector(lookDir * (float)delta * joystickSensitivity);
        }
    }

    public void ManualRotate(Vector2 relativeMotion)
    {
        // Only apply if this is the correct player (Player 1 / Mouse User)
        if (player.PlayerDeviceId == -1)
        {
            ApplyRotation(relativeMotion.X * sensitivity, relativeMotion.Y * sensitivity);
        }
    }

    // Unified Rotation Logic (Used by both Mouse and Joystick)
    private void ApplyRotation(float yawChange, float pitchChange)
    {
        // 1. Yaw (Left/Right)
        this.RotateY(Mathf.DegToRad(-yawChange));

        // 2. Pitch (Up/Down)
        if (_springArm != null)
        {
            float currentPitch = _springArm.RotationDegrees.X;
            currentPitch -= pitchChange;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            _springArm.RotationDegrees = new Vector3(currentPitch, 0, 0);
        }
    }

    // Updated Joystick Method
    public void RotateFromVector(Vector2 V)
    {
        if (V.Length() == 0)
            return;
        ApplyRotation(V.X, V.Y);
    }

    public float CameraYaw
    {
        get { return this.GlobalRotation.Y; }
    }
}
