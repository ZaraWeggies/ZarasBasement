# Zara's Basement 🎮

A mobile and desktop game hub in the style of classic flash game websites. A cozy basement where players can discover and play a collection of arcade-style mini-games.

![MonoGame](https://img.shields.io/badge/MonoGame-3.8-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Platforms](https://img.shields.io/badge/platforms-Windows%20%7C%20macOS%20%7C%20Linux%20%7C%20Android%20%7C%20iOS-green)

## Features

- **Interactive Hub** - Navigate a pixel-art basement with clickable game stations
- **Mini-game Collection** - Growing library of quick, fun games
- **Cross-Platform** - Play on desktop or mobile
- **Persistent Stats** - High scores and play counts saved locally
- **90s Aesthetic** - Chunky buttons, retro vibes

## Current Games

### Assimilate! 🎨
A flood-it/drench style puzzle game. Fill the entire board with one color in 25 moves or less!

- 14×14 grid with 6 colors
- Score based on remaining moves
- Track high scores and wins

## Building & Running

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- For Android: `dotnet workload install android`
- For iOS: `dotnet workload install ios` (macOS only)

### Desktop (Windows/macOS/Linux)

```bash
cd ZarasBasement.DesktopGL
dotnet run
```

### Android

```bash
# Install Android SDK dependencies (first time only)
cd ZarasBasement.Android
dotnet build -t:InstallAndroidDependencies -f net8.0-android \
  -p:AcceptAndroidSDKLicenses=true

# Build debug APK
dotnet build

# Build release APK
dotnet build -c Release
# Output: bin/Release/net8.0-android/com.zarasbasement-Signed.apk

# Install on connected device
dotnet build -t:Install -c Release
```

### iOS (macOS only)

```bash
cd ZarasBasement.iOS
dotnet build

# Run on simulator
dotnet build -t:Run
```

## Project Structure

```
ZarasBasement/
├── ZarasBasement.Core/          # Shared game logic
│   ├── Games/                   # Mini-game implementations
│   │   └── Assimilate/          # Flood-it puzzle game
│   ├── Hub/                     # Main hub screen
│   ├── Screens/                 # Screen management
│   ├── Services/                # Save system
│   └── UI/                      # UI components
├── ZarasBasement.DesktopGL/     # Desktop build
├── ZarasBasement.Android/       # Android build
└── ZarasBasement.iOS/           # iOS build
```

## Adding New Games

1. Create a folder in `ZarasBasement.Core/Games/YourGame/`
2. Implement the `IMinigame` interface
3. Register in `ZarasBasementGame.cs`:

```csharp
var info = new GameInfo
{
    Id = "your-game",
    Title = "Your Game",
    Description = "A fun game!",
    ThumbnailPath = "Games/YourGame/thumbnail",
    Difficulty = 3,
    Tags = new[] { "Arcade" }
};
GameRegistry.Register(info, () => new YourGame());
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed documentation.

## Controls

- **Desktop**: Mouse click to interact
- **Mobile**: Tap to interact
- **Escape/Back**: Exit (from hub) or quit game

## Save Data Location

- **Windows**: `%LOCALAPPDATA%\ZarasBasement\save.json`
- **macOS/Linux**: `~/.local/share/ZarasBasement/save.json`
- **Android**: App internal storage
- **iOS**: App documents directory

## Tech Stack

- [MonoGame 3.8](https://www.monogame.net/) - Cross-platform game framework
- .NET 8.0
- C# 12

## License

MIT License - feel free to use this as a template for your own game hub!

## Roadmap

- [ ] More mini-games (Snake, Breakout, Memory Match)
- [ ] Sound effects and music
- [ ] Achievements system
- [ ] Custom basement decorations
- [ ] Leaderboards

---

Made with ❤️ by Zara
