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

    public int PlayerDeviceId { get; private set; }
    public Color PlayerColor { get; private set; }

    /*Grabbing Functionality*/
    // Valid Grab Area (volume)
    private Area3D grabRange;

    // Ref to Grabbed Block Object
    private RigidBody3D grabbedBlock;

    private Node3D prevParent;

    // Reference to Block's Collision Shape
    private CollisionShape3D blockCollider;

    // Temporary Collision Shape
    private CollisionShape3D tempCollider;

    private GobotSkin characterModel;

    private Node3D grabFeatures;

    // Store the current input direction for this player
    private Vector2 inputDirection = Vector2.Zero;

    //for use with game manager
    public void Initialize(int deviceId, Color color, int xOffset, int zOffset)
    {
        this.PlayerDeviceId = deviceId;
        this.PlayerColor = color;
        this.Position = new Vector3(this.Position.X + xOffset, 0, this.Position.Z + zOffset);
    }

    public override void _Ready()
    {
        Camera = GetNode<Node3D>(CameraPath);

        // Initialize grab Features
        grabFeatures = GetNode<Node3D>("GrabFeatures");
        // Initialize the grab area
        grabRange = GetNode<Area3D>("GrabFeatures/GrabRange");
        // Initialize the reference to the Character Model
        characterModel = GetNode<GobotSkin>("GobotSkin");
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

        // Use the stored input direction (updated in _UnhandledInput per device)
        Vector2 inputVector = inputDirection;

        // Create direction vector and rotate by camera's Y rotation
        Vector3 moveDirection = new Vector3(inputVector.X, 0, inputVector.Y);
        moveDirection = moveDirection.Rotated(Vector3.Up, Camera.GlobalRotation.Y);
        moveDirection = moveDirection.Normalized();

        if (moveDirection != Vector3.Zero)
        {
            // Add Velocity
            velocity.X = moveDirection.X * Speed;
            velocity.Z = moveDirection.Z * Speed;

            // Rotate the Model
            // calc rotation angle
            // Set Rotation of the Char Model
            float currAngle = characterModel.Rotation.Y;
            characterModel?.Run();

            // Target Angle
            float targetAngle = MathF.Atan2(moveDirection.X, moveDirection.Z);

            // Rotate Towards Angle
            float nextAngle = Mathf.LerpAngle(currAngle, targetAngle, TurnSpeed * (float)delta);
            characterModel.Rotation = new Vector3(Rotation.X, nextAngle, Rotation.Z);
            // Rotate Grab Features
            grabFeatures.Rotation = new Vector3(Rotation.X, nextAngle, Rotation.Z);
        }
        else
        {
            // Smooth Stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
            characterModel?.Idle();
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        Vector3 velocity = Velocity;
        if (@event.Device != PlayerDeviceId)
        {
            return; //Not this player's input
        }

        // Update movement direction based on this player's input
        inputDirection = Input.GetVector("Move Back", "Move Forward", "Move Left", "Move Right");

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            characterModel?.Jump();
            velocity.Y = JumpVelocity;
        }

        // Handle Grab Button (device-specific)
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

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        // Movement and grab are now in _UnhandledInput for per-player input
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

                // Disable Block Collision
                blockCollider = rb.GetNode<CollisionShape3D>("BlockCollider");
                blockCollider.Disabled = true;

                // Make Block a child of player
                rb.Reparent(this);

                // Set the Position with the offset
                rb.Position = holdOffset;

                // Create new Collision Shape the same size as the block
                tempCollider = new CollisionShape3D();
                // Set Shape
                tempCollider.Shape = rb.GetNode<CollisionShape3D>("BlockCollider").Shape;
                // Add as child
                AddChild(tempCollider);
                // Set Relative Position
                tempCollider.Position = holdOffset;

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

        //TODO Play Placing Animation

        // Remove the Temp Collision Shape
        if (tempCollider != null)
        {
            tempCollider.QueueFree();
            tempCollider = null;
        }

        // Set to original parent
        grabbedBlock.Reparent(prevParent);

        // Reset the Block's Collider
        blockCollider.Disabled = false;

        // Place the Block in front of player
        grabbedBlock.GlobalPosition =
            GlobalPosition + PlacementOffset.Rotated(Vector3.Up, characterModel.Rotation.Y);

        // cast to Rigid Body 3d and turn off freeze
        if (grabbedBlock is RigidBody3D rb)
        {
            rb.Freeze = false;
        }

        // Remove the grabbed Block
        grabbedBlock = null;
    }
}
