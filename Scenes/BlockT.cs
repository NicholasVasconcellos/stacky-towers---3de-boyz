using System;
using Godot;

public partial class BlockT : Block
{
    private Godot.Collections.Array<CollisionShape3D> colliders;

    [Export]
    private int cubeLenght = 2;

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

    protected override void initCubeCenters()
    {
        cubeCenters = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 2, 0),
            new Vector3(-1, 0, 0),
        };

        // Scale by current cube lenght
        for (int i = 0; i < cubeCenters.Length; i++)
        {
            cubeCenters[i] *= cubeLenght;
        }
    }

    public override void Highlight()
    {
        // Change Skin to be Highlighted Skin
        defaultMeshes.Visible = false;
        highlightMeshes.Visible = true;
    }

    public override void removeHighlight()
    {
        // Change Skin to default
        defaultMeshes.Visible = true;
        highlightMeshes.Visible = false;
    }

    public override Node3D getPreviewMeshes()
    {
        return previewMeshes;
    }
}
