using System;
using Godot;

public partial class GoalZone : Area3D
{
    private GameManager gameManager;

    [Export]
    public int GoalHeight = 100;

    public override void _Ready()
    {
        // Connect the body_entered signal to a method
        Position = new Vector3(Position.X, GoalHeight, Position.Z);
    }

    private void _on_body_entered(Node3D body)
    {
        // Check if the body that entered is the Player
        if (body is Player playerNode)
        {
            GD.Print($"Player Device ID: {playerNode.PlayerDeviceId}");
            // Notify GameManager of win condition
            gameManager = GameManager.Instance;
            gameManager.GoToWinScreen($"Player {playerNode.PlayerDeviceId} Wins!");
        }
    }
}
