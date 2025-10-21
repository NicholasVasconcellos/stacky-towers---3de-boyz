using Godot;
using System;

public partial class OptionsMenuFromMainMenu : Control
{
	public void OnBackButton_Pressed()
	{
		GetTree().ChangeSceneToFile("res://UI/Scenes/main_menu.tscn");
	}
}
