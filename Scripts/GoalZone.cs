using System;
using Godot;

public partial class GoalZone : Area3D
{
    private GameManager gameManager;

    [Export]
    public int GoalHeight = 25; // (Unused right now; could be used for variable goal height)

    public override void _Ready()
    {
        // Connect the body_entered signal to a method
        this.Position = new Vector3(this.Position.X, GoalHeight, this.Position.Z);
    }

    private void _on_body_entered(Node3D body)
    {
        // Check if the body that entered is the Player
        if (body is Player playerNode)
        {
            GD.Print($"Player Device ID: {playerNode.PlayerDeviceId}");
            // Notify GameManager of win condition
            gameManager = GameManager.Instance;
            gameManager.GoToWinScreen();
        }
    }
}
