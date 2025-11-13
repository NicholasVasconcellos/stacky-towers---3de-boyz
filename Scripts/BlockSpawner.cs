using System;
using Godot;

public partial class BlockSpawner : Node3D
{
    [Export]
    public PackedScene BlockScene;

    [Export]
    public float BlocksPerSecond = 2.0f;

    [Export]
    public Vector3 SpawnRange = new Vector3(10, 0, 10);

    [Export]
    public Vector3 SpawnCenter = new Vector3(0, 10, 0);

    private Random random = new Random();
    private float timer = 0f;
    private float spawnInterval;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        spawnInterval = 1.0f / BlocksPerSecond;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (BlockScene == null)
        {
            return;
        }

        timer += (float)delta;

        if (timer >= spawnInterval)
        {
            SpawnBlock();
            // Reset timer
            timer = 0f;
        }
    }

    private void SpawnBlock()
    {
        // Add an instance of BlockScene
        var block = (Block)BlockScene.Instantiate();

        // Get Rand Float from 0 to 1
        // Mult by the range
        // Subtract by half the range to centralize
        var randPosition = new Vector3(
            (float)(random.NextDouble() * SpawnRange.X - SpawnRange.X / 2),
            (float)(random.NextDouble() * SpawnRange.Y - SpawnRange.Y / 2),
            (float)(random.NextDouble() * SpawnRange.Z - SpawnRange.Z / 2)
        );

        // Set position
        block.Position = SpawnCenter + randPosition;
        // Add this as child
        AddChild(block);
    }
}
