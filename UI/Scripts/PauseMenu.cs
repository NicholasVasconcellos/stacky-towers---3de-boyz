using Godot;
using System;

public partial class PauseMenu : Control
{
	private UIManager _uiManager;

	public override void _Ready()
	{
		_uiManager = GetParent<UIManager>();
		
		if (_uiManager == null)
		{
			GD.PrintErr("Error: Could not find and cast the parent CanvasLayer to type 'UIManager'!");
		}
	}
	
	public void OnResumeButton_Pressed()
	{
		_uiManager.TogglePause();
	}
	
	public void OnSettingsButton_Pressed()
	{
		_uiManager.ShowOptionsMenu();
	}

	public void OnQuitButton_Pressed()
	{
		GetTree().Quit();
	}
}
