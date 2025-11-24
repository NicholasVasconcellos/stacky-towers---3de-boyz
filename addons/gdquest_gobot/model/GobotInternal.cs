using System;
using Godot;

public partial class GobotInternal : Node3D
{
    [Signal]
    public delegate void FootStepEventHandler();

    // Move the signal registration to the constructor
    public GobotInternal()
    {
        if (!HasUserSignal("foot_step"))
        {
            AddUserSignal("foot_step");
        }
    }

    public override void _Ready()
    {
        GD.Print("GobotInternal ready, checking signal: " + HasSignal("foot_step"));
    }
}
