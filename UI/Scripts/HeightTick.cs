using Godot;
using System;

public partial class HeightTick : Control
{
	[Export] public string TickText { get; set; } = "0m";

	private Label _label;
	
	public override void _Ready()
	{
		_label = GetNode<Label>("TickLabel");
		_label.Text = TickText;
	}
}
