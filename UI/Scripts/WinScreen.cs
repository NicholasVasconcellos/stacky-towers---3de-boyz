using Godot;
using System;

public partial class WinScreen : Control
{
	// Define signals to tell the UIManager what to do
	[Signal]
	public delegate void PlayAgainEventHandler();
	[Signal]
	public delegate void MainMenuEventHandler();

	private Label winnerText;

	private Button playAgainButton;

	private Button mainMenuButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		
		winnerText = GetNode<Label>("CenterContainer/VBoxContainer/WinnerText");
		playAgainButton = GetNode<Button>("CenterContainer/VBoxContainer/PlayAgainButton");
		mainMenuButton = GetNode<Button>("CenterContainer/VBoxContainer/MainMenuButton");
		
		// Connect the button signals to our local functions
		playAgainButton.Pressed += OnPlayAgain_Pressed;
		mainMenuButton.Pressed += OnMainMenu_Pressed;
	}

	/// <summary>
	/// This function can be called from the UIManager to set the text.
	/// </summary>
	/// <param name="text"></param>
	public void SetWinnerText(string text)
	{
		winnerText.Text = text;
	}

	private void OnPlayAgain_Pressed()
	{
		GetTree().Paused = false;
		GameManager.Instance.StartGame();
	}

	private void OnMainMenu_Pressed()
	{
		GetTree().Paused = false;
		GameManager.Instance.GoToMainMenu();
	}
}
