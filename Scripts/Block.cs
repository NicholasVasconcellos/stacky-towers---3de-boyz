using System;
using Godot;

public partial class Block : RigidBody3D
{
    private Node3D snapPoints;
    private MeshInstance3D defaultMesh;
    private MeshInstance3D highlightMesh;

    private CollisionShape3D blockCollider;

    // Block Side lengh in meters
    [Export]
    private int lenght = 2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Get Snap Points Container
        snapPoints = GetNode<Node3D>("SnapPoints");
        defaultMesh = GetNode<MeshInstance3D>("Meshes/DefaultMesh");
        highlightMesh = GetNode<MeshInstance3D>("Meshes/HighlightMesh");

        // SEt Visibility
        defaultMesh.Visible = true;
        highlightMesh.Visible = false;

        // Set Collider Reference
        blockCollider = GetNode<CollisionShape3D>("BlockCollider");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    private void initSnapPoints()
    {
        float halfLen = lenght / 2.0f;

        // Get a position vector for each face
        Vector3[] facePositions = new Vector3[]
        {
            new Vector3(0, halfLen, 0), // Up
            new Vector3(0, -halfLen, 0), // Down
            new Vector3(halfLen, 0, 0), // Right
            new Vector3(-halfLen, 0, 0), // Left
            new Vector3(0, 0, halfLen), // Front
            new Vector3(0, 0, -halfLen), // Back
        };

        // Instanciate the elements
        for (int i = 0; i < facePositions.Length; i++)
        {
            // Instantiate
            Node3D snapPoint = new Node3D();
            // Rename in order
            snapPoint.Name = $"SnapPoint_{i}";
            // Set Position
            snapPoint.Position = facePositions[i];
            // Add as Child
            snapPoints.AddChild(snapPoint);
        }
    }

    public void Highlight()
    {
        // Change Skin to be Highlighted Skin
        defaultMesh.Visible = false;
        highlightMesh.Visible = true;
    }

    public void removeHighlight()
    {
        GD.Print("I have been Called");
        // Change Skin to default
        defaultMesh.Visible = true;
        highlightMesh.Visible = false;
    }

    public CollisionShape3D getCollider()
    {
        return blockCollider;
    }
}
