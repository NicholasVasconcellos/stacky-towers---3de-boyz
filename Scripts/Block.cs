using System;
using Godot;

public partial class Block : RigidBody3D
{
    private Node3D snapPoints;
    private MeshInstance3D defaultMesh;
    private MeshInstance3D highlightMesh;

    private MeshInstance3D previewMesh;
    private MeshInstance3D debugMesh;

    private CollisionShape3D blockCollider;

    // Grounded Check
    private float groundCheckDistance = 0.15f;

    // private bool _isGrounded;
    // private bool _isRoofed;

    // Block Side lengh in meters
    [Export]
    private int lenght = 2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Get Snap Points Container
        snapPoints = GetNode<Node3D>("SnapPoints");

        // Get Mesh Object Reference
        defaultMesh = GetNode<MeshInstance3D>("Meshes/DefaultMesh");
        highlightMesh = GetNode<MeshInstance3D>("Meshes/HighlightMesh");
        previewMesh = GetNode<MeshInstance3D>("Meshes/PreviewMesh");
        debugMesh = GetNode<MeshInstance3D>("Meshes/DebugMesh");

        // // Get Ray Cast References
        // groundCheck = GetNode<RayCast3D>("GroundCheck");
        // roofCheck = GetNode<RayCast3D>("RoofCheck");

        // SEt Visibility
        defaultMesh.Visible = true;
        highlightMesh.Visible = false;

        // Set Collider Reference
        blockCollider = GetNode<CollisionShape3D>("BlockCollider");
    }

    public override void _PhysicsProcess(double delta)
    {
        // _isGrounded = groundCheck.IsColliding();
        // _isRoofed = roofCheck.IsColliding();

        // if (_isGrounded)
        // {
        //     CollisionLayer = 0b10;
        //     CollisionMask = 0b110;
        // }
        // else
        // {
        //     // Falling
        //     CollisionLayer = 0b1000;
        //     CollisionMask = 0b1111;
        // }
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

    public MeshInstance3D getPreviewMesh()
    {
        return previewMesh;
    }

    public void addDebugMesh()
    {
        debugMesh.Visible = true;
    }

    public void removeDebugMesh()
    {
        debugMesh.Visible = false;
    }

    // public bool isGrounded()
    // {
    //     return _isGrounded;
    // }

    // public bool isRoofed()
    // {
    //     return _isRoofed;
    // }

    public int getLenght()
    {
        return lenght;
    }
}
