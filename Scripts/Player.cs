using System;
using Godot;
using Godot.NativeInterop;

public partial class Player : CharacterBody3D
{
    [Signal]
    public delegate void FuelUpdatedEventHandler(double currentFuel, double maxFuel);

    [Export(PropertyHint.Range, "0, 50")]
    public float JetpackAcceleration { get; set; } = 15.0f;

    [Export(PropertyHint.Range, "0, 100")]
    public double MaxJetpackFuel { get; set; } = 100.0;

    [Export(PropertyHint.Range, "0, 50")]
    public double FuelBurnRate { get; set; } = 20.0; // Fuel per second

    [Export(PropertyHint.Range, "0, 50")]
    public double FuelRegenRate { get; set; } = 10.0; // Fuel per second

    [Export(PropertyHint.Range, "0, 5")]
    public double RegenDelay { get; set; } = 1.0;

    private double _timeSinceLastJetpack = 0.0;

    private double _currentFuel;
    private bool _jetpackInputHeld = false;
    private Jetpack _jetpack;

    private bool _canMove = true;
    private PlayerHUD _localHUD;

    [Export]
    public float Speed = 5.0f;

    [Export]
    public float Acceleration = 5.0f;

    //how fast character turns to face move direction
    [Export]
    public float TurnSpeed = 2.0f;

    [Export]
    public float JumpVelocity = 6.0f;

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
    // Time for block to reach grab position

    // Become Click Locked while in the middle of Placing or Grabbing
    private bool clickLocked = false;

    [Export]
    private float grabTime = 0.2f;

    // Time for block to reach placement Position
    [Export]
    private float placeTime = 0.2f;
    private Node3D placementSnapPoint = null;

    // Valid Grab Area (volume)
    private Area3D grabRange;

    private Area3D placeRange;

    // Ref to Grabbed Block Object
    private Block grabbedBlock;

    private MeshInstance3D placePreview = null;

    private Node3D prevParent;

    // Ref to Currently Highlighted Block Object
    private Block highlightedBlock = null;

    // Array of Blocks in Range
    private Godot.Collections.Array<Block> blocksInRange = new();

    // Reference to Block's Collision Shape
    private CollisionShape3D blockCollider;

    // Temporary Collision Shape
    private CollisionShape3D tempCollider;

    /* Block Placement Features*/
    private RayCast3D placeRay;

    private ShapeCast3D placeShapeCast;

    [Export]
    private float distSquareTreshold = 1.0f;

    private GobotSkin characterModel;

    // Container for all elements that rotate on charcter movement
    private Node3D bodyPivot;

    private Node3D grabFeatures;

    // Store the current input direction for this player
    private Vector2 inputDirection = Vector2.Zero;

    //for use with game manager
    public Player Initialize(int deviceId, Color color, int xOffset, int zOffset)
    {
        PlayerDeviceId = deviceId;
        PlayerColor = color;
        Position = new Vector3(Position.X + xOffset, 0, Position.Z + zOffset);
        return this;
    }

    public override void _Ready()
    {
        AddToGroup("Players");

        Camera = GetNode<Node3D>(CameraPath);

        // Init Pivot Node
        bodyPivot = GetNode<Node3D>("BodyPivot");
        // Initialize grab Features
        grabFeatures = GetNode<Node3D>("BodyPivot/GrabFeatures");
        // Initialize the grab area
        grabRange = GetNode<Area3D>("BodyPivot/GrabFeatures/GrabRange");

        // Initialize the reference to the Character Model
        characterModel = GetNode<GobotSkin>("BodyPivot/GobotSkin");

        // Signals for Grab Range
        grabRange.BodyEntered += OnBlockEntered;
        grabRange.BodyExited += OnBlockExited;

        // Jetpack Variables
        _jetpack = GetNode<Jetpack>("BodyPivot/GobotSkin/JetpackMount/Jetpack");
        _currentFuel = MaxJetpackFuel;

        // HUD
        _localHUD = GetNode<PlayerHUD>("CanvasLayer/PlayerHud");
        _localHUD.OnUpdateFuel(_currentFuel, MaxJetpackFuel);

        // Get RayCast ref
        placeRay = GetNode<RayCast3D>("BodyPivot/PlaceRay_Front");
        // Get Placement Shape Cast
        placeShapeCast = GetNode<ShapeCast3D>("BodyPivot/PlaceShapeCast");
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

        bool isJetpacking = _canMove && _jetpackInputHeld && _currentFuel > 0;

        if (isJetpacking)
        {
            velocity.Y = JumpVelocity * (float)0.75;
            _currentFuel -= FuelBurnRate * delta;
            _jetpack.StartEffects();

            _timeSinceLastJetpack = 0.0;
        }
        else
        {
            _jetpack.StopEffects();

            _timeSinceLastJetpack += delta;
        }

        // Use the stored input direction (updated in _UnhandledInput per device)
        Vector2 inputVector = _canMove ? inputDirection : Vector2.Zero;

        // Create direction vector and rotate by camera's Y rotation
        Vector3 moveDirection = new Vector3(inputVector.X, 0, inputVector.Y);
        //moveDirection = moveDirection.Rotated(Vector3.Up, Camera.GlobalRotation.Y);
        float cameraAngle = Camera.GetNode<Node3D>("SpringArm3D").GlobalRotation.Y;
        moveDirection = moveDirection.Rotated(Vector3.Up, cameraAngle);

        if (moveDirection != Vector3.Zero)
        {
            // Add Velocity
            velocity.X = moveDirection.X * Speed;
            velocity.Z = moveDirection.Z * Speed;

            // Play Run Animation
            characterModel?.Run();

            // Rotate
            // calc rotation angle
            // Set Rotation of the Body Pivot
            float currAngle = bodyPivot.Rotation.Y;

            // Calc Target Angle from move direction
            float targetAngle = MathF.Atan2(moveDirection.X, moveDirection.Z);

            // Rotate Towards Angle
            float nextAngle = Mathf.LerpAngle(currAngle, targetAngle, TurnSpeed * (float)delta);
            bodyPivot.Rotation = new Vector3(Rotation.X, nextAngle, Rotation.Z);
            // Rotate Grab Features
            // grabFeatures.Rotation = new Vector3(Rotation.X, nextAngle, Rotation.Z);
        }
        else
        {
            // Smooth Stopping
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
            // Play Idle Animation
            characterModel?.Idle();
        }

        if (_timeSinceLastJetpack > RegenDelay && _currentFuel < MaxJetpackFuel)
        {
            _currentFuel += FuelRegenRate * delta;
            _currentFuel = Math.Min(_currentFuel, MaxJetpackFuel);
        }

        // Placement Preview

        // Ensure Immediate Collision Updates
        placeShapeCast.ForceShapecastUpdate();
        if (
            grabbedBlock != null
            && placeShapeCast.IsColliding()
            && placeShapeCast.GetCollider(0) is Block targetBlock
        )
        {
            var collisionPoint = placeShapeCast.GetCollisionPoint(0);
            var normal = placeShapeCast.GetCollisionNormal(0);
            var distanceSquare = GlobalPosition.DistanceSquaredTo(collisionPoint);

            Vector3 placePosition;

            if (distanceSquare < distSquareTreshold)
            {
                // Up Place
                placePosition = targetBlock.GlobalPosition + Vector3.Up * targetBlock.getLenght();
            }
            else
            {
                // Front Place

                // Place position is in the normal direction offset by half a lenght
                placePosition = targetBlock.GlobalPosition + normal * targetBlock.getLenght();
            }

            GD.Print(
                $"Target: {targetBlock.GlobalPosition} | New Place: {placePosition} | Diff: {placePosition - targetBlock.GlobalPosition}"
            );

            // Check if Placing at position will clip any blocks (Todo)

            // Instanceciate grabbed blocked preview mesh at the place Position
            if (placePreview == null)
            {
                GD.Print("Unable to Find Place Preview Mesh");
            }
            // Position to target Position
            placePreview.GlobalPosition = placePosition;
            // Adjust rotation to be in line with target Block
            placePreview.GlobalRotation = targetBlock.Rotation;
            // Set to visible
            placePreview.Visible = true;

            // if (distanceSquare < distSquareTreshold)
            // {
            //     // Place on Top of the Block

            //     // Place position is in the normal direction offset by half a lenght
            //     Vector3 placePosition =
            //         targetBlock.GlobalPosition + normal * targetBlock.getLenght();

            //     // Instanceciate grabbed blocked preview mesh at the place Position
            //     MeshInstance3D placePreview = grabbedBlock.getPreviewMesh();

            //     AddChild(placePreview);
            //     placePreview.GlobalPosition = placePosition;
            //     placePreview.Visible = true;

            //     // show the preview mesh
            // }
            // else
            // {
            //     // Preview Front

            //     // Get the target location

            //     // Show the preview mesh
            // }
        }
        else
        {
            if (placePreview != null)
            {
                // Hide Preview for Now
                placePreview.Visible = false;
            }
        }

        // SEt Velocity and Move
        Velocity = velocity;
        MoveAndSlide();

        _localHUD.OnUpdateFuel(_currentFuel, MaxJetpackFuel);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_canMove)
            return;

        Vector3 velocity = Velocity;
        if (@event.Device != PlayerDeviceId)
        {
            return; //Not this player's input
        }

        // Update movement direction based on this player's input
        inputDirection = Input.GetVector("Move Left", "Move Right", "Move Forward", "Move Back");

        // Handle jump button
        if (Input.IsActionJustPressed("ui_accept"))
        {
            if (IsOnFloor())
            {
                characterModel?.Jump();
                velocity.Y = JumpVelocity;
            }
            else
            {
                _jetpackInputHeld = true;
            }
        }
        else if (Input.IsActionJustReleased("ui_accept"))
        {
            // Variable jump height
            if (velocity.Y > 0 && !_jetpackInputHeld)
            {
                velocity.Y *= 0.2f;
            }

            _jetpackInputHeld = false;
        }

        // Handle Grab Button (device-specific)
        if (Input.IsActionJustPressed("Grab") && !clickLocked)
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
    }

    public override void _Process(double delta)
    {
        // Movement and grab are now in _UnhandledInput for per-player input
    }

    private void Grab()
    {
        // Double Check not already holding or No Block in Range
        if (grabbedBlock != null || highlightedBlock == null)
        {
            return;
        }

        // Lock Click Interactions while grab is in process
        clickLocked = true;

        // Set internal variable grabbedBlock to the highlighted block
        grabbedBlock = highlightedBlock;

        // Set the Internal Preview Mesh Variable (Duplicate from the block's)
        placePreview = grabbedBlock.getPreviewMesh().Duplicate() as MeshInstance3D;
        GetTree().Root.AddChild(placePreview);
        placePreview.Visible = false; // Still Invisible

        // Unhighlight the block, Free the Array, remove reference
        highlightedBlock.removeHighlight();
        blocksInRange.Clear();
        highlightedBlock = null;

        // Store the previous parent of the block
        prevParent = grabbedBlock.GetParent<Node3D>();

        // Make Block a child of player
        grabbedBlock.Reparent(this);

        // Set Layer to None while moving to grab position (invisible)
        grabbedBlock.CollisionLayer = 0b0;
        // Set Mask to None
        grabbedBlock.CollisionMask = 0b0;

        // Disable physics of held block (make static)
        grabbedBlock.Freeze = true;

        Block blockBeingGrabbed = grabbedBlock;

        Tween tween = CreateTween();
        tween
            .TweenProperty(blockBeingGrabbed, "position", holdOffset, grabTime)
            .SetTrans(Tween.TransitionType.Linear);

        // Callback function when tween finishes
        tween.TweenCallback(
            Callable.From(() =>
            {
                // Set Layer to Player ONly while block is held
                blockBeingGrabbed.CollisionLayer = 0b1;
                // Disable Click Lock (Enable Grab / Place)
                clickLocked = false;
            })
        );
    }

    private void Place()
    {
        // Double Check not already Placed
        if (grabbedBlock == null)
        {
            return;
        }

        // Lock Click INteraction while Place is in process
        clickLocked = true;

        // Reset to original parent
        grabbedBlock.Reparent(prevParent);

        // Make it invisble on the way to placement location so it doesn't collide with player
        grabbedBlock.CollisionLayer = 0b0;

        // Reference to block just placed
        Block placedBlock = grabbedBlock;

        // Remove Grabbed Block Reference
        grabbedBlock = null;

        Vector3 targetPosition;

        if (placePreview != null && placePreview.Visible == true)
        {
            // if the placement preview is available

            // Target Postion is same as the preview
            targetPosition = placePreview.GlobalPosition;

            // Set the rotatin of the placed block (Fix make the rotation gradual as well)
            placedBlock.GlobalRotation = placePreview.GlobalRotation;

            // Remove the Placement Preview
            placePreview.Visible = false;
            placePreview.QueueFree();
            placePreview = null;
        }
        else
        {
            // Place in front of player
            targetPosition =
                GlobalPosition + PlacementOffset.Rotated(Vector3.Up, bodyPivot.Rotation.Y);
        }

        // Gradually Move Block to Target Position
        Tween tween = CreateTween();
        tween
            .TweenProperty(placedBlock, "global_position", targetPosition, placeTime)
            .SetTrans(Tween.TransitionType.Linear);

        // Callback function when tween finishes
        tween.TweenCallback(
            Callable.From(() =>
            {
                // Reset Block Layer Settings
                placedBlock.CollisionLayer = 0b10;
                // Player: No, Block: Yes, Floor: Yes
                placedBlock.CollisionMask = 0b110;

                // Un-Freeze Physics
                placedBlock.Freeze = false;

                // Disable Click Lock - Enable Grab/Place
                clickLocked = false;
            })
        );
    }

    private void OnBlockEntered(Node3D body)
    {
        // Ignore if Already Holding
        if (grabbedBlock != null)
        {
            return;
        }

        // New Block in Hitbox
        if (body is Block block)
        {
            // Add to Array
            blocksInRange.Add(block);

            // Only one block,
            if (blocksInRange.Count == 1)
            {
                // set that as highlighted block
                highlightedBlock = block;
                // Highlight it
                block.Highlight();
            }

            // More than one block, get the closest one
            UpdateHighlight();
        }
    }

    private void OnBlockExited(Node3D body)
    {
        // Ignore if Already Holding
        if (grabbedBlock != null)
        {
            return;
        }

        if (body is Block block)
        {
            // Remove it from array
            blocksInRange.Remove(block);

            // If Exitign block was the highlighted block
            if (block == highlightedBlock)
            {
                // Remove it
                block.removeHighlight();
                highlightedBlock = null;
            }

            // If Still Blocks in Range update highlight
            if (blocksInRange.Count > 0)
            {
                UpdateHighlight();
            }
        }
    }

    private void UpdateHighlight()
    {
        // Remove Curr Highlight if any
        if (highlightedBlock != null)
        {
            highlightedBlock.removeHighlight();
            highlightedBlock = null;
        }

        float minSquareDist = float.MaxValue;
        float currSquareDistance;

        foreach (var block in blocksInRange)
        {
            // Skip if reference no longer there
            if (!IsInstanceValid(block))
            {
                continue;
            }

            currSquareDistance = GlobalPosition.DistanceSquaredTo(block.GlobalPosition);
            if (currSquareDistance < minSquareDist)
            {
                minSquareDist = currSquareDistance;
                highlightedBlock = block;
            }
        }

        // Highlight Closest Block
        if (highlightedBlock != null)
        {
            highlightedBlock.Highlight();
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        _canMove = enabled;

        if (!enabled)
        {
            inputDirection = Vector2.Zero;
            _jetpackInputHeld = false;
            _jetpack?.StopEffects();
            characterModel?.Idle();
        }
    }
}
