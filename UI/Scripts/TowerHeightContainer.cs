using System;
using System.Collections.Generic;
using Godot;

public partial class TowerHeightContainer : CenterContainer
{
    private GameManager gameManager;
    private float GoalHeight; // Default goal height
    private List<TextureRect> playerHeadIcons = new List<TextureRect>();

    //make it so it spawns the appropriate number of head icon things based on number of players
    public override void _Ready()
    {
        gameManager = GameManager.Instance;
        gameManager.PlayerConfigs.ForEach(playerConfig =>
        {
            GD.Print($"Player Device ID: {playerConfig.DeviceId}");
            //get head icon for each player
        });
        //will make work later
        //var goalZone = GetNode<GoalZone>("GoalZone");
        GoalHeight = 25;
        // collect all Player{n}Marker TextureRect nodes and store them
        for (int i = 0; i < gameManager.PlayerConfigs.Count; i++)
        {
            string nodeName = $"Player{i + 1}Marker";
            var found = GetNode<TextureRect>($"AxisZone/{nodeName}");
            if (found != null)
                playerHeadIcons.Add(found);
            else
                GD.PrintErr($"Could not find TextureRect node named '{nodeName}'.");
        }
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
                float heightProportion = playerHeight / GoalHeight;
                float actualHeight = heightProportion * GetNode<Control>("AxisZone").Size.Y;
                int playerIndex = gameManager.PlayerConfigs.IndexOf(playerConfig);
                var headIcon = playerHeadIcons[playerIndex];
                headIcon.Position = new Vector2(
                    headIcon.Position.X,
                    GetNode<Control>("AxisZone").Size.Y - actualHeight
                );
                GD.Print(
                    $"Player {playerConfig.DeviceId} Height: {playerHeight}m, Proportion: {heightProportion}, Actual Y Position: {actualHeight}"
                );
                //update head icon element position based on height proportionaal to goal height
            }
        });
    }
}
