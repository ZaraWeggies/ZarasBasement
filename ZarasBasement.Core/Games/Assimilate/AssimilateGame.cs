using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Zara_s_Basement.Core.Input;
using Zara_s_Basement.Core.Screens;
using Zara_s_Basement.Core.UI;
using Button = Zara_s_Basement.Core.UI.Button;

namespace Zara_s_Basement.Core.Games.Assimilate;

/// <summary>
/// Assimilate! - A flood-it / drench style puzzle game.
/// Fill the entire board with one color in limited moves!
/// </summary>
public class AssimilateGame : Screen, IMinigame
{
    // Game settings
    private const int BoardWidth = 14;
    private const int BoardHeight = 14;
    private const int ColorCount = 6;
    private const int MaxMoves = 30;
    
    // Game state
    private Board? _board;
    private int _movesTaken;
    private GameState _state = GameState.Playing;
    
    // Stats
    private GameStats _stats;
    
    // Graphics
    private Texture2D? _pixel;
    private SpriteFont? _font;
    
    // UI
    private QuitButton? _quitButton;
    private Button? _restartButton;
    private readonly List<Button> _colorButtons = new();
    
    // Layout
    private Rectangle _boardRect;
    private int _cellSize;
    
    // Animation
    private float _winAnimProgress;
    private float _gameOverAnimProgress;

    /// <inheritdoc />
    public GameInfo Info { get; } = new()
    {
        Id = "assimilate",
        Title = "Assimilate!",
        Description = "Fill the board with one color. Classic flood-it puzzle!",
        ThumbnailPath = "Games/Assimilate/thumbnail",
        Difficulty = 2,
        EstimatedMinutes = 3,
        Tags = new[] { "Arcade", "Strategy", "Classic" }
    };

    /// <inheritdoc />
    public event Action? RequestQuit;

    /// <inheritdoc />
    public event Action<GameStats>? StatsChanged;

    public AssimilateGame()
    {
        _stats = GameStats.CreateNew(Info.Id);
    }

    /// <inheritdoc />
    public GameStats GetStats() => _stats;

    /// <inheritdoc />
    public void SetStats(GameStats stats)
    {
        _stats = stats;
    }

    public override void Initialize()
    {
        base.Initialize();
        StartNewGame();
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);
        
        // Create pixel texture
        _pixel = new Texture2D(GraphicsDevice!, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        // Load font
        _font = content.Load<SpriteFont>("Fonts/Hud");
        
        // Set up UI
        SetupLayout();
        SetupUI();
    }

    private void SetupLayout()
    {
        var viewport = GraphicsDevice!.Viewport;
        
        // Calculate cell size to fit the board nicely
        int maxBoardWidth = viewport.Width - 200; // Leave room for UI
        int maxBoardHeight = viewport.Height - 100; // Leave room for header/footer
        
        _cellSize = Math.Min(maxBoardWidth / BoardWidth, maxBoardHeight / BoardHeight);
        _cellSize = Math.Min(_cellSize, 32); // Cap at 32px
        
        // Center the board
        int boardPixelWidth = BoardWidth * _cellSize;
        int boardPixelHeight = BoardHeight * _cellSize;
        
        _boardRect = new Rectangle(
            (viewport.Width - boardPixelWidth) / 2,
            80,
            boardPixelWidth,
            boardPixelHeight
        );
    }

    private void SetupUI()
    {
        var viewport = GraphicsDevice!.Viewport;
        
        // Quit button
        _quitButton = new QuitButton();
        _quitButton.SetScreenBounds(new Rectangle(0, 0, viewport.Width, viewport.Height));
        _quitButton.QuitRequested += () => RequestQuit?.Invoke();
        
        // Color buttons
        _colorButtons.Clear();
        int buttonSize = 40;
        int buttonSpacing = 10;
        int totalButtonWidth = ColorCount * buttonSize + (ColorCount - 1) * buttonSpacing;
        int buttonStartX = (viewport.Width - totalButtonWidth) / 2;
        int buttonY = _boardRect.Bottom + 20;
        
        for (int i = 0; i < ColorCount; i++)
        {
            var btn = new Button
            {
                Bounds = new Rectangle(
                    buttonStartX + i * (buttonSize + buttonSpacing),
                    buttonY,
                    buttonSize,
                    buttonSize
                ),
                Text = ""
            };
            
            int colorIndex = i; // Capture for closure
            btn.Clicked += () => OnColorClicked(colorIndex);
            _colorButtons.Add(btn);
        }
        
        // Restart button
        _restartButton = new Button
        {
            Bounds = new Rectangle(
                viewport.Width / 2 - 60,
                buttonY + buttonSize + 20,
                120,
                30
            ),
            Text = "NEW GAME"
        };
        _restartButton.Clicked += StartNewGame;
    }

    private void StartNewGame()
    {
        _board = new Board(BoardWidth, BoardHeight, ColorCount);
        _movesTaken = 0;
        _state = GameState.Playing;
        _winAnimProgress = 0;
        _gameOverAnimProgress = 0;
    }

    private void OnColorClicked(int colorIndex)
    {
        if (_state != GameState.Playing || _board == null) return;
        
        // Don't count if clicking current color
        if (colorIndex == _board.CurrentColor) return;
        
        // Perform flood
        _board.Flood(colorIndex);
        _movesTaken++;
        
        // Check win condition
        if (_board.IsComplete())
        {
            _state = GameState.Won;
            RecordWin();
        }
        else if (_movesTaken >= MaxMoves)
        {
            _state = GameState.Lost;
            RecordLoss();
        }
    }

    private void RecordWin()
    {
        _stats.TimesPlayed++;
        _stats.Wins++;
        _stats.LastPlayed = DateTime.UtcNow;
        
        // Score based on remaining moves
        int score = (MaxMoves - _movesTaken + 1) * 100;
        if (score > _stats.HighScore)
        {
            _stats.HighScore = score;
        }
        
        StatsChanged?.Invoke(_stats);
    }

    private void RecordLoss()
    {
        _stats.TimesPlayed++;
        _stats.LastPlayed = DateTime.UtcNow;
        StatsChanged?.Invoke(_stats);
    }

    public override void Update(GameTime gameTime)
    {
        InputHelper.Update();
        
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update animations
        if (_state == GameState.Won)
            _winAnimProgress = Math.Min(_winAnimProgress + delta * 2f, 1f);
        if (_state == GameState.Lost)
            _gameOverAnimProgress = Math.Min(_gameOverAnimProgress + delta * 2f, 1f);
        
        // Update UI
        _quitButton?.Update();
        _restartButton?.Update();
        
        if (_state == GameState.Playing)
        {
            foreach (var btn in _colorButtons)
            {
                btn.Update();
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_pixel == null || _font == null || _board == null || GraphicsDevice == null) return;
        
        var viewport = GraphicsDevice.Viewport;
        
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        // Background
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(30, 30, 40));
        
        // Title
        string title = "ASSIMILATE!";
        var titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2((viewport.Width - titleSize.X) / 2, 20), Color.White);
        
        // Move counter
        string moves = $"Moves: {_movesTaken} / {MaxMoves}";
        var movesColor = _movesTaken > MaxMoves - 5 ? Color.Red : Color.White;
        spriteBatch.DrawString(_font, moves, new Vector2((viewport.Width - _font.MeasureString(moves).X) / 2, 45), movesColor);
        
        // Draw the board
        DrawBoard(spriteBatch);
        
        // Draw color buttons
        DrawColorButtons(spriteBatch);
        
        // Draw restart button
        _restartButton?.Draw(spriteBatch, _pixel, _font);
        
        // Draw quit button
        _quitButton?.Draw(spriteBatch, _pixel, _font);
        
        // Draw win/lose overlay
        if (_state == GameState.Won)
            DrawWinOverlay(spriteBatch, viewport);
        else if (_state == GameState.Lost)
            DrawLoseOverlay(spriteBatch, viewport);
        
        spriteBatch.End();
    }

    private void DrawBoard(SpriteBatch spriteBatch)
    {
        if (_board == null || _pixel == null) return;
        
        // Draw border
        int borderSize = 3;
        spriteBatch.Draw(_pixel, new Rectangle(
            _boardRect.X - borderSize,
            _boardRect.Y - borderSize,
            _boardRect.Width + borderSize * 2,
            _boardRect.Height + borderSize * 2
        ), new Color(60, 60, 70));
        
        // Draw cells
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                var cellRect = new Rectangle(
                    _boardRect.X + x * _cellSize,
                    _boardRect.Y + y * _cellSize,
                    _cellSize,
                    _cellSize
                );
                
                Color cellColor = _board.GetColor(x, y);
                
                // Highlight connected cells on win
                if (_state == GameState.Won)
                {
                    float pulse = (float)Math.Sin(_winAnimProgress * Math.PI * 4 + x * 0.2f + y * 0.2f);
                    cellColor = Color.Lerp(cellColor, Color.White, pulse * 0.3f * _winAnimProgress);
                }
                
                spriteBatch.Draw(_pixel, cellRect, cellColor);
            }
        }
        
        // Draw completion percentage
        float completion = _board.GetCompletionPercentage();
        string compText = $"{completion:P0}";
        var compSize = _font!.MeasureString(compText);
        spriteBatch.DrawString(_font, compText, 
            new Vector2(_boardRect.Right + 10, _boardRect.Y), 
            Color.Gray);
    }

    private void DrawColorButtons(SpriteBatch spriteBatch)
    {
        if (_pixel == null || _board == null) return;
        
        for (int i = 0; i < _colorButtons.Count; i++)
        {
            var btn = _colorButtons[i];
            var color = Board.Colors[i];
            var bounds = btn.Bounds;
            
            bool isCurrent = i == _board.CurrentColor;
            
            // Draw thin border
            int border = 2;
            spriteBatch.Draw(_pixel, new Rectangle(
                bounds.X - border, bounds.Y - border,
                bounds.Width + border * 2, bounds.Height + border * 2
            ), isCurrent ? Color.White : new Color(60, 60, 70));
            
            // Fill with solid color (dimmed if current)
            var displayColor = isCurrent ? color : color;
            spriteBatch.Draw(_pixel, bounds, displayColor);
            
            // Hover effect - slight brighten
            // if (btn.IsHovered && _state == GameState.Playing && !isCurrent)
            // {
            //     spriteBatch.Draw(_pixel, bounds, Color.White * 0.15f);
            // }
        }
    }

    private void DrawWinOverlay(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_pixel == null || _font == null) return;
        
        // Semi-transparent overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), 
            Color.Black * (0.5f * _winAnimProgress));
        
        // Win text
        string winText = "YOU WIN!";
        var winSize = _font.MeasureString(winText);
        float scale = 1f + _winAnimProgress * 0.5f;
        var winPos = new Vector2(
            viewport.Width / 2 - winSize.X * scale / 2,
            viewport.Height / 2 - winSize.Y * scale / 2 - 40
        );
        
        spriteBatch.DrawString(_font, winText, winPos + new Vector2(2, 2), Color.Black * _winAnimProgress, 
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, winText, winPos, Color.Gold * _winAnimProgress, 
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        
        // Score
        int score = (MaxMoves - _movesTaken + 1) * 100;
        string scoreText = $"Score: {score}";
        var scoreSize = _font.MeasureString(scoreText);
        var scorePos = new Vector2(viewport.Width / 2 - scoreSize.X / 2, viewport.Height / 2);
        spriteBatch.DrawString(_font, scoreText, scorePos, Color.White * _winAnimProgress);
        
        // Moves remaining
        string movesText = $"Moves remaining: {MaxMoves - _movesTaken}";
        var movesSize = _font.MeasureString(movesText);
        var movesPos = new Vector2(viewport.Width / 2 - movesSize.X / 2, viewport.Height / 2 + 25);
        spriteBatch.DrawString(_font, movesText, movesPos, Color.LightGray * _winAnimProgress);
    }

    private void DrawLoseOverlay(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_pixel == null || _font == null) return;
        
        // Semi-transparent overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), 
            Color.Black * (0.6f * _gameOverAnimProgress));
        
        // Lose text
        string loseText = "OUT OF MOVES!";
        var loseSize = _font.MeasureString(loseText);
        var losePos = new Vector2(
            viewport.Width / 2 - loseSize.X / 2,
            viewport.Height / 2 - loseSize.Y / 2 - 20
        );
        
        spriteBatch.DrawString(_font, loseText, losePos + new Vector2(2, 2), Color.Black * _gameOverAnimProgress);
        spriteBatch.DrawString(_font, loseText, losePos, Color.Red * _gameOverAnimProgress);
        
        // Completion percentage
        string compText = $"Board filled: {_board!.GetCompletionPercentage():P0}";
        var compSize = _font.MeasureString(compText);
        var compPos = new Vector2(viewport.Width / 2 - compSize.X / 2, viewport.Height / 2 + 10);
        spriteBatch.DrawString(_font, compText, compPos, Color.Gray * _gameOverAnimProgress);
    }

    public override void UnloadContent()
    {
        _pixel?.Dispose();
        base.UnloadContent();
    }

    private enum GameState
    {
        Playing,
        Won,
        Lost
    }
}
