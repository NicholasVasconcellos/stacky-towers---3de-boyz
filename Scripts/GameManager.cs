using System.Collections.Generic;
using Godot;

public partial struct PlayerConfig
{
    public PlayerConfig(int deviceId = -1, Color playerColor = default, Player player = null)
    {
        DeviceId = deviceId;
        PlayerColor = playerColor == default ? Colors.White : playerColor;
        PlayerInstance = player;
    }

    public int DeviceId { get; set; } // Controller port for the player (0, 1, 2, 3)
    public Color PlayerColor { get; set; }
    public Player PlayerInstance { get; set; } = null;
}

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } // Use to manage overall game state

    public enum GameState
    {
        MainMenu,
        PlayerSetup,
        InGame,
        Paused,
        WinScreen,
    }

    // Set this whenever you change the state of the game
    public GameState CurrentState { get; private set; }
    public const int MaxPlayers = 4;

    //store all the player configs, private set so it can only be changed via GameManager
    public List<PlayerConfig> PlayerConfigs { get; private set; }

    public override void _Ready()
    {
        Instance = this; // set for GoalZone usage when autoloaded
        GD.Print("GameManager ready running");
        // Initialize the list
        PlayerConfigs = new List<PlayerConfig>();

        PlayerConfigs.Add(new PlayerConfig(0, Colors.Red));

        //remove these to make it 1 player
        PlayerConfigs.Add(new PlayerConfig(1, Colors.Blue));
    }

    public override void _Process(double delta) { }

    // Once a proper player setup screen is made, use this function
    public void AddPlayer(int deviceId, Color color, Player playerInstance = null)
    {
        // Add checks here to prevent > 4 players or duplicate device IDs
        PlayerConfigs.Add(new PlayerConfig(deviceId, color, playerInstance));
    }

    public void AddPlayerInstanceToPlayerConfig(int deviceId, Player playerInstance)
    {
        for (int i = 0; i < PlayerConfigs.Count; i++)
        {
            if (PlayerConfigs[i].DeviceId == deviceId)
            {
                PlayerConfigs[i] = new PlayerConfig(
                    deviceId,
                    PlayerConfigs[i].PlayerColor,
                    playerInstance
                );
                return;
            }
        }
    }

    public void GoToMainMenu()
    {
        PlayerConfigs.Clear();
        CurrentState = GameState.MainMenu;
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }

    public void GoToWinScreen()
    {
        GD.Print("Go to win screen called");
        PlayerConfigs.Clear();
        CurrentState = GameState.WinScreen;
        GetTree().ChangeSceneToFile("res://UI/Scenes/WinScreen.tscn");
    }

    public void StartGame()
    {
        CurrentState = GameState.InGame;
        GetTree().ChangeSceneToFile("res://Scenes/Level.tscn");
    }

    //consider moving pause to gamemanger
}
