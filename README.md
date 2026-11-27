# 2048 — Unity Puzzle Game

A Unity & C# implementation of the classic **2048** puzzle game featuring a clean grid system, smooth tile merging, localization support, audio, and menu/settings management.

## ✨ Features
- **Core 2048 Mechanics:** Grid-based movement, tile spawning (`Spawner`), and merging logic (`MergeController`).
- **Score System:** Live score tracking via `ScoreManager`.
- **UI & Game Flow:** `MainMenu` and `GameScene` with pause (`PauseController`) and settings (`SettingsController`) menus.
- **Localization:** Multi-language UI support (`LocalizationManager` & `LocalizedText`).
- **Audio:** Background music support (`BackgroundMusic`).

## 🛠️ Tech Stack
- **Engine:** Unity (URP 2D)
- **Language:** C#
- **UI:** TextMesh Pro

## 📁 Project Structure
- `Assets/Scenes/` — `MainMenu.unity`, `GameScene.unity`
- `Assets/Script/` — Core gameplay, grid, input, localization, audio, and UI controllers

## 🚀 Getting Started
1. Clone the repository:
   ```bash
   git clone https://github.com/Msdtek/2048-game-unity.git
   ```
2. Open the project in **Unity Hub**.
3. Open `Assets/Scenes/MainMenu.unity` and press **Play**.