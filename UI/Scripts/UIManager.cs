using Godot;
using System;
using System.Threading.Tasks;

public partial class UIManager : CanvasLayer
{
	private Control _pauseMenu;
	private Control _optionsMenu;
	private TimerLabel _timerLabel;
	private WinScreen _winScreen;
	private CenterContainer _towerHeightContainer;
	// [Export] private PackedScene _playerHudPrefab;
	// [Export] private Control _hudContainer;

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
		// [TODO] Connect win logic (highest tower wins) to here
		RunWinSequence("Time's Up!");
	}

	/// <summary>
	/// Called manually by GoalZone when a player reaches the top.
	/// </summary>
	/// <param name="winnerName"></param>
	public void TriggerWinSequence(string winnerName)
	{
		RunWinSequence(winnerName);
	}

	/// <summary>
	/// The shared logic that handles freezing, camera panning, and UI showing.
	/// </summary>
	/// <param name="resultText"></param>
	public async void RunWinSequence(string resultText)
	{
		if (_isGameOver) return;
		_isGameOver = true;
		
		GD.Print($"Game Over Sequence Started. Result: {resultText}");
		
		_timerLabel.SetProcess(false);
		_timerLabel.Hide();
		_towerHeightContainer.Hide();
		
		GetTree().CallGroup("Players", "SetInputEnabled", false);

		GD.Print("Camera Panning...");
		// [TODO] Camera pan over tower here
		// For now, we just wait 2 seconds to simulate the panning time.
		// Later, replace this with: await CameraController.PanToTowers();
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		
		GD.Print("Sequence finished. Showing Win Screen.");
		_winScreen.SetWinnerText(resultText);
		_winScreen.Show();
		
		GetTree().Paused = true;
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
	//
	// public void CreateHUDForPlayer(Player player)
	// {
	// 	PlayerHUD newHud = _playerHudPrefab.Instantiate<PlayerHUD>();
	// 	_hudContainer.AddChild(newHud);
	//
	// 	player.FuelUpdated += newHud.OnUpdateFuel;
	// 	newHud.Modulate = player.PlayerColor;
	// 	
	// 	newHud.OnUpdateFuel(player.MaxJetpackFuel, player.MaxJetpackFuel);
	// }
}
