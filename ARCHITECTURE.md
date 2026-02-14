# Zara's Basement - Game Hub Architecture

A mobile and desktop game hub in the style of old flash game websites. A cozy basement where players can discover and play a collection of arcade-style mini-games.

## Project Structure

```
ZarasBasement/
├── ZarasBasement.Core/          # Shared game logic
│   ├── Games/                   # Mini-game implementations
│   │   ├── Assimilate/          # Flood-it puzzle game
│   │   │   ├── AssimilateGame.cs
│   │   │   └── Board.cs
│   │   ├── GameInfo.cs          # Game metadata
│   │   ├── GameRegistry.cs      # Available games registry
│   │   ├── GameStats.cs         # Per-game statistics
│   │   └── IMinigame.cs         # Mini-game interface
│   ├── Hub/                     # Main hub screen
│   │   ├── HubScreen.cs         # Basement environment
│   │   └── Station.cs           # Clickable game launchers
│   ├── Input/
│   │   └── InputHelper.cs       # Unified mouse/touch input
│   ├── Screens/                 # Screen management
│   │   ├── IScreen.cs
│   │   ├── Screen.cs            # Base screen class
│   │   ├── ScreenManager.cs     # Screen stack & transitions
│   │   └── ScreenTransition.cs
│   ├── Services/
│   │   └── SaveManager.cs       # JSON persistence
│   ├── UI/                      # UI components
│   │   ├── Button.cs            # 90s-style beveled button
│   │   └── QuitButton.cs        # Game quit button
│   └── Content/                 # Shared assets
├── ZarasBasement.DesktopGL/     # Windows/macOS/Linux
├── ZarasBasement.Android/       # Android
└── ZarasBasement.iOS/           # iOS
```

## Architecture

### Screen Management
- **ScreenManager**: Maintains a stack of screens with push/pop/switch operations
- **Transitions**: Fade transitions between screens (expandable to slides, etc.)
- **IScreen**: Interface for all screens (Initialize, LoadContent, Update, Draw, OnEnter, OnExit)

### Mini-game System
- **IMinigame**: Interface extending IScreen with game metadata and stats
- **GameRegistry**: Static registry of available games with factory functions
- **GameStats**: Per-game statistics (high scores, play count, wins)
- **SaveManager**: JSON-based persistence in platform-appropriate location

### Hub Design
- Visual basement environment with brick walls and wooden floor
- **Stations**: Clickable objects that launch games
  - Arcade cabinets, computer monitors, wall posters
  - Show game title and high score
  - Glow/highlight on hover
- Tooltip system showing game description and stats

### UI Pattern
- 90s Windows-style beveled buttons
- Chunky "QUIT" button in corner of all games
- Consistent visual language across hub and games

## Current Games

### Assimilate! (Flood-it)
- 14×14 grid of colored cells
- 6 colors, 25 moves maximum
- Goal: Fill entire board with one color
- Scoring: Points based on moves remaining

## Adding New Games

1. Create folder in `Games/YourGame/`
2. Implement `IMinigame` interface
3. Register in `Zara's BasementGame.RegisterGames()`:

```csharp
var info = new GameInfo
{
    Id = "your-game",
    Title = "Your Game",
    Description = "Short description",
    ThumbnailPath = "Games/YourGame/thumbnail",
    Difficulty = 3,
    EstimatedMinutes = 5,
    Tags = new[] { "Arcade", "Puzzle" }
};
GameRegistry.Register(info, () => new YourGame());
```

## Future Enhancements

### Games to Add
- [ ] Snake clone
- [ ] Breakout/Arkanoid
- [ ] Memory match
- [ ] Simple platformer
- [ ] Endless runner
- [ ] Rhythm game

### Hub Features
- [ ] Custom basement art assets
- [ ] Ambient sounds/music
- [ ] Unlockable decorations
- [ ] Achievement system
- [ ] Settings screen (volume, language)

### Technical
- [ ] Cloud save sync
- [ ] Leaderboards
- [ ] Daily challenges
- [ ] Game thumbnails as textures

## Build & Run

```bash
# Desktop (macOS/Windows/Linux)
cd ZarasBasement.DesktopGL
dotnet run

# Build only
dotnet build

# Run tests (when added)
dotnet test
```

## Platform Notes

- **Desktop**: 1280×720 window, mouse input
- **Mobile**: Full screen, touch input, virtual gamepad support planned
- **Persistence**: `~/.local/share/ZarasBasement/save.json` (varies by platform)
