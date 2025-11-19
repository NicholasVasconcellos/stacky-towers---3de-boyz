using System;
using Godot;

public partial class GoalZone : Area3D
{
    [Export]
    public UIManager LevelUIManager;

    [Export]
    public int GoalHeight = 1000;

    private bool _hasTriggered = false;
    
    public override void _Ready()
    {
        // Connect the body_entered signal to a method
        Position = new Vector3(Position.X, GoalHeight, Position.Z);
    }

    private void _on_body_entered(Node3D body)
    {
        if (_hasTriggered) return;
        
        // Check if the body that entered is the Player
        if (body is Player playerNode)
        {
            GD.Print($"Player Device ID: {playerNode.PlayerDeviceId}");
            
            _hasTriggered = true;
            // Notify UIManager of win condition
            if (LevelUIManager != null)
            {
                LevelUIManager.TriggerWinSequence($"Player {playerNode.PlayerDeviceId + 1} Wins!");
            }
            else
            {
                GD.PrintErr("GoalZone: UIManager not assigned in Inspector!");
            }
        }
    }
}
