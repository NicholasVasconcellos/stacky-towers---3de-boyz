using System;
using Godot;

public partial class BlockSpawner : Node3D
{
    [Export]
    public PackedScene[] BlockScenes; // Array of block types

    [Export]
    public float BlocksPerSecond = 2.0f;

    [Export]
    public Vector3 SpawnRange = new Vector3(10, 0, 10);

    [Export]
    public Vector3 SpawnCenter = new Vector3(0, 10, 0);

    [Export]
    private int maxBlocks = 50;

    private Random random = new Random();
    private float timer = 0f;
    private float spawnInterval;

    private int count;

    public override void _Ready()
    {
        spawnInterval = 1.0f / BlocksPerSecond;
    }

    public override void _Process(double delta)
    {
        if (BlockScenes == null || BlockScenes.Length == 0)
        {
            return;
        }

        timer += (float)delta;

        if (count >= maxBlocks)
        {
            return;
        }

        if (timer >= spawnInterval)
        {
            SpawnBlock();
            count++;
            timer = 0f;
        }
    }

    private void SpawnBlock()
    {
        // Pick a random block type
        int index = random.Next(BlockScenes.Length);
        PackedScene selectedScene = BlockScenes[index];

        if (selectedScene == null)
        {
            GD.PrintErr($"BlockScene at index {index} is null");
            return;
        }

        var block = (Block)selectedScene.Instantiate();

        var randPosition = new Vector3(
            (float)(random.NextDouble() * SpawnRange.X - SpawnRange.X / 2),
            (float)(random.NextDouble() * SpawnRange.Y - SpawnRange.Y / 2),
            (float)(random.NextDouble() * SpawnRange.Z - SpawnRange.Z / 2)
        );

        block.Position = SpawnCenter + randPosition;
        AddChild(block);
    }
}
