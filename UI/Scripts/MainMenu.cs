using Godot;

namespace StackyTowers3DEBoyz.UI.Scripts;

public partial class MainMenu : Control
{
	public void OnStartPressed()
	{
		GetTree().ChangeSceneToFile("res://Level.tscn");
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