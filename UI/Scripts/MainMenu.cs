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
		GD.Print("Settings pressed");
	}

	public void OnExitPressed()
	{
		GetTree().Quit();
	}
}