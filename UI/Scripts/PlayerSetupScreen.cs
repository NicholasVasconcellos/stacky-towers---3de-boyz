using Godot;
using System.Collections.Generic;

public partial class PlayerSetupScreen : Control
{
    [Export] private Godot.Collections.Array<PlayerCard> _playerCards;
    [Export] private Button _startButton;

    private GameManager _gameManager;

    // predefined colors for players 1-4
    private readonly Color[] _playerColors = new Color[] 
    { 
        Colors.Red, 
        Colors.Blue, 
        Colors.Green, 
        Colors.Yellow 
    };

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        
        // 1. Clear any existing players from previous sessions
        _gameManager.ResetPlayers();

        // 2. Auto-Add Player 1 (Keyboard/Mouse) as Device -1
        JoinPlayer(-1);

        // 3. Update the Start Button state
        UpdateStartButton();
    }

    public override void _Input(InputEvent @event)
    {
        // Listen for Controller Button Presses
        if (@event is InputEventJoypadButton joyBtn && joyBtn.Pressed)
        {
            // Check for "A" button (Xbox) or "Cross" (PS)
            if (joyBtn.ButtonIndex == JoyButton.A)
            {
                AttemptJoin(joyBtn.Device);
            }
        }
    }

    private void AttemptJoin(int deviceId)
    {
        // Check if this device is already registered
        foreach (var config in _gameManager.PlayerConfigs)
        {
            if (config.DeviceId == deviceId) return; // Already joined!
        }

        // Check if lobby is full
        if (_gameManager.PlayerConfigs.Count >= GameManager.MaxPlayers) return;

        // Success: Join the player
        JoinPlayer(deviceId);
    }

    private void JoinPlayer(int deviceId)
    {
        int newPlayerIndex = _gameManager.PlayerConfigs.Count;
        Color newColor = _playerColors[newPlayerIndex % _playerColors.Length];

        // Add to Game Manager
        _gameManager.AddPlayer(deviceId, newColor);

        // Update the visual Card
        UpdateUI();
    }

    private void UpdateUI()
    {
        var players = _gameManager.PlayerConfigs;

        for (int i = 0; i < _playerCards.Count; i++)
        {
            PlayerCard card = _playerCards[i];

            if (i < players.Count)
            {
                // Slot is taken
                card.PlayerNumber = i + 1;
                card.State = PlayerCard.PlayerState.Joined;
                card.JoinedColor = players[i].PlayerColor;
                
                // Show Input Type text
                string inputType = players[i].DeviceId == -1 ? "KEYBOARD" : $"CONTROLLER {players[i].DeviceId}";
                card.JoinedText = "READY\n" + inputType;
            }
            else
            {
                // Slot is empty
                card.PlayerNumber = i + 1;
                card.State = PlayerCard.PlayerState.Waiting;
            }

            // Force the card to redraw its visuals
            card.UpdateCardVisuals();
        }

        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        // Only allow start if we have at least 2 players? (Or 1 if we support solo)
        bool canStart = _gameManager.PlayerConfigs.Count >= 1; 
        _startButton.Disabled = !canStart;
        
        if (canStart) _startButton.Text = "START GAME";
        else _startButton.Text = "WAITING FOR PLAYERS...";
    }

    public void OnStartButton_Pressed()
    {
        // Only start if ready
        if (!_startButton.Disabled)
        {
            _gameManager.StartGame();
        }
    }

    public void OnBackButton_Pressed()
    {
        _gameManager.GoToMainMenu();
    }
}