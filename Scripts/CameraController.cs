using System;
using System.Runtime.CompilerServices;
using Godot;

public partial class CameraController : Node3D
{
    [Export] public float sensitivity = 0.3f;
    [Export] public float maxPitch = 60f;
    [Export] public float minPitch = -30f;
    
    // Joystick sensitivity often needs to be different than mouse
    [Export] public float joystickSensitivity = 2.0f; 

    private int _playerDeviceId;
    private Player player;
    private SpringArm3D _springArm;

    public override void _Ready()
    {
        // 1. Grab the Player reference
        player = Owner as Player; // Or GetParent() depending on setup
        if (player == null)
        {
            GD.PrintErr("Camera controller cannot find player");
            return;
        }

        _playerDeviceId = player.PlayerDeviceId;
        
        // 2. Grab the SpringArm reference
        _springArm = GetNode<SpringArm3D>("SpringArm3D");
        
        // Optional: Ensure the SpringArm doesn't inherit rotation from the parent 
        // if you want the camera to stay steady when the player turns.
        _springArm.TopLevel = false; 
    }

    // Called on Every Input (Mouse)
    public override void _Input(InputEvent @event)
    {
        // // 1. SAFETY CHECK: Only move camera if the mouse is captured (Gameplay Mode)
        // if (Input.MouseMode != Input.MouseModeEnum.Captured)
        // {
        //     return;
        // }
        //
        // // 2. Handle Mouse Motion
        // if (@event is InputEventMouseMotion motion)
        // {
        //     ApplyRotation(motion.Relative.X * sensitivity, motion.Relative.Y * sensitivity);
        // }
    }

    // Called every frame (Joystick)
    public override void _Process(double delta) 
    {
        // Assuming you are getting a Vector2 from your Input map somewhere else
        // and calling RotateFromVector. 
        // If you are calling RotateFromVector from the Player script, this is fine.
        // If you want to handle it here:
        Vector2 lookDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (lookDir.Length() > 0)
        {
             RotateFromVector(lookDir * (float)delta * joystickSensitivity);
        }
    }
    
    public void ManualRotate(Vector2 relativeMotion)
    {
        // Only apply if this is the correct player (Player 1 / Mouse User)
        if (_playerDeviceId == 0) 
        {
            ApplyRotation(relativeMotion.X * sensitivity, relativeMotion.Y * sensitivity);
        }
    }

    // Unified Rotation Logic (Used by both Mouse and Joystick)
private void ApplyRotation(float yawChange, float pitchChange)
    {
        // 1. Yaw (Left/Right) - Rotate the ROOT (The Turntable)
        // We rotate 'this' (The CameraController node), NOT the _springArm.
        // Since the Controller is always flat (0,0,0), it rotates perfectly horizontally.
        this.RotateY(Mathf.DegToRad(-yawChange));

        // 2. Pitch (Up/Down) - Rotate the SPRING ARM (The Hinge)
        float currentPitch = _springArm.RotationDegrees.X;
        
        currentPitch -= pitchChange; 
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Apply rotation ONLY to X. Keep Y and Z at 0 for the arm.
        // The Arm doesn't need to know about Yaw, because its Parent (Root) handles that.
        _springArm.RotationDegrees = new Vector3(currentPitch, 0, 0);
    }

    // Updated Joystick Method
    public void RotateFromVector(Vector2 V)
    {
        if (V.Length() == 0) return;
        
        // Pass the Vector data to the unified function
        // V.X is Left/Right, V.Y is Up/Down
        ApplyRotation(V.X, V.Y);
    }

    // --- HELPER FOR YOUR WASD MOVEMENT ---
    // Since we are rotating the SpringArm (child), the Root (parent) Rotation stays at 0.
    // If your Player script asks for "Rotation.Y", it gets 0.
    // Use this property to get the ACTUAL camera angle for movement.
public float CameraYaw
    {
        // We now return the Global Rotation of the Controller (Root), 
        // because that is the one doing the horizontal spinning.
        get { return this.GlobalRotation.Y; }
    }
}