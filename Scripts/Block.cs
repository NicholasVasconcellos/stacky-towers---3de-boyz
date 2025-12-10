using System;
using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Block : RigidBody3D
{
    // Max Speed
    [Export]
    protected float maxSpeed = 20f;
    protected Vector3[] cubeCenters;
    protected Node3D defaultMeshes;
    protected Node3D highlightMeshes;

    protected Node3D previewMeshes;

    protected CollisionShape3D blockCollider;

    // Grounded Check
    protected float groundCheckDistance = 0.15f;

    // private bool _isGrounded;
    // private bool _isRoofed;

    // Block Side lengh in meters
    [Export]
    protected int lenght = 2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Initialize mesh references
        initMeshes();

        // Init Cube Centers
        initCubeCenters();

        // Ray Castign Off
        // // Get Ray Cast References
        // groundCheck = GetNode<RayCast3D>("GroundCheck");
        // roofCheck = GetNode<RayCast3D>("RoofCheck");

        initMeshVisiblity();

        // Set Collider Reference
        initColliderReferences();
    }

    public override void _PhysicsProcess(double delta)
    {
        ClampVelocity();
    }

    protected void ClampVelocity()
    {
        if (LinearVelocity.Length() > maxSpeed)
        {
            LinearVelocity = LinearVelocity.Normalized() * maxSpeed;
        }
    }

    protected virtual void initCubeCenters()
    {
        cubeCenters = new Vector3[] { new Vector3(0, 0, 0) };
    }

    public Vector3[] getCubeCenters()
    {
        return cubeCenters.ToArray();
    }

    protected virtual void initMeshVisiblity()
    {
        // SEt Visibility
        defaultMeshes.Visible = true;
        highlightMeshes.Visible = false;
    }

    protected virtual void initMeshes()
    {
        // Get Mesh Container References
        defaultMeshes = GetNode<Node3D>("Meshes/DefaultMeshes");
        highlightMeshes = GetNode<Node3D>("Meshes/HighlightMeshes");
        previewMeshes = GetNode<Node3D>("Meshes/PreviewMeshes");
    }

    protected virtual void initColliderReferences()
    {
        blockCollider = GetNode<CollisionShape3D>("BlockCollider");
    }

    public virtual void Highlight()
    {
        // Change Skin to be Highlighted Skin
        defaultMeshes.Visible = false;
        highlightMeshes.Visible = true;
    }

    public virtual void removeHighlight()
    {
        // Change Skin to default
        defaultMeshes.Visible = true;
        highlightMeshes.Visible = false;
    }

    public virtual CollisionShape3D getCollider()
    {
        return blockCollider;
    }

    public virtual Node3D getPreviewMeshes()
    {
        return previewMeshes;
    }

    public int getblockLength()
    {
        return lenght;
    }
}
