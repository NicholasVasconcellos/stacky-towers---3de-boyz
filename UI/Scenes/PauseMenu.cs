using Godot;
using System;

public partial class PauseMenu : Control
{
	// Declare a private field to hold the reference
	private Level _currentLevel; 

	public override void _Ready()
	{
		// PauseMenu should be a child of the Level node above it
		_currentLevel = GetNode<Level>("..");

		if (_currentLevel == null)
		{
			GD.PrintErr("Error: Could not find and cast the parent node to type 'Level'!");
		}
	}
	
	public override void _Process(double delta)
	{
		// Use ui_cancel for Escape key
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			_currentLevel.TogglePause();
		}
	}
	
	public void OnResumeButton_Pressed()
	{
		_currentLevel.TogglePause();
	}
	
	public void OnSettingsButton_Pressed()
	{
		GD.Print("Settings pressed");
	}

	public void OnQuitButton_Pressed()
	{
		GetTree().Quit();
	}
}
