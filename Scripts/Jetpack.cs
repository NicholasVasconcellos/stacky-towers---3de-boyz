using Godot;
using System;

public partial class Jetpack : Node3D
{
	// Node references
	private GpuParticles3D _particles;
	private AudioStreamPlayer3D _sound;
	
	public override void _Ready()
	{
		// Get the nodes
		_particles = GetNode<GpuParticles3D>("ThrusterParticles");
		_sound = GetNode<AudioStreamPlayer3D>("ThrusterSound");
	}

	/// <summary>
	/// Turns on all jetpack visual and audio effects.
	/// </summary>
	public void StartEffects()
	{
		if (!_particles.Emitting)
		{
			_particles.Emitting = true;
		}

		if (!_sound.IsPlaying())
		{
			_sound.Play();
		}
	}

	/// <summary>
	/// Turns off all jetpack visual and audio effects.
	/// </summary>
	public void StopEffects()
	{
		if (_particles.Emitting)
		{
			_particles.Emitting = false;
		}

		if (_sound.IsPlaying())
		{
			_sound.Stop();
		}
	}
}
