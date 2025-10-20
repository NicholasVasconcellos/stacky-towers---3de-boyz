using Godot;
using System;

public partial class Level : Node3D
{
    // Make sure to assign the PauseMenu node in the Godot Inspector
    [Export]
    private Control PauseMenu;
    
    public override void _Ready()
    {
        // Set "Process Mode" to "Always" so it can still function when GetTree().Paused is true.
        PauseMenu.ProcessMode = ProcessModeEnum.Always;
        PauseMenu.Hide();
    }

    /// <summary>
    /// Toggles the game's official pause state and shows/hides the pause menu.
    /// </summary>
    public void TogglePause()
    {
        bool isCurrentlyPaused = GetTree().Paused;

        if (isCurrentlyPaused)
        {
            GetTree().Paused = false;
            PauseMenu.Hide();
        }
        else
        {
            GetTree().Paused = true;
            PauseMenu.Show();
        }
        
        // Note: Engine.TimeScale is not needed here, as GetTree().Paused 
        // handles freezing all nodes whose Process Mode is set to 'Inherit' (the default).
    }
}