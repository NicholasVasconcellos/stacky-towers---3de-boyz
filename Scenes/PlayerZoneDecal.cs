using System;
using Godot;

public partial class PlayerZoneDecal : Node3D
{
    // Assigned by PlayerZones manager
    public int PlayerDeviceId { get; set; } = -1;

    [Export]
    private MeshInstance3D _zoneMesh{ get; set; }

    public override void _Ready()
    {
        if (_zoneMesh == null)
        {
            GD.PrintErr("PlayerZoneDecal: Zone Mesh is not assigned!");
        }
    }

    public void SetColor(Color color)
    {
        if (_zoneMesh != null)
        {
            StandardMaterial3D mat = new StandardMaterial3D();
            mat.AlbedoColor = color;
            _zoneMesh.MaterialOverride = mat;
        }
    }
}
