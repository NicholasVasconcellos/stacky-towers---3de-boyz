using System;
using Godot;
using Godot.NativeInterop;

public partial class Player : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    [Export]
    public float Acceleration = 5.0f;

    //how fast character turns to face move direction
    [Export]
    public float TurnSpeed = 2.0f;

    [Export]
    public float JumpVelocity = 4.5f;

    // Position of Held Block

    [Export]
    public Vector3 holdOffset = new Vector3(0, 2.5f, 0);

    [Export]
    public Vector3 PlacementOffset = new Vector3(0, 1, 2);

    [Export]
    public NodePath CameraPath;
    private Node3D Camera;

    /*Grabbing Functionality*/
    // Valid Grab Area (volume)
    private Area3D grabRange;

    // Ref to Grabbed Block Object
    private RigidBody3D grabbedBlock;

    private Node3D prevParent;

    // Collision shape equal to the held block
    private CollisionShape3D heldBlockCollision;

    public override void _Ready()
    {
        Camera = GetNode<Node3D>(CameraPath);

        // Initialize the grab area
        grabRange = GetNode<Area3D>("GrabRange");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            // Decrement Velocity by a * dT
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Get 2d Input Vector
        Vector2 inputVector = Input.GetVector(
            "Move Back",
            "Move Forward",
            "Move Left",
            "Move Right"
        );
        GD.Print("input:", inputVector);

        // Create direction vector and rotate by camera's Y rotation
        Vector3 moveDirection = new Vector3(inputVector.X, 0, inputVector.Y);
        moveDirection = moveDirection.Rotated(Vector3.Up, Camera.GlobalRotation.Y);
        moveDirection = moveDirection.Normalized();

        if (moveDirection != Vector3.Zero)
        {
            velocity.X = moveDirection.X * Speed;
            velocity.Z = moveDirection.Z * Speed;

            // use this to change the angle of the skin once it's been imported
            Vector2 targetAngle = new Vector2(inputVector.X, inputVector.Y);
        }
        else
        {
            // Smooth Stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        // Handle Grab Button
        if (Input.IsActionJustPressed("Grab"))
        {
            if (grabbedBlock == null)
            {
                Grab();
            }
            else
            {
                Place();
            }
        }
    }

    private void Grab()
    {
        // Double Check not already holding
        if (grabbedBlock != null)
        {
            return;
        }

        // TODO: Play Grab Animation

        // Check all Block Objects in the Grab Range
        var objects = grabRange.GetOverlappingBodies();

        // for each Node in the range
        foreach (var body in objects)
        {
            // if it is a block, and a rigid body (then cast it to a rigid body variable rb)
            if (body.IsInGroup("Block") && body is RigidBody3D rb)
            {
                // Set Internal variable to that rigid Body
                grabbedBlock = rb;

                // Store the previous parent of the block
                prevParent = rb.GetParent<Node3D>();

                // Turn off gravity and momementum
                rb.Freeze = true;

                // Make Block a child of player
                rb.Reparent(this);

                // Set the Position with the offset
                rb.Position = holdOffset;

                // Make Block Non Colliding
                rb.CollisionLayer = 0;
                rb.CollisionMask = 0;

                // Create new Collision Shape the same size as the block
                heldBlockCollision = new CollisionShape3D();
                // Set Shape
                heldBlockCollision.Shape = rb.GetNode<CollisionShape3D>("CollisionShape3D").Shape;
                // Add as child
                AddChild(heldBlockCollision);
                // Set Relative Position
                heldBlockCollision.Position = holdOffset;

                break;
            }
        }
    }

    private void Place()
    {
        // Double Check not already Placed
        if (grabbedBlock == null)
        {
            return;
        }

        //TODO Play Place Animation

        // Set to original parent
        grabbedBlock.Reparent(prevParent);

        // Place the Block in front of player
        grabbedBlock.GlobalPosition = GlobalPosition + PlacementOffset;

        // cast to Rigid Body 3d and turn off freeze
        if (grabbedBlock is RigidBody3D rb)
        {
            rb.Freeze = false;
        }

        grabbedBlock.CollisionLayer = 2;
        grabbedBlock.CollisionMask = 3;

        // Remove the grabbed Block
        grabbedBlock = null;
    }
}
