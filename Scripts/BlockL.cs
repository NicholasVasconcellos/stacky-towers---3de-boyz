using System;
using Godot;

public partial class BlockL : Block
{
    private Node3D snapPoints;
    private MeshInstance3D defaultMesh;
    private MeshInstance3D highlightMesh;

    private MeshInstance3D previewMesh;
    private MeshInstance3D debugMesh;
    private Godot.Collections.Array<CollisionShape3D> colliders;

    [Export]
    private int cubeLenght = 2;

    public override void _Ready()
    {
        base._Ready();
    }

    protected override void initColliderReferences()
    {
        // Read All Children from Root
        foreach (Node child in GetChildren())
        {
            // If it's a collider add it to array
            if (child is CollisionShape3D collider)
            {
                colliders.Add(collider);
            }
        }
    }
}
