# Candy Crush — Unity Edition

An advanced **C# / Unity** remake of the original console C++ Candy Crush game. The original C++ source in `Candy-Crush-Cpp/` is **unchanged**.

## What's New in Unity

| Feature | Original C++ | Unity Version |
|---------|----------------|---------------|
| Platform | Windows console | Cross-platform (PC, Mac, WebGL, Mobile) |
| Architecture | Single procedural file | OOP with managers & UI layers |
| Input | Shift + arrow keys | Click-to-swap (mouse/touch ready) |
| Swaps | Always costs a turn | Invalid swaps are rejected |
| Animation | Text flash only | Smooth swap, fall, and destroy animations |
| Special candies | None | Striped, wrapped, color bomb |
| Combos | Basic cascade | Combo multiplier scoring |
| Particles | None | Match particle bursts |
| Audio | None | Music + SFX hooks |
| Scores | `SCOREBOARD.txt` file | Persistent high-score list (PlayerPrefs) |
| Pause | None | ESC to pause/resume |

## Requirements

- **Unity 2022.3 LTS** or newer
- TextMeshPro (installed automatically on first open)

## Quick Start

1. Open **Unity Hub** => Add =>** select the `Unity-CandyCrush` folder.
2. When Unity opens, import TMP Essentials if prompted.
3. In the menu bar, click **Candy Crush => Setup Full Project**.
4. Open `Assets/Scenes/MainGame.unity` and press **Play**.

## Game Modes (from original C++)

### Easy Mode
- 8×8 board, 5 candy colors
- 60 second timer, 15 moves
- Target score: **500**

### Hard Mode
- 10×10 board, 7 candy colors
- 40 second timer, 15 moves
- Target score: **800**

## How to Play

1. Enter your name on the main menu.
2. Choose **Easy** or **Hard** mode.
3. Click a candy, then click an adjacent candy to swap.
4. Match 3+ candies of the same color to score.
5. Reach the target score before time or moves run out.
6. Press **ESC** to pause.

### Special Candies

- **4 in a row** → Striped candy (clears a row or column)
- **5 in a row** → Color bomb (clears all of one color)
- **L-shape match** → Wrapped candy (3×3 explosion)

## Project Structure

```
Unity-CandyCrush/
├── Assets/
│   ├── Editor/
│   │   └── CandyCrushSetup.cs      # One-click scene setup
│   ├── Scripts/
│   │   ├── Core/                   # Board, candy, match, input
│   │   ├── Data/                   # ScriptableObject configs
│   │   ├── Systems/                # Audio, save data
│   │   └── UI/                     # Menu, HUD, panels
│   ├── Resources/                  # Auto-generated config assets
│   ├── Prefabs/                    # Candy prefab
│   └── Scenes/                     # MainGame scene
└── Packages/manifest.json
```

## Customization

Edit `Assets/Resources/GameConfig.asset` to tune:
- Board size, timer, moves, target score
- Point values per match type
- Animation speeds

Edit `Assets/Resources/CandyPalette.asset` to change candy colors and sprites.

## Adding Audio

Assign AudioClips on the **AudioManager** object in the scene:
- Menu music, game music
- Match, swap, invalid swap, combo, victory, game over sounds

## Credits

Based on the original C++ console game by:
- Wareesha Ashraf (22F-3441)

NUCES-FAST, FSD — Programming Fundamentals

Unity conversion adds modern match-3 mechanics while preserving the original difficulty settings.
