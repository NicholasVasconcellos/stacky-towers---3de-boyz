using Godot;
using System;

public partial class PlayerSetupScreen : Control
{
	private GameManager gameManager;

	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
	}

	public void OnStartButton_Pressed()
	{
		gameManager.StartGame();
	}
	
	public void OnBackButton_Pressed()
	{
		GetTree().ChangeSceneToFile("res://UI/Scenes/main_menu.tscn");
	}
}
