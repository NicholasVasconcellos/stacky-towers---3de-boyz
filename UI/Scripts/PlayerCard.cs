using Godot;
using System;

public partial class PlayerCard : PanelContainer
{
	public enum PlayerState
	{
		Waiting,
		Joined
	}

	[Export] 
	public int PlayerNumber { get; set; } = 1;

	[Export]
	public PlayerState State { get; set; } = PlayerState.Waiting;

	[Export]
	public Color JoinedColor { get; set; } = new Color("#60d394"); // Green
	
	[Export]
	public Color WaitingColor { get; set; } = new Color("#555555"); // Gray

	[Export]
	public string JoinedText { get; set; } = "READY";
	
	[Export]
	public string WaitingText { get; set; } = "Press A to Join";

	private Label _title;
	private Label _status;
	private TextureRect _icon;
	
	public override void _Ready()
	{
		_title = GetNode<Label>("VBoxContainer/Title");
		_status = GetNode<Label>("VBoxContainer/Status");
		_icon = GetNode<TextureRect>("VBoxContainer/Icon");

		UpdateCardVisuals();
	}

	public void UpdateCardVisuals()
	{
		_title.Text = $"PLAYER {PlayerNumber}";

		if (State == PlayerState.Joined)
		{
			_status.Text = JoinedText;
			_icon.Modulate = JoinedColor;
		}
		else
		{
			_status.Text = WaitingText;
			_icon.Modulate = WaitingColor;
		}
	}
}
