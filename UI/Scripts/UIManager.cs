using Godot;
using System;

public partial class UIManager : CanvasLayer
{
	private Control _pauseMenu;
	private Control _optionsMenu;
	private TimerLabel _timerLabel;

	private bool _isGameOver = false;
	
	public override void _Ready()
	{
		_pauseMenu = GetNode<Control>("PauseMenu");
		_optionsMenu = GetNode<Control>("OptionsMenu");
		_timerLabel = GetNode<TimerLabel>("TimerLabel");
		
		_pauseMenu.ProcessMode = ProcessModeEnum.Always;
		_optionsMenu.ProcessMode = ProcessModeEnum.Always;
		ProcessMode =  ProcessModeEnum.Always;
		
		_pauseMenu.Hide();
		_optionsMenu.Hide();

		_timerLabel.TimeUp += _OnTimerTimeUp;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (_isGameOver) return;
			if (_optionsMenu.IsVisibleInTree())
			{
				ShowPauseMenu();
			}
			else
			{
				TogglePause();
			}
		}
	}

	/// <summary>
	/// This function is called when the TimerLabel's "TimeUp" signal is emitted.
	/// </summary>
	private void _OnTimerTimeUp()
	{
		GD.Print("UIManager heard that the time is up!");
		
		TogglePause();
		_isGameOver = true;
	}

	/// <summary>
	/// Toggles the game's official pause state and shows/hides the pause menu.
	/// </summary>
	public void TogglePause()
	{
		if (_isGameOver) return;
		
		bool isCurrentlyPaused = GetTree().Paused;

		_optionsMenu.Hide();
		
		if (isCurrentlyPaused)
		{
			GetTree().Paused = false;
			_pauseMenu.Hide();
		}
		else
		{
			GetTree().Paused = true;
			_pauseMenu.Show();
		}
	}

	/// <summary>
	/// Called when 'Settings' is clicked on the Pause Menu
	/// </summary>
	public void ShowOptionsMenu()
	{
		_pauseMenu.Hide();
		_optionsMenu.Show();
	}

	/// <summary>
	/// Called when 'Back' is clicked on the Options Menu.
	/// </summary>
	public void ShowPauseMenu()
	{
		_optionsMenu.Hide();
		_pauseMenu.Show();
	}
}
