using System;
using Godot;

public partial class PlayerZones : Node3D
{
    public override void _Ready()
    {
        var playerConfigs = GameManager.Instance.PlayerConfigs;
        GD.Print($"PlayerZones: Found {playerConfigs.Count} player configs.");
        int index = 0;

        foreach (Node3D child in GetChildren())
        {
            //idk why this is PlayerZone but PlayerZoneDecal is what it's supposed to be named and doen't work
            if (child is PlayerZone playerZone)
            {
                GD.Print($"PlayerZones: Processing Zone {index}");
                if (index < playerConfigs.Count)
                {
                    // assign player to this zone
                    var config = playerConfigs[index];
                    GD.Print(
                        $"PlayerZones: Assigning Zone {index} to DeviceId {config.DeviceId} with Color {config.PlayerColor}"
                    );
                    playerZone.PlayerDeviceId = config.DeviceId;
                    playerZone.SetColor(config.PlayerColor);
                    playerZone.Visible = true;
                }
                else
                {
                    // hide unused zones
                    GD.Print($"PlayerZones: Hiding unused Zone {index}");
                    playerZone.Visible = false;
                }
                index++;
            }
        }
    }
}
