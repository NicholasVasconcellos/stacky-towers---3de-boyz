using System.Collections.Generic;
using Godot;

public class PlayerConfig
{
    public int DeviceId { get; set; } // Controller port for the player (0, 1, 2, 3)
    public Color PlayerColor { get; set; }
}

public partial class GameManager : Node
{
    // Use to manage overall game state
    public enum GameState
    {
        MainMenu,
        PlayerSetup,
        InGame,
        Paused,
        ResultsScreen,
    }

    // Set this whenever you change the state of the game
    public GameState CurrentState { get; private set; }
    public const int MaxPlayers = 4;

    //store all the player configs, private set so it can only be changed via GameManager
    public List<PlayerConfig> PlayerConfigs { get; private set; }

    public override void _Ready()
    {
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
        // PlayerConfigs.Add(new PlayerConfig { DeviceId = 1, PlayerColor = Colors.Blue });
        // PlayerConfigs.Add(
        //     new PlayerConfig
        //     {
        //         DeviceId = 0, // assign this during player setup
        //         PlayerColor = Colors.Red,
        //     }
        // );
    }

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
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }

    public void StartGame()
    {
        CurrentState = GameState.InGame;
        GetTree().ChangeSceneToFile("res://Level.tscn");
    }

    //consider moving pause to gamemanger
}
