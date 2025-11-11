using Godot;
using System;

public partial class UIManager : CanvasLayer
{
	private Control _pauseMenu;
	private Control _optionsMenu;
	private TimerLabel _timerLabel;
	private WinScreen _winScreen;
	private CenterContainer _towerHeightContainer;

	private bool _isGameOver = false;
	
	public override void _Ready()
	{
		_pauseMenu = GetNode<Control>("PauseMenu");
		_optionsMenu = GetNode<Control>("OptionsMenu");
		_timerLabel = GetNode<TimerLabel>("TimerLabel");
		_winScreen = GetNode<WinScreen>("WinScreen");
		_towerHeightContainer = GetNode<CenterContainer>("TowerHeightContainer");
		
		_pauseMenu.ProcessMode = ProcessModeEnum.Always;
		_optionsMenu.ProcessMode = ProcessModeEnum.Always;
		ProcessMode =  ProcessModeEnum.Always;
		
		_pauseMenu.Hide();
		_optionsMenu.Hide();
		_winScreen.Hide();

		_timerLabel.TimeUp += _OnTimerTimeUp;
		
		_winScreen.PlayAgain += OnWinScreenPlayAgain;
		_winScreen.MainMenu += OnWinScreenMainMenu;
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
		
		_isGameOver = true;
		GetTree().Paused = true;
		
		_pauseMenu.Hide();
		_optionsMenu.Hide();
		_towerHeightContainer.Hide();
		_timerLabel.Hide();
		
		// [TODO] Connect win logic to here
		_winScreen.SetWinnerText("Player 1 Wins!");

		_winScreen.Show();
	}

	private void OnWinScreenPlayAgain()
	{
		// Reload the current level
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnWinScreenMainMenu()
	{
		// Load the main menu scene
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://UI/Scenes/main_menu.tscn");
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
			_towerHeightContainer.Visible = true;
		}
		else
		{
			GetTree().Paused = true;
			_pauseMenu.Show();
			_towerHeightContainer.Visible = false;
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
