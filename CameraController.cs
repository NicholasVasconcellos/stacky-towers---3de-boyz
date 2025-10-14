using System;
using Godot;

public partial class CameraController : Node3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        CharacterBody3D character = GetTree().GetNodesInGroup("Player")[0] as CharacterBody3D;
        GlobalPosition = character.GlobalPosition;
    }
}
