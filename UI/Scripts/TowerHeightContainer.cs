using System;
using Godot;

public partial class TowerHeightContainer : CenterContainer
{
    private GameManager gameManager;

    //make it so it spawns the appropriate number of head icon things based on number of players
    public override void _Ready()
    {
        gameManager = GameManager.Instance;
        gameManager.PlayerConfigs.ForEach(playerConfig =>
        {
            GD.Print($"Player Device ID: {playerConfig.DeviceId}");
            //get head icon for each player
        });
    }

    public override void _Process(double delta)
    {
        //update player markers based on their heights
        gameManager.PlayerConfigs.ForEach(playerConfig =>
        {
            //get player height
            if (playerConfig.PlayerInstance != null)
            {
                float playerHeight = playerConfig.PlayerInstance.GlobalPosition.Y;
                //update head icon element position based on height proportionaal to goal height
            }
        });
    }
}
