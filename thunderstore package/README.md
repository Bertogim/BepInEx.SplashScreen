![Loading preview](https://raw.githubusercontent.com/Bertogim/BepInEx.SplashScreen/refs/heads/main/example.png)

This is a cool mod if you want to disable the console and have a visible loading bar! It's also very useful for tracking progress when loading large modpacks.

## Custom image
Add a file named `LoadingImage.png` inside the plugins folder, the loading screen will scan all the plugins folder, or if you are making a modpack and want to override any mod loading image just place your `LoadingImage.png` in `BepInEx/patchers/Bertogim-LoadingScreen`

## About the fork
BepInEx.SplashScreen.GUI.exe is renamed to LoadingScreen.GUI.exe to try to fix discord activity detecting the loading screen as other game

This fork is only tested with Lethal Company but it should work with other games

## Got issues or feedback?
Feel free to open an issue at https://github.com/Bertogim/BepInEx.SplashScreen/issues

# BepInEx Loading Progress Splash Screen
A BepInEx patcher that displays a loading screen on game startup with information about patchers and plugins being currently loaded. It's best suited for games where patchers and plugins take a long time to initialize.

This patcher is mostly meant for inclusion in modpacks to give end-users immediate feedback after starting a heavily modded game. It can sometimes take a long time for the game window to appear and/or become responsive - especially on slow systems - which can be interpretted by the user as the game crashing.

The patcher and GUI app have evolved from a very old version of [risk-of-thunder/BepInEx.GUI](https://github.com/risk-of-thunder/BepInEx.GUI), though at this point most of the code has been rewritten and this version works in all games. That being said, if you are modding Risk Of Rain 2, use risk-of-thunder/BepInEx.GUI for a better experience.

> 🛠️ This is a fork of the original project [BepInEx/BepInEx.SplashScreen](https://github.com/BepInEx/BepInEx.SplashScreen), with possible modifications or repackaging by the forking author.

## How to use
1. Install [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.15 or later, or 6.0.0-be.674 or later (works on both mono and IL2CPP).
2. Download latest release for your BepInEx version.
3. Extract the release so that the patcher files end up inside `BepInEx\patchers`.
4. You should now see the splash screen appear on game start-up, assuming BepInEx is configured properly.

### Splash screen doesn't appear
1. Make sure that `LoadingScreen.GUI.exe` and `BepInEx.SplashScreen.Patcher.dll` are both present inside the `BepInEx\patchers` folder.
2. Check if the splash screen isn't disabled in `BepInEx\config\BepInEx.cfg`. If you can't see this file or the SplashScreen Enable setting, it means either BepInEx isn't correctly configured or this patcher is failing to start for some reason.
3. Update BepInEx 5 to latest version and make sure that it is running.
4. If the splash screen still does not appear, check the game log for any errors or exceptions. You can report issues on [GitHub](https://github.com/Bertogim/BepInEx.SplashScreen/issues).

## Contributing
Feel free to start issues, and by all means submit some PRs! Contributions should be submitted to the repository at https://github.com/Bertogim/BepInEx.SplashScreen.

You can discuss changes and talk with other modders on the [official BepInEx Discord server](https://discord.gg/MpFEDAg).

## Compiling
Clone the repository and open the .sln with Visual Studio 2022 (with .NET desktop development and .NET 3.5 development tools installed). Hit `Build Solution` and it should just work.
