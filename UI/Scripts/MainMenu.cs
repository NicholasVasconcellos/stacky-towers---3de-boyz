using Godot;

namespace StackyTowers3DEBoyz.UI.Scripts;

public partial class MainMenu : Control
{
    private GameManager gameManager;

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/GameManager");
    }

    public void OnStartPressed()
    {
        gameManager.StartGame();
    }

    public void OnSettingsPressed()
    {
        GetTree().ChangeSceneToFile("res://UI/Scenes/OptionsMenuFromMainMenu.tscn");
    }

    public void OnExitPressed()
    {
        GetTree().Quit();
    }
}
