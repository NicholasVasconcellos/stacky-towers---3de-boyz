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

    private GameManager gameManager;
    private World3D mainWorld;

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
        foreach (PlayerConfig config in players)
        {
            GD.Print($"--- Loop start for Device {config.DeviceId} ---");

            // Create subviewport container for each player
            var viewportContainer = new SubViewportContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                Stretch = true,
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

            // Initialize Player
            playerInstance.Initialize(
                config.DeviceId,
                config.PlayerColor,
                xOffset: config.DeviceId * 4,
                zOffset: 0
            );

            // add player to viewport and add viewport to container
            subViewport.AddChild(playerInstance);
            viewportContainer.AddChild(subViewport);

            if (SplitScreenContainer == null)
            {
                GD.PrintErr(
                    "Issue: SplitScreenContainer is NULL. Assign 'GridContainer' node into the 'Split Screen Container' slot in the Level scene Inspector."
                );
                return; // Stop the function
            }
            SplitScreenContainer.AddChild(viewportContainer);
            GD.Print($"--- Loop finished for Device {config.DeviceId} ---");
        }
    }
}
