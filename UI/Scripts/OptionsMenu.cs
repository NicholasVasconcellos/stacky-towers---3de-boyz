using Godot;
using System;

public partial class OptionsMenu : Control
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
	
	public void OnBackButton_Pressed()
	{
		_uiManager.ShowPauseMenu();
	}
}
