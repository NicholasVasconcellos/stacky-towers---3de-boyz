using System;
using Godot;

public partial class BlockL : Block
{
    private Node3D defaultMeshes;
    private Node3D highlightMeshes;

    private Node3D previewMeshes;
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

    protected override void initMeshes()
    {
        // Get Mesh Container References
        defaultMeshes = GetNode<Node3D>("Meshes/DefaultMeshes");
        highlightMeshes = GetNode<Node3D>("Meshes/HighlightMeshes");
        previewMeshes = GetNode<Node3D>("Meshes/PreviewMeshes");
    }

    public override void Highlight()
    {
        // Change Skin to be Highlighted Skin
        defaultMeshes.Visible = false;
        highlightMeshes.Visible = true;
    }

    public override void removeHighlight()
    {
        GD.Print("I have been Called");
        // Change Skin to default
        defaultMeshes.Visible = true;
        highlightMeshes.Visible = false;
    }

    // public override CollisionShape3D getCollider()
    // {
    //     return blockCollider;
    // }

    // public override MeshInstance3D getPreviewMesh()
    // {
    //     return previewMeshes;
    // }
}
