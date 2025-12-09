using System;
using Godot;

public partial class HeightTick : Control
{
    [Export]
    public string TickText { get; set; } = "0m";

    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("TickLabel");
        _label.Text = TickText;
    }

    public void UpdateTickText(string newText)
    {
        TickText = newText;
        if (_label != null)
        {
            _label.Text = TickText;
        }
    }
}
