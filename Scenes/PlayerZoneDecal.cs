using System;
using Godot;

public partial class PlayerZone : Node3D
{
    // Assigned by PlayerZones manager
    public int PlayerDeviceId { get; set; } = -1;

    private MeshInstance3D _zoneMesh;

    public override void _Ready()
    {
        _zoneMesh = GetNode<MeshInstance3D>("MeshInstance3D");
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
