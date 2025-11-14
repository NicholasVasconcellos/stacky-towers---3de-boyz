using System.Collections.Generic;
using Godot;

public class PlayerConfig
{
    public int DeviceId { get; set; } // Controller port for the player (0, 1, 2, 3)
    public Color PlayerColor { get; set; }
    public int GoalHeight { get; set; } = 10; // Default goal height
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

        PlayerConfigs.Add(
            new PlayerConfig
            {
                DeviceId = 0, // assign this during player setup
                PlayerColor = Colors.Red,
            }
        );

        //remove these to make it 1 player
        PlayerConfigs.Add(new PlayerConfig { DeviceId = 1, PlayerColor = Colors.Blue });
    }

    public override void _Process(double delta) { }

    // Once a proper player setup screen is made, use this function
    public void AddPlayer(int deviceId, Color color)
    {
        // Add checks here to prevent > 4 players or duplicate device IDs
        PlayerConfigs.Add(new PlayerConfig { DeviceId = deviceId, PlayerColor = color });
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
