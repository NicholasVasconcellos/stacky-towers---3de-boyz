using Godot;
using System;

public partial class TimerLabel : Label
{
	[Signal]
	public delegate void TimeUpEventHandler();

	[Export] public double StartTimeInMinutes { get; set; } = 10.0;

	private double _timeRemaining;
	private bool _isFinished = false;
	
	public override void _Ready()
	{
		_timeRemaining = StartTimeInMinutes * 60.0;
		UpdateTimeDisplay();
	}

	public override void _Process(double delta)
	{
		bool isPaused = GetTree().IsPaused();
		if (isPaused || _isFinished)
		{
			return;
		}
		
		_timeRemaining -= delta;

		if (_timeRemaining <= 0.0)
		{
			_isFinished = true;
			_timeRemaining = 0.0;

			EmitSignal(SignalName.TimeUp);
		}
		
		UpdateTimeDisplay();
	}

	private void UpdateTimeDisplay()
	{
		TimeSpan time = TimeSpan.FromSeconds(_timeRemaining);
		Text = time.ToString(@"mm\:ss");
	}

	public void Reset()
	{
		_timeRemaining = StartTimeInMinutes * 60.0;
		_isFinished = false;
		UpdateTimeDisplay();
	}

	public bool IsFinished()
	{
		return _isFinished;
	}
}
