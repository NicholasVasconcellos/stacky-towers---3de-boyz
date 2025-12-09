using System;
using Godot;
using Godot.Collections;
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

    private float blockLength;

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

    private Vector3[] cubePositions;

    // private Shape3D grabbedBlockColliderShape;

    private PhysicsShapeQueryParameters3D query;

    // Placement Preview mesh
    private Node3D placePreview = null;

    private Array<MeshInstance3D> placePreviewMeshes;

    private StandardMaterial3D snapPreviewMaterial;
    private StandardMaterial3D invalidPreviewMaterial;

    private StandardMaterial3D defaultPreviewMaterial;

    private Node3D prevParent;

    // Define this near your other constants/exports

    [Export]
    private float placeCollisionScale = 0.9f;

    // Ref to Currently Highlighted Block Object
    private Block highlightedBlock = null;

    // Array of Blocks in Range
    private Godot.Collections.Array<Block> blocksInRange = new();

    // Reference to Block's Collision Shape
    private CollisionShape3D blockCollider;

    // Temporary Collision Shape
    private CollisionShape3D tempCollider;

    /* Block Placement Features*/

    private BoxShape3D scaledUnitCubeShape;

    private ShapeCast3D placeShapeCast;

    [Export]
    private float distSquareTreshold = 1.0f;

    //private AnimationNodeStateMachinePlayback _playback;

    // Container for all elements that rotate on charcter movement
    private Node3D bodyPivot;

    private Node3D grabFeatures;

    // Store the current input direction for this player
    private Vector2 inputDirection = Vector2.Zero;

    private string _actionMoveLeft;
    private string _actionMoveRight;
    private string _actionMoveForward;
    private string _actionMoveBack;
    private string _actionJump;
    private string _actionGrab;

    //for use with game manager
    public Player Initialize(int deviceId, Color color, int xOffset, int zOffset)
    {
        PlayerDeviceId = deviceId;
        PlayerColor = color;
        Position = new Vector3(Position.X + xOffset, 0, Position.Z + zOffset);

        if (PlayerDeviceId == -1) // Keyboard Player
        {
            _actionMoveLeft = "kb_move_left";
            _actionMoveRight = "kb_move_right";
            _actionMoveForward = "kb_move_forward";
            _actionMoveBack = "kb_move_back";
            _actionJump = "kb_jump";
            _actionGrab = "kb_grab";
        }
        else // Controller Player
        {
            _actionMoveLeft = "joy_move_left";
            _actionMoveRight = "joy_move_right";
            _actionMoveForward = "joy_move_forward";
            _actionMoveBack = "joy_move_back";
            _actionJump = "joy_jump";
            _actionGrab = "joy_grab";
        }
        return this;
    }

    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        AddToGroup("Players");
        animationPlayer = GetNode<AnimationPlayer>("BodyPivot/PlayerSkin/AnimationPlayer");
        if (animationPlayer == null)
        {
            GD.PrintErr("Animation Player not found in PlayerSkin");
        }
        Camera = GetNode<Node3D>(CameraPath);
        // Init Pivot Node
        bodyPivot = GetNode<Node3D>("BodyPivot");
        // Initialize grab Features
        grabFeatures = GetNode<Node3D>("BodyPivot/GrabFeatures");
        // Initialize the grab area
        grabRange = GetNode<Area3D>("BodyPivot/GrabFeatures/GrabRange");

        // Signals for Grab Range
        grabRange.BodyEntered += OnBlockEntered;
        grabRange.BodyExited += OnBlockExited;

        // Jetpack Variables
        _jetpack = GetNode<Jetpack>("BodyPivot/PlayerSkin/JetpackMount/Jetpack");
        _currentFuel = MaxJetpackFuel;

        // HUD
        _localHUD = GetNode<PlayerHUD>("CanvasLayer/PlayerHud");
        _localHUD.OnUpdateFuel(_currentFuel, MaxJetpackFuel);

        // Standard Material for Place Preview
        // Pre-create both materials
        snapPreviewMaterial = new StandardMaterial3D();
        snapPreviewMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        snapPreviewMaterial.AlbedoColor = new Color(0, 1, 0, 0.5f); // Green

        invalidPreviewMaterial = new StandardMaterial3D();
        invalidPreviewMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        invalidPreviewMaterial.AlbedoColor = new Color(1, 0, 0, 0.5f); // Red

        defaultPreviewMaterial = new StandardMaterial3D();
        defaultPreviewMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        defaultPreviewMaterial.AlbedoColor = new Color(0, 0, 1, 0.5f); // Blue

        // Get Placement Shape Cast
        placeShapeCast = GetNode<ShapeCast3D>("BodyPivot/PlaceShapeCast");

        // Init Query Parameter for Collision Check
        query = new PhysicsShapeQueryParameters3D();
        query.CollideWithBodies = true;
        query.CollisionMask = 0b10; // Check's only for blocks
        // Init basic cube shape (TODO GET THE BLOCK SHAPE FROM BLOCK CLASS STATIC VAR)
        float scaledCubeSize = 2 * placeCollisionScale; // Shrinked to remove tangential checks
        scaledUnitCubeShape = new BoxShape3D { Size = Vector3.One * scaledCubeSize };

        // Init Placement Preview Meshses Array
        placePreviewMeshes = new Array<MeshInstance3D>();
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
            //characterModel?.Run();
            animationPlayer.Play("Run");

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
            //characterModel?.Idle();
            //if no other animation playing, play idle
            animationPlayer.Play("idle");
        }

        if (_timeSinceLastJetpack > RegenDelay && _currentFuel < MaxJetpackFuel)
        {
            _currentFuel += FuelRegenRate * delta;
            _currentFuel = Math.Min(_currentFuel, MaxJetpackFuel);
        }

        // Update Placement Preview
        if (grabbedBlock != null)
        {
            updatePlacePreview();
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

        bool isKeyboardEvent = (
            @event is InputEventKey
            || @event is InputEventMouseButton
            || @event is InputEventMouseMotion
        );
        bool isJoypadEvent = (@event is InputEventJoypadButton || @event is InputEventJoypadMotion);

        if (PlayerDeviceId == -1)
        {
            if (!isKeyboardEvent)
                return; // KBM Player ignores controller inputs
        }
        else
        {
            if (!isJoypadEvent)
                return; // Controller Player ignores keyboard inputs
            if (@event.Device != PlayerDeviceId)
                return; // Controller Player ignores other controllers
        }

        Vector3 velocity = Velocity;

        // Update movement direction based on this player's input
        inputDirection = Input.GetVector(
            _actionMoveLeft,
            _actionMoveRight,
            _actionMoveForward,
            _actionMoveBack
        );

        // Handle jump button
        if (Input.IsActionJustPressed(_actionJump))
        {
            if (IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }
            else
            {
                _jetpackInputHeld = true;
            }
        }
        else if (Input.IsActionJustReleased(_actionJump))
        {
            // Variable jump height
            if (velocity.Y > 0 && !_jetpackInputHeld)
            {
                velocity.Y *= 0.2f;
            }

            _jetpackInputHeld = false;
        }

        if (Input.IsActionJustPressed(_actionGrab))
        {
            TryGrab();
        }
        Velocity = velocity;
        // falling, use fall animation, if rising, use jump animation
        if (!IsOnFloor())
        {
            if (Velocity.Y > 0)
            {
                animationPlayer.Stop();
                animationPlayer.Play("Jump");
            }
            else
            {
                animationPlayer.Stop();
                animationPlayer.Play("Fall");
            }
        }
    }

    public void TryGrab()
    {
        if (grabbedBlock == null)
        {
            animationPlayer.Play("Pick Up");
            Grab();
        }
        else
        {
            Place();
        }
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

        // Update Block Lenght Variable
        blockLength = grabbedBlock.getblockLength();

        // Set the Internal Preview Mesh Variable (Duplicate from the block's preview meshes)
        placePreview = (Node3D)grabbedBlock.getPreviewMeshes().Duplicate();
        // Pushes Individual Meshes to array
        loadPlacePreviewMeshes();

        // Add as child of grab features
        grabFeatures.AddChild(placePreview);

        // TODO: MAKE PLACE OFFSET A FUNCTION OF BLOCK'S LENGHT ON Z
        // Set starting position of place preview based on place Offset
        placePreview.Position = PlacementOffset;
        placePreview.Visible = true; // Make it visible

        // get the cube positions for this block
        cubePositions = grabbedBlock.getCubeCenters();

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

        // Reset Rotation
        blockBeingGrabbed.Rotation = Vector3.Zero;

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

    private void loadPlacePreviewMeshes()
    {
        if (placePreview == null)
        {
            GD.PrintErr("Can't possibly load the meshes if there is no placePreview node");
            return;
        }

        // Add all the Children meshes to an array so they can be updated
        foreach (Node child in placePreview.GetChildren())
        {
            // If it's a mesh add it to array
            if (child is MeshInstance3D mesh)
            {
                placePreviewMeshes.Add(mesh);
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

        // Lock Click INteraction while Place is in process
        clickLocked = true;

        // Reset to original parent
        grabbedBlock.Reparent(prevParent);

        // Make it invisble on the way to placement location so it doesn't collide with player
        grabbedBlock.CollisionLayer = 0b0;

        // Reference to block just placed
        Block placedBlock = grabbedBlock;

        // Clear Grabbed Block Variables
        grabbedBlock = null; // Block Ref

        cubePositions = null;

        if (placePreview == null)
        {
            GD.PrintErr("No Place Preview even though has held block");
            return;
        }

        // Target Postion is same as the preview position
        Vector3 targetPosition = placePreview.GlobalPosition;

        // Set the rotatin of the placed block (Fix make the rotation gradual as well)
        placedBlock.GlobalRotation = placePreview.GlobalRotation;

        // Remove the Placement Preview
        placePreview.Visible = false;
        placePreview.QueueFree();
        placePreview = null;
        placePreviewMeshes.Clear(); // Clear the Array as well

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

    private void updatePlacePreview()
    {
        // If colliding , get target block and update position to snapped position
        // Ensure Immediate Collision Updates
        bool isSnapping = false;

        placeShapeCast.ForceShapecastUpdate();
        if (
            grabbedBlock != null
            && placeShapeCast.IsColliding()
            && placeShapeCast.GetCollider(0) is Block targetBlock
        )
        {
            isSnapping = true;
            // Calculate new place position
            var collisionPoint = placeShapeCast.GetCollisionPoint(0);

            Vector3 dist = GlobalPosition - collisionPoint;

            // Get Normal and align it to the target block's coordinate system
            // Vector3 normal = alignedNormal(placeShapeCast.GetCollisionNormal(0), targetBlock);
            Vector3 normal = alignedNormal(dist, targetBlock);

            // Normal will be a line from block to player, but rotate it to align to block's roation

            var distanceSquare = GlobalPosition.DistanceSquaredTo(collisionPoint);

            // Rotate to match existing block
            placePreview.GlobalRotation = targetBlock.GlobalRotation;

            // Collision Point Snapped to nearest cube within block
            Vector3 nearestCube = GetNearestCubeCenter(collisionPoint, targetBlock);
            // Instad of targetBlock.GlobalPosition

            if (distanceSquare < distSquareTreshold)
            {
                // Up Place

                placePreview.GlobalPosition =
                    nearestCube + Vector3.Up * targetBlock.getblockLength();
            }
            else
            {
                // Front Place

                // Place position is in the normal direction offset by half a lenght
                placePreview.GlobalPosition = nearestCube + normal * targetBlock.getblockLength();
            }
        }
        else
        {
            // No Target Block, Place Preview still based on offset Position
            placePreview.Position = PlacementOffset;
            // Reset Relative rotation of the mesh (same as it was when picked up)
            placePreview.Rotation = Vector3.Zero;
        }

        // Check if Placing at position with rotation will clip any blocks
        bool wouldCollide = CheckBlockCollision(
            placePreview.GlobalPosition,
            placePreview.GlobalRotation
        );

        if (wouldCollide)
        {
            // Instanceciate grabbed blocked preview mesh at the place Position
            if (placePreview == null)
            {
                GD.Print("Unable to Find Place Preview Mesh");
            }

            setPlacePreviewMaterial(invalidPreviewMaterial);
        }
        else
        {
            // No Collisino
            // Instanceciate grabbed blocked preview mesh at the place Position
            if (placePreview == null)
            {
                GD.Print("Unable to Find Place Preview Mesh");
            }

            if (isSnapping)
            {
                // Adjust the Color
                setPlacePreviewMaterial(snapPreviewMaterial);
            }
            else
            {
                // Adjust the Color
                setPlacePreviewMaterial(defaultPreviewMaterial);
            }
        }

        // Collision preview
    }

    private Vector3 GetNearestCubeCenter(Vector3 collisionPoint, Block targetBlock)
    {
        Vector3[] targetCubeCenters = targetBlock.getCubeCenters();
        Vector3 nearestCenter = targetBlock.GlobalPosition;
        float minDistSq = float.MaxValue;

        Basis targetBasis = targetBlock.GlobalTransform.Basis;

        foreach (Vector3 localCenter in targetCubeCenters)
        {
            // Convert local cube center to global position
            Vector3 globalCenter = targetBlock.GlobalPosition + (targetBasis * localCenter);

            float distSq = collisionPoint.DistanceSquaredTo(globalCenter);
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                nearestCenter = globalCenter;
            }
        }

        return nearestCenter;
    }

    private void setPlacePreviewMaterial(StandardMaterial3D material)
    {
        foreach (var mesh in placePreviewMeshes)
        {
            mesh.MaterialOverride = material;
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
            //characterModel?.Idle();
        }
    }

    // Helper function to align to target cube's faces
    public Vector3 alignedNormal(Vector3 vector, Block targetBlock)
    {
        Basis blockBasis = targetBlock.Transform.Basis;
        float dotX = vector.Dot(blockBasis.X);
        float dotY = vector.Dot(blockBasis.Y);
        float dotZ = vector.Dot(blockBasis.Z);

        float absDotX = Mathf.Abs(dotX);
        float absDotY = Mathf.Abs(dotY);
        float absDotZ = Mathf.Abs(dotZ);

        if (absDotX > absDotY && absDotX > absDotZ)
        {
            return blockBasis.X * (dotX >= 0 ? 1 : -1);
        }
        else if (absDotY > absDotZ)
        {
            return blockBasis.Y * (dotY >= 0 ? 1 : -1);
        }
        else
        {
            return blockBasis.Z * (dotZ >= 0 ? 1 : -1);
        }
    }

    private bool CheckBlockCollision(Vector3 position, Vector3 rotation)
    {
        // if (grabbedBlockColliderShape == null)
        // {
        //     GD.PrintErr("Checking Block Collision without valid block shape");
        //     return true;
        // }

        if (grabbedBlock == null)
        {
            GD.PrintErr("Checking Block collision without grabbed block");
            return true;
        }

        if (cubePositions.IsEmpty())
        {
            GD.PrintErr("Checking Block collision but Cube Positions array is empty");
            return true;
        }

        // Basic cube shape with the current lenght

        // Set the Query Parameters based on block collider
        // How do I create a cube with the blockLength * placementOffset Scale here?
        query.Shape = scaledUnitCubeShape;
        var spaceState = GetWorld3D().DirectSpaceState;
        var basis = Basis.FromEuler(rotation);

        // Query for each of the cubes

        for (int i = 0; i < cubePositions.Length; i++)
        {
            // Get Cube Position accounting for rotation
            var cubePosition = position + (basis * cubePositions[i]);

            // Apply Final Position and Rotation to query shape
            query.Transform = new Transform3D(basis, cubePosition);

            // Check for collision
            var results = spaceState.IntersectShape(query);

            // Consider collisions with block objects that aren't the targetBlock
            foreach (var result in results)
            {
                if (result["collider"].Obj is Block block)
                {
                    return true; // Would collide with another block
                }
            }
        }

        return false; // Safe to place
    }
}
