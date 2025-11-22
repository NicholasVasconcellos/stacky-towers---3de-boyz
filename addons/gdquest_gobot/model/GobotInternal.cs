using Godot;
using System;

public partial class GobotInternal : Node3D
{
    // 1. Keep this for good measure, though it's currently being ignored.
    [Signal]
    public delegate void FootStepEventHandler();

    public override void _Ready()
    {
        // 2. THE FORCE FIX:
        // We check if the engine knows about the signal. 
        // If not, we force-add it manually while the game is loading.
        if (!HasUserSignal("foot_step"))
        {
            AddUserSignal("foot_step");
            GD.Print("Force-registered 'foot_step' signal on " + Name);
        }
    }
}