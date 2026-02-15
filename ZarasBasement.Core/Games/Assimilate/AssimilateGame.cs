using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Zara_s_Basement.Core.Input;
using Zara_s_Basement.Core.Screens;
using Zara_s_Basement.Core.UI;

namespace Zara_s_Basement.Core.Games.Assimilate;

/// <summary>
/// Assimilate! - A flood-it / drench style puzzle game.
/// Fill the entire board with one color in limited moves!
/// Ported from original HTML5 version.
/// </summary>
public class AssimilateGame : Screen, IMinigame
{
    // Game settings (matching original)
    private const int GridSize = 14;
    private const int ColorCount = 6;
    private const int BaseMaxMoves = 30;
    
    // Game state
    private Board? _board;
    private int _moves;           // Moves remaining
    private int _level;           // Current level (0-indexed)
    private int _points;          // Accumulated points
    private GameState _state = GameState.Playing;
    
    // Stats for hub integration
    private GameStats _stats;
    
    // Graphics
    private Texture2D? _pixel;
    private Texture2D? _skyTexture;
    private SpriteFont? _font;
    
    // Background animation
    private float _bgOffsetX;
    private float _bgOffsetY;
    
    // Layout
    private Rectangle _boardRect;
    private int _cellSize;
    private int _boardSize;
    private Rectangle[] _colorButtonBounds = new Rectangle[ColorCount];
    private Rectangle _resetButtonBounds;
    private Rectangle _quitButtonBounds;
    private int _lastViewportWidth;
    private int _lastViewportHeight;
    
    // Colors
    private static readonly Color BorderBlue = new(0x0C, 0x02, 0x75);       // #0c0275
    private static readonly Color TextBlue = new(0x0C, 0x02, 0x75);         // #0c0275
    private static readonly Color LoseRed = new(0xFF, 0x00, 0x00);          // #FF0000
    
    // Animation
    private float _winAnimProgress;
    private float _loseAnimProgress;
    
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
        NewGame(0);
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);
        
        // Create pixel texture
        _pixel = new Texture2D(GraphicsDevice!, 1, 1);
        _pixel.SetData(new[] { Color.White });
        
        // Load font
        _font = content.Load<SpriteFont>("Fonts/Hud");
        
        // Load sky background
        _skyTexture = content.Load<Texture2D>("Games/Assimilate/sky1");
        
        // Set up layout
        SetupLayout();
    }
    
    public override void OnViewportChanged(int width, int height)
    {
        // Will be handled in Update
    }

    private void SetupLayout()
    {
        var viewport = GraphicsDevice!.Viewport;
        LayoutHelper.Update(viewport);
        
        int margin = 20;
        int headerHeight = 80;
        
        if (LayoutHelper.IsPortrait)
        {
            // Portrait: board on top, controls below
            int availableWidth = viewport.Width - margin * 2;
            int availableHeight = viewport.Height - headerHeight - 340;  // More room for larger buttons
            
            _boardSize = Math.Min(availableWidth, availableHeight);
            _boardSize -= _boardSize % GridSize;
            _cellSize = _boardSize / GridSize;
            
            _boardRect = new Rectangle(
                (viewport.Width - _boardSize) / 2,
                headerHeight,
                _boardSize,
                _boardSize
            );
            
            // Color buttons in 3 rows of 2 - larger for mobile
            int btnSize = 85;
            int btnSpacing = 15;
            int controlsY = _boardRect.Bottom + 70;  // Room for Level + Pick text
            int controlsX = (viewport.Width - (2 * btnSize + btnSpacing)) / 2;
            
            int[,] buttonLayout = {
                { 3, 4 },  // White, Red
                { 5, 0 },  // Yellow, Green
                { 2, 1 }   // Blue, Purple
            };
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    int colorIndex = buttonLayout[row, col];
                    _colorButtonBounds[colorIndex] = new Rectangle(
                        controlsX + col * (btnSize + btnSpacing),
                        controlsY + row * (btnSize + btnSpacing),
                        btnSize, btnSize
                    );
                }
            }
            
            _resetButtonBounds = new Rectangle(
                viewport.Width / 2 - 60,
                controlsY + 3 * (btnSize + btnSpacing) + 10,
                120, 40
            );
        }
        else
        {
            // Landscape: board on left, controls on right
            int availableHeight = viewport.Height - headerHeight - 50;
            int availableWidth = (int)(viewport.Width * 0.55f);
            
            _boardSize = Math.Min(availableWidth, availableHeight);
            _boardSize -= _boardSize % GridSize;
            _cellSize = _boardSize / GridSize;
            
            int boardAreaWidth = (int)(viewport.Width * 0.6f);
            _boardRect = new Rectangle(
                (boardAreaWidth - _boardSize) / 2,
                headerHeight + (availableHeight - _boardSize) / 2,
                _boardSize,
                _boardSize
            );
            
            // Color buttons on right side in 3x2 grid
            int btnSize = 80;
            int btnSpacing = 15;
            int controlsX = _boardRect.Right + 50;
            int controlsY = _boardRect.Y + 40;
            
            int[,] buttonLayout = {
                { 3, 4 },  // White, Red
                { 5, 0 },  // Yellow, Green
                { 2, 1 }   // Blue, Purple
            };
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    int colorIndex = buttonLayout[row, col];
                    _colorButtonBounds[colorIndex] = new Rectangle(
                        controlsX + col * (btnSize + btnSpacing),
                        controlsY + row * (btnSize + btnSpacing),
                        btnSize, btnSize
                    );
                }
            }
            
            _resetButtonBounds = new Rectangle(
                controlsX,
                controlsY + 3 * (btnSize + btnSpacing) + 5,
                btnSize * 2 + btnSpacing,
                35
            );
        }
        
        // Quit button in top right
        _quitButtonBounds = new Rectangle(viewport.Width - 50, 10, 40, 40);
    }

    private void NewGame(int level)
    {
        _level = level;
        _moves = BaseMaxMoves - level;
        
        if (level == 0)
        {
            _points = 0;
        }
        
        _board = new Board(GridSize, GridSize, ColorCount);
        _state = GameState.Playing;
        _winAnimProgress = 0;
        _loseAnimProgress = 0;
    }

    private void Play(int colorIndex)
    {
        if (_state != GameState.Playing || _board == null) return;
        if (_moves <= 0) return;
        if (colorIndex == _board.CurrentColor) return;
        
        int cellsChanged = _board.Flood(colorIndex);
        _points += cellsChanged;
        _moves--;
        
        CheckGameState();
    }

    private void CheckGameState()
    {
        if (_board == null) return;
        
        if (_board.IsComplete())
        {
            _state = GameState.Won;
            RecordWin();
        }
        else if (_moves <= 0)
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
        
        if (_points > _stats.HighScore)
        {
            _stats.HighScore = _points;
        }
        
        StatsChanged?.Invoke(_stats);
    }

    private void RecordLoss()
    {
        _stats.TimesPlayed++;
        _stats.LastPlayed = DateTime.UtcNow;
        
        if (_points > _stats.HighScore)
        {
            _stats.HighScore = _points;
        }
        
        StatsChanged?.Invoke(_stats);
    }

    public override void Update(GameTime gameTime)
    {
        InputHelper.Update();
        
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Animate background (scrolling clouds)
        _bgOffsetX += delta * 20f;
        _bgOffsetY -= delta * 40f;
        
        // Check for viewport changes
        if (GraphicsDevice != null)
        {
            var vp = GraphicsDevice.Viewport;
            if (vp.Width != _lastViewportWidth || vp.Height != _lastViewportHeight)
            {
                _lastViewportWidth = vp.Width;
                _lastViewportHeight = vp.Height;
                SetupLayout();
            }
        }
        
        // Update animations
        if (_state == GameState.Won)
        {
            _winAnimProgress = Math.Min(_winAnimProgress + delta * 2f, 1f);
            
            if (_winAnimProgress >= 1f)
            {
                NewGame(_level + 1);
            }
        }
        
        if (_state == GameState.Lost)
        {
            _loseAnimProgress = Math.Min(_loseAnimProgress + delta * 2f, 1f);
        }
        
        // Handle input
        if (_state == GameState.Playing)
        {
            HandlePlayingInput();
        }
        
        // Reset button always active
        if (InputHelper.WasClicked(_resetButtonBounds))
        {
            NewGame(0);
        }
        
        // Quit button
        if (InputHelper.WasClicked(_quitButtonBounds))
        {
            RequestQuit?.Invoke();
        }
    }

    private void HandlePlayingInput()
    {
        // Check color button clicks
        for (int i = 0; i < ColorCount; i++)
        {
            if (InputHelper.WasClicked(_colorButtonBounds[i]))
            {
                Play(i);
                return;
            }
        }
        
        // Check board click
        if (InputHelper.WasClicked(_boardRect) && _board != null)
        {
            var pos = InputHelper.PointerPosition;
            int gridX = (int)((pos.X - _boardRect.X) / _cellSize);
            int gridY = (int)((pos.Y - _boardRect.Y) / _cellSize);
            
            if (gridX >= 0 && gridX < GridSize && gridY >= 0 && gridY < GridSize)
            {
                int clickedColor = _board[gridX, gridY];
                Play(clickedColor);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_pixel == null || _font == null || _board == null || GraphicsDevice == null) return;
        
        var viewport = GraphicsDevice.Viewport;
        
        spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
        
        // Draw scrolling sky background
        DrawBackground(spriteBatch, viewport);
        
        // Draw HUD
        DrawHUD(spriteBatch, viewport);
        
        // Draw board
        DrawBoard(spriteBatch);
        
        // Draw color buttons
        DrawColorButtons(spriteBatch);
        
        // Draw AI hint
        DrawHint(spriteBatch);
        
        // Draw reset button
        DrawResetButton(spriteBatch);
        
        // Draw quit button
        DrawQuitButton(spriteBatch);
        
        // Draw win/lose messages
        if (_state == GameState.Won)
            DrawWinCelebration(spriteBatch, viewport);
        else if (_state == GameState.Lost)
            DrawLoseMessage(spriteBatch, viewport);
        
        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_skyTexture == null) return;
        
        int texW = _skyTexture.Width;
        int texH = _skyTexture.Height;
        
        // Wrap offsets
        int offsetX = ((int)_bgOffsetX % texW + texW) % texW;
        int offsetY = ((int)_bgOffsetY % texH + texH) % texH;
        
        // Tile the background
        for (int y = -offsetY; y < viewport.Height; y += texH)
        {
            for (int x = -offsetX; x < viewport.Width; x += texW)
            {
                spriteBatch.Draw(_skyTexture, new Vector2(x, y), Color.White);
            }
        }
    }

    private void DrawHUD(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_font == null) return;
        
        int margin = 20;
        
        // Score (top left)
        string scoreText = _points.ToString("N0");
        spriteBatch.DrawString(_font, scoreText, new Vector2(margin, 20), TextBlue);
        
        // Moves remaining (large, top center-right)
        string movesText = _moves.ToString();
        var movesSize = _font.MeasureString(movesText);
        float movesScale = 2.5f;
        
        float movesX = LayoutHelper.IsPortrait 
            ? viewport.Width - margin - movesSize.X * movesScale
            : viewport.Width / 2 + 80;
        
        spriteBatch.DrawString(_font, movesText, new Vector2(movesX, 15), TextBlue,
            0f, Vector2.Zero, movesScale, SpriteEffects.None, 0f);
        
        // Level indicator
        string levelText = $"Level {_level + 1}";
        var levelSize = _font.MeasureString(levelText);
        float levelY = LayoutHelper.IsPortrait ? _boardRect.Bottom + 10 : _boardRect.Bottom + 8;
        spriteBatch.DrawString(_font, levelText, 
            new Vector2(_boardRect.X + (_boardRect.Width - levelSize.X) / 2, levelY), 
            TextBlue);
    }

    private void DrawBoard(SpriteBatch spriteBatch)
    {
        if (_board == null || _pixel == null) return;
        
        var currentColor = Board.Colors[_board.CurrentColor];
        
        // Draw glow/shadow
        int glowSize = 6;
        var glowColor = new Color(
            (int)(currentColor.R * 0.3f),
            (int)(currentColor.G * 0.3f),
            (int)(currentColor.B * 0.3f)
        );
        spriteBatch.Draw(_pixel, new Rectangle(
            _boardRect.X - glowSize - 4,
            _boardRect.Y - glowSize - 4,
            _boardRect.Width + (glowSize + 4) * 2,
            _boardRect.Height + (glowSize + 4) * 2
        ), glowColor * 0.5f);
        
        // Draw border (matches current color)
        int borderSize = 4;
        spriteBatch.Draw(_pixel, new Rectangle(
            _boardRect.X - borderSize,
            _boardRect.Y - borderSize,
            _boardRect.Width + borderSize * 2,
            _boardRect.Height + borderSize * 2
        ), currentColor);
        
        // Draw cells
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var cellRect = new Rectangle(
                    _boardRect.X + x * _cellSize,
                    _boardRect.Y + y * _cellSize,
                    _cellSize,
                    _cellSize
                );
                
                Color cellColor = _board.GetColor(x, y);
                
                if (_state == GameState.Won)
                {
                    float pulse = (float)Math.Sin(_winAnimProgress * Math.PI * 4 + x * 0.2f + y * 0.2f);
                    cellColor = Color.Lerp(cellColor, Color.White, pulse * 0.3f * _winAnimProgress);
                }
                
                spriteBatch.Draw(_pixel, cellRect, cellColor);
            }
        }
    }

    private void DrawColorButtons(SpriteBatch spriteBatch)
    {
        if (_pixel == null || _board == null) return;
        
        var currentColor = Board.Colors[_board.CurrentColor];
        var glowColor = new Color(
            (int)(currentColor.R * 0.3f),
            (int)(currentColor.G * 0.3f),
            (int)(currentColor.B * 0.3f)
        );
        bool isDisabled = _state == GameState.Lost;
        
        for (int i = 0; i < ColorCount; i++)
        {
            var bounds = _colorButtonBounds[i];
            var color = isDisabled ? new Color(0x8A, 0x89, 0x89) : Board.Colors[i];
            
            // Glow behind button
            if (!isDisabled)
            {
                int glow = 4;
                DrawFilledCircle(spriteBatch, 
                    bounds.X - glow, bounds.Y - glow,
                    bounds.Width + glow * 2, bounds.Height + glow * 2,
                    glowColor * 0.4f);
            }
            
            // Draw circular button
            DrawFilledCircle(spriteBatch, bounds.X, bounds.Y, bounds.Width, bounds.Height, color);
            
            // Hover highlight
            if (!isDisabled && InputHelper.IsPointerOver(bounds))
            {
                DrawFilledCircle(spriteBatch, 
                    bounds.X + 3, bounds.Y + 3, 
                    bounds.Width - 6, bounds.Height - 6, 
                    Color.White * 0.25f);
            }
        }
    }

    private void DrawFilledCircle(SpriteBatch spriteBatch, int x, int y, int width, int height, Color color)
    {
        if (_pixel == null) return;
        
        int cx = x + width / 2;
        int cy = y + height / 2;
        int radius = width / 2;
        
        for (int dy = -radius; dy <= radius; dy++)
        {
            int dx = (int)Math.Sqrt(radius * radius - dy * dy);
            spriteBatch.Draw(_pixel, new Rectangle(cx - dx, cy + dy, dx * 2, 1), color);
        }
    }

    private void DrawHint(SpriteBatch spriteBatch)
    {
        if (_font == null || _board == null || _state != GameState.Playing) return;
        
        int bestMove = _board.GetBestMove();
        string hintText = $"Pick: {Board.ColorNames[bestMove]}";
        
        var hintSize = _font.MeasureString(hintText);
        float hintX, hintY;
        
        if (LayoutHelper.IsPortrait)
        {
            hintX = (GraphicsDevice!.Viewport.Width - hintSize.X) / 2;
            hintY = _boardRect.Bottom + 38;  // Below Level text
        }
        else
        {
            hintX = _colorButtonBounds[3].X;
            hintY = _colorButtonBounds[3].Y - 30;
        }
        
        spriteBatch.DrawString(_font, hintText, new Vector2(hintX, hintY), TextBlue);
    }

    private void DrawResetButton(SpriteBatch spriteBatch)
    {
        if (_font == null) return;
        
        string resetText = "Reset";
        var textSize = _font.MeasureString(resetText);
        var textPos = new Vector2(
            _resetButtonBounds.X + (_resetButtonBounds.Width - textSize.X) / 2,
            _resetButtonBounds.Y + (_resetButtonBounds.Height - textSize.Y) / 2
        );
        
        Color textColor = new Color(0xC7, 0x24, 0x24);
        if (InputHelper.IsPointerOver(_resetButtonBounds))
        {
            textColor = Color.Red;
        }
        
        spriteBatch.DrawString(_font, resetText, textPos, textColor);
    }

    private void DrawQuitButton(SpriteBatch spriteBatch)
    {
        if (_pixel == null) return;
        
        // Simple X button
        Color iconColor = Color.Gray;
        if (InputHelper.IsPointerOver(_quitButtonBounds))
        {
            iconColor = Color.White;
        }
        
        // Draw background
        spriteBatch.Draw(_pixel, _quitButtonBounds, Color.Black * 0.3f);
        
        // Draw X
        int padding = 12;
        int thickness = 3;
        var bounds = _quitButtonBounds;
        
        // Draw X lines using rectangles (diagonal approximation)
        for (int i = 0; i < bounds.Width - padding * 2; i++)
        {
            int x1 = bounds.X + padding + i;
            int y1 = bounds.Y + padding + i;
            int x2 = bounds.X + bounds.Width - padding - i;
            int y2 = bounds.Y + padding + i;
            
            spriteBatch.Draw(_pixel, new Rectangle(x1, y1, thickness, thickness), iconColor);
            spriteBatch.Draw(_pixel, new Rectangle(x2 - thickness, y2, thickness, thickness), iconColor);
        }
    }

    private void DrawWinCelebration(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_font == null) return;
        
        string winText = "Level Complete!";
        var textSize = _font.MeasureString(winText);
        float scale = 1f + _winAnimProgress * 0.3f;
        
        var pos = new Vector2(
            viewport.Width / 2 - textSize.X * scale / 2,
            viewport.Height / 2 - textSize.Y * scale / 2
        );
        
        spriteBatch.DrawString(_font, winText, pos + new Vector2(2, 2), Color.Black * _winAnimProgress,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_font, winText, pos, Color.Gold * _winAnimProgress,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawLoseMessage(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (_font == null) return;
        
        // "You didn't make it!" (matching original)
        string loseText = "You didn't make it!";
        var textSize = _font.MeasureString(loseText);
        
        var pos = new Vector2(
            viewport.Width / 2 - textSize.X / 2,
            30
        );
        
        spriteBatch.DrawString(_font, loseText, pos, LoseRed * _loseAnimProgress);
        
        // Final score
        string scoreText = $"Final Score: {_points:N0}";
        var scoreSize = _font.MeasureString(scoreText);
        var scorePos = new Vector2(
            viewport.Width / 2 - scoreSize.X / 2,
            viewport.Height / 2
        );
        spriteBatch.DrawString(_font, scoreText, scorePos, TextBlue * _loseAnimProgress);
        
        // Level reached
        string levelText = $"Reached Level {_level + 1}";
        var levelSize = _font.MeasureString(levelText);
        var levelPos = new Vector2(
            viewport.Width / 2 - levelSize.X / 2,
            viewport.Height / 2 + 30
        );
        spriteBatch.DrawString(_font, levelText, levelPos, TextBlue * _loseAnimProgress);
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
