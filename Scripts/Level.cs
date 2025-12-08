using System.Collections.Generic;
using Godot;

public partial class Level : Node3D
{
    //in the editor, drag the player scene into there
    [Export]
    private PackedScene PlayerScene;

    // In the editor, drag the GridContainer node here
    [Export]
    private GridContainer SplitScreenContainer;

    [Export]
    public UIManager UIManager;

    private GameManager gameManager;
    private World3D mainWorld;
    
    private CameraController _player0Camera;

    public override void _Ready()
    {
        GD.Print("Level _Ready() called.");
        // Set "Process Mode" to "Always" so it can still function when GetTree().Paused is true.

        //get an instance of the game manager
        gameManager = GetNode<GameManager>("/root/GameManager");
        mainWorld = GetWorld3D();

        //set up players
        List<PlayerConfig> players = gameManager.PlayerConfigs;

        GD.Print($"GameScene loaded. Found {players.Count} player(s) in GameManager.");
        if (players.Count == 0)
        {
            GD.PrintErr("GameScene loaded with 0 players, going back to main menu.");
            gameManager.GoToMainMenu(); // Go back to menu
            return;
        }

        SplitScreenContainer.Columns = (players.Count > 1) ? 2 : 1;
        // Loop through all the playerconfigs and create each player + viewport
        // https://www.gdquest.com/library/split_screen_coop/
        for (int i = 0; i < players.Count; i++)
        {
            PlayerConfig config = players[i];
            GD.Print($"--- Loop start for Device {config.DeviceId} ---");

            // Create subviewport container for each player
            var viewportContainer = new SubViewportContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                Stretch = true,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            // Create viewport for each player
            var subViewport = new SubViewport { HandleInputLocally = false, World3D = mainWorld };

            // Get player scene to initialize
            if (PlayerScene == null)
            {
                GD.PrintErr(
                    "Issue: PlayerScene is NULL. Assign 'Player.tscn' to the 'Player Scene' slot in the Level scene Inspector."
                );
                return; // Stop the function
            }
            Player playerInstance = PlayerScene.Instantiate<Player>();
            gameManager.AddPlayerInstanceToPlayerConfig(config.DeviceId, playerInstance);

            // Log Collision Layer and Mask
            GD.Print($"Player {config.DeviceId} - CollisionLayer: {playerInstance.CollisionLayer}");
            GD.Print($"Player {config.DeviceId} - CollisionMask: {playerInstance.CollisionMask}");

            // Add player to viewport and add viewport to container
            subViewport.AddChild(playerInstance);
            viewportContainer.AddChild(subViewport);

            // Update UI with Goal Height
            //var goalZone = GetNode<GoalZone>("GoalZone");
            //var ui = GetNode<UIManager>("UIManager");
            //ui.UpdateHeight(goalZone.ZoneHeight);

            // Initialize Player
            playerInstance.Initialize(
                config.DeviceId,
                config.PlayerColor,
                xOffset: config.DeviceId * 4,
                // Add Height so Player doesn't fall below ground
                zOffset: 3
            );

            if (SplitScreenContainer == null)
            {
                GD.PrintErr(
                    "Issue: SplitScreenContainer is NULL. Assign 'GridContainer' node into the 'Split Screen Container' slot in the Level scene Inspector."
                );
                return; // Stop the function
            }
            
            if (config.DeviceId == -1)
            {
                // Make sure your Camera in the Player scene is actually named "CameraController"
                _player0Camera = playerInstance.GetNodeOrNull<CameraController>("CameraController");
                if (_player0Camera == null) GD.PrintErr("Level could not find CameraController on Player 0!");
            }
            SplitScreenContainer.AddChild(viewportContainer);
            GD.Print($"--- Loop finished for Device {config.DeviceId} ---");
        }
    }
    
    // This catches mouse movement globally, before the Viewports can mess it up.
    public override void _Input(InputEvent @event)
    {
        // Only process if Mouse is Captured (Gameplay Mode)
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;

        // If it's mouse motion...
        if (@event is InputEventMouseMotion motion)
        {
            // ...feed it directly to Player 0's camera
            if (_player0Camera != null)
            {
                _player0Camera.ManualRotate(motion.Relative);
            }
        }
        else if (@event.IsActionPressed("kb_grab"))
        {
            var player0 = _player0Camera?.GetParentOrNull<Player>();
        
            if (player0 != null)
            {
                player0.TryGrab();
            }
        }
    }
}
