using Godot;
using System;

public partial class PlayerHUD : Control
{
	private TextureProgressBar _fuelBar;
	
	public override void _Ready()
	{
		_fuelBar = GetNode<TextureProgressBar>("FuelBar");
	}

	public void OnUpdateFuel(double currentFuel, double MaxFuel)
	{
		_fuelBar.MaxValue = MaxFuel;
		_fuelBar.Value = currentFuel;

		// Visible = currentFuel < MaxFuel;
	}
	
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed)
		{
			GD.Print($"\nThe click was caught by: {Name}\n");
		}
	}
}
