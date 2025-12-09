using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TowerHeightContainer : CenterContainer
{
    private GameManager gameManager;
    private Control axisZone;
    private GoalZone goalZone;
    private float goalHeight = 20;

    private List<TextureRect> playerHeadIcons = new List<TextureRect>();
    private Control goalTick;

    public override void _Ready()
    {
        gameManager = GameManager.Instance;
        axisZone = GetNode<Control>("AxisZone");

        goalZone = GetNodeOrNull<GoalZone>("../../GoalZone");

        if (goalZone != null)
        {
            goalHeight = goalZone.GoalHeight;

            // Adjust for collision shape bottom to show the actual entry height
            var colShape = goalZone.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
            if (colShape != null && colShape.Shape is BoxShape3D boxShape)
            {
                float zoneScaleY = goalZone.Scale.Y;
                float colScaleY = colShape.Scale.Y;
                float colPosY = colShape.Position.Y;
                float boxHeight = boxShape.Size.Y;

                float worldBoxHeight = boxHeight * colScaleY * zoneScaleY;
                float worldBoxCenterOffset = colPosY * zoneScaleY;
                float worldBoxCenter = goalZone.GoalHeight + worldBoxCenterOffset;
                float worldBoxBottom = worldBoxCenter - (worldBoxHeight / 2.0f);

                goalHeight = worldBoxBottom - 0.5f;
                GD.Print(
                    $"TowerHeightContainer: Adjusted goal height to {goalHeight} (Center: {goalZone.GoalHeight})"
                );
            }
        }
        else
        {
            GD.Print("TowerHeightContainer: GoalZone not found, using default height.");
        }

        // Get goal tick
        goalTick = axisZone.GetNodeOrNull<Control>("HeightTick_50m");
        if (goalTick != null && goalTick is HeightTick tick)
        {
            tick.UpdateTickText("Goal");
        }
        else
        {
            GD.Print("TowerHeightContainer: GoalTick not found or not a HeightTick.");
        }

        // Get player head icons
        playerHeadIcons.Clear();
        for (int i = 0; i < gameManager.PlayerConfigs.Count; i++)
        {
            var playerConfig = gameManager.PlayerConfigs[i];
            var markerName = $"Player{i + 1}Marker";
            var marker = axisZone.GetNodeOrNull<TextureRect>(markerName);

            if (marker != null)
            {
                playerHeadIcons.Add(marker);
                marker.Visible = true;
                marker.Modulate = playerConfig.PlayerColor;
            }
        }

        // Move all the way to the right if single player
        if (gameManager.PlayerConfigs.Count == 1)
        {
            AnchorLeft = 0.5f;
            AnchorRight = 1.5f;
        }
    }

    public override void _Process(double delta)
    {
        if (axisZone == null)
            return;

        float axisHeight = axisZone.Size.Y;
        float pixelsPerMeter = (axisHeight * 0.9f) / Math.Max(goalHeight, 1);

        if (goalTick != null)
        {
            float goalY = axisHeight - (goalHeight * pixelsPerMeter);
            goalTick.Position = new Vector2(50, goalY);
        }

        for (int i = 0; i < gameManager.PlayerConfigs.Count; i++)
        {
            var playerConfig = gameManager.PlayerConfigs[i];
            if (i >= playerHeadIcons.Count)
                continue;

            var marker = playerHeadIcons[i];

            float playerHeight = 0;
            if (playerConfig.PlayerInstance != null && IsInstanceValid(playerConfig.PlayerInstance))
            {
                playerHeight = playerConfig.PlayerInstance.GlobalPosition.Y;
            }

            float markerY = axisHeight - (playerHeight * pixelsPerMeter);
            float xPos = 5 + (i * 25);

            marker.Position = new Vector2(xPos, markerY - (marker.Size.Y / 2));
        }
    }
}
